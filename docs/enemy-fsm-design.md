# Enemy FSM 设计与实现

状态：DAY_16 已实现并于 2026-09-05 完成验收；工程行为通过，语言理解部分验证。

## 当前行为与改动意图

`EnemyController` 现以 Idle / Chase / Attack / Dead 四状态运行；原 `ContactDamage` 已改名为 `EnemyAttack`，碰撞进入伤害被主动范围攻击替代。`EnemyHealth` 管理生命、通知 Dead 并最终销毁；`WaveSpawner` 继续负责配置检查、生成、目标注入、波次和玩家死亡后的停机。

Idle 采用无边界随机游走，每隔配置的 `IdleTime` 更换方向；这是本人确认的当前玩法，不限制在出生点半径内。三类敌人的追击速度、伤害、冷却和阈值由各自 `EnemyStatsConfig` 提供，运行时状态与计时器保留在每个敌人实例中。

## 状态与转移

以下是本人手绘图与讨论补充的文字整理，不另制作重复转移表。`d` 表示敌人与玩家距离，`enterRange < leaveRange`；具体数值尚未决定。

- **Idle**：无边界慢速随机游走，按 `IdleTime` 更换方向；不负责主动攻击。
- **Chase**：朝玩家快速移动，复用现有追踪计算；满足攻击条件后才进入 Attack。
- **Attack**：由 `EnemyAttack` 主动范围查询并调用玩家扣血，一次攻击结束后返回 Chase；首次进入范围允许立即攻击，之后才受 cooldown 限制。跨组件 `FixedUpdate` 的默认执行顺序可能带来最多一个物理帧的移动停顿，当前规模接受该取舍。
- **Dead**：停止行动并取消尚未结算的攻击；无返回存活状态的边。已结算伤害不回滚。

已确认的边：

- Idle → Chase：`d < enterRange`。
- Chase → Idle：`d >= leaveRange`。
- Chase → Attack：`d < attackRange`，且距上次攻击的时间 `>= attackInterval`。
- Attack → Chase：本次攻击执行完毕。
- Idle / Chase / Attack → Dead：`health <= 0`，死亡优先于普通行为和转移。

等于触发距离时，Idle 不进入追击；等于脱离距离时，Chase 退出追击。两个追击阈值之间，Idle/Chase 保持原状态，不因追击距离条件相互切换；Chase 的攻击判定仍独立进行。未列出的敌人状态边视为非法边。

初始化不增加一个长期运行状态：先检查依赖，再按距离选择初态；严格小于 `IdleToChaseDistance` 进入 Chase，等于或大于时进入 Idle。运行中两个追击阈值之间保持当前状态。

## 全局规则与组件职责

- **缺少初始目标就快速失败**：Spawner 启动时检查所用 Prefab、必需组件与场景玩家引用；实例的 `Initialize` 检查注入目标，失败报错、不启动 FSM。Prefab 模板允许 target 为空。当前项目没有单独销毁玩家而保留敌人的路径，运行中目标失效防护暂不扩展。
- **玩家死亡全部停机**：停止生成，现存敌人停止移动和攻击，取消未结算攻击；这不是敌人死亡，不进入 Dead，也不退回巡逻。
- **生命与 FSM 分工**：`EnemyHealth` 保留血量、归零判断和最终销毁职责；拟在销毁前通知 `EnemyController`，由统一切换入口进入 Dead 并终止行为。具体通知接口尚未实现，不依赖对象销毁后下一帧再轮询死亡。
- **停止行为必须真实生效**：当前 Spawner 只禁用 `EnemyController`；若未来存在独立组件、协程或延迟回调，必须确保这些攻击不会在停机后继续执行。

## 实现方案决定

已选择 **enum + switch**，转移判断和统一切换入口先留在 `EnemyController`，明确调用 exit / entry action。枚举定义可单独放 `EnemyState.cs`；当前状态、冷却等运行字段属于每个敌人实例，不做共享静态数据。

- **enum + switch**：当前成本最低，复用组件生命周期、便于追踪调用顺序；状态增加后容易形成大类。独立测试是否方便取决于条件判断是否与 Unity 操作混在一起。
- **普通 C# state class**：有利于拆分复杂行为和复用；需要明确上下文依赖，由控制器主动调用更新与进入/退出方法。依赖设计好才利于独立测试，并非拆文件就自动解耦。待状态代码明显膨胀或真实复用需求出现时再考虑。
- **ScriptableObject state asset**：方便通过资产组合与复用行为，但增加资产、引用及运行上下文管理成本；共享资产不能混放各敌人的计时器等状态，资产生命周期也不等于敌人的状态切换。当前不需要这层配置能力。

## BLU-16 实际实现

1. `EnemyState.cs`：定义 Idle / Chase / Attack / Dead，不增加状态类框架。
2. `EnemyController.cs`：持有实例状态、移动与 cooldown，读取配置，验证依赖和参数关系，并通过统一入口切换状态；Dead 是终态。
3. `EnemyAttack.cs`：在 Attack 状态按配置范围查询 `PlayerHealth`、结算一次伤害、重置 cooldown 并返回 Chase；保留原脚本 GUID，使三份 Prefab 引用连续。
4. `EnemyHealth.cs`：生命归零后先通知 FSM 进入 Dead，再由 Health 保持唯一销毁职责。
5. `WaveSpawner.cs`：无需修改，继续承担生成、目标注入、清波和玩家死亡后的停机。

## 当前参数

| 类型 | Chase / Idle 速度 | 伤害 | cooldown | 攻击范围 | Idle→Chase | Chase→Idle | 生命 |
|---|---|---:|---:|---:|---:|---:|---:|
| FastFragile | 5 / 3 | 12 | 0.8 | 2 | 4 | 5 | 90 |
| Normal | 4 / 2 | 18 | 1.0 | 3 | 5 | 6 | 150 |
| Heavy | 3 / 1.5 | 24 | 1.2 | 2 | 5 | 7 | 420 |

以上速度是本人运行后有意调整的玩法参数，不是旧 `moveSpeed` 的意外回归。

## 验证结果

- 本人完成最终手动回归：Fast / Normal 的 12 / 18 伤害、无旧碰撞叠加、敌人死亡与波次继续、玩家死亡停机、R 重开、Console 无红色错误。
- AI 静态复核三份配置与 Prefab 引用、脚本 GUID、状态转移、最新程序集时间和 Editor 日志；`git diff --check` 通过。
- 攻击范围硬编码占位符曾导致配置范围与查询范围不一致，本人在验收中识别并改为读取配置。
- 双阈值与 Dead 终态通过 Human-first 预测检查；LC-07 对 enum、引用、继承和组合通过，接口、抽象状态类与 concrete state 术语保持 PARTIAL。
- 没有自动化测试或本日媒体证据；精确边界主要通过代码追踪和手工冒烟验证，不夸大为完整状态级测试。

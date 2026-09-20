# 玩家投射物

2026-09-20：TestArena 中按 Q 向范围内最近的存活敌人发射能量弹。选择目标只决定发射瞬间的方向；扣血发生在实际接触敌人时，子弹不会追踪移动目标。

## 调用与状态

`PlayerShoot` 读取 BoltShoot 配置，在冷却结束时查询 Collider，筛选带 EnemyHealth 且 CurrentHealth > 0 的对象，遍历保留最近目标。无目标不发射、不消耗冷却；开局可发射。Instantiate 返回新对象后调用 ArrowAction.Initialize。

`ArrowAction` 保存每枚子弹的方向、速度、伤害、最大距离与起点，内部归一化方向并调整朝向。无效参数会报错并销毁实例。FixedUpdate 通过 Transform 位移实现直线飞行，超过最大距离销毁。碰到 EnemyHealth 扣血并请求销毁；忽略玩家与其他投射物，碰到其余碰撞体销毁。

玩法允许同一物理步内收到的多个敌人接触回调分别结算伤害，未增加 hasHit 单目标限制。当前敌人使用单 Collider；不保证多 Collider 去重，也未实现确定性的同时接触排序。Prefab 使用 Kinematic Rigidbody2D、Continuous 和 Trigger BoxCollider2D；这些设置不等于已证明任意速度均不漏判。

PlayerHealth 死亡时禁用 PlayerShoot；已生成的子弹继续完成自身生命周期。Health 分别管理各自血量与死亡，EnemyHealth 负责目标资格，IDamageable 不负责敌我筛选。

## 当前参数

| 参数 | 值 |
|---|---:|
| 伤害 | 30 |
| 索敌半径 | 20 |
| 冷却 | 0.1 秒 |
| 速度 | 10 单位/秒 |
| 最大飞行距离 | 20 |

前三项来自 BoltShoot.asset，后两项保存在场景 PlayerShoot 组件中；每次发射复制到实例。近战独立配置 NormalAttack 当前为伤害 30、范围 20、冷却 1 秒。原 PlayerSkill 配置改名保留 GUID。

拿到 n 个候选后，最近目标扫描时间 O(n)、额外空间 O(1)；计入物理查询返回的数组，额外空间 O(n)。距离相等时保留后遍历到的目标，不保证稳定次序。当前组件查找要求 EnemyHealth 和 Collider 位于同一个 GameObject。

## 验证与限制

- 作者确认运行验证通过，包括死亡后 Q 无效、R 重开恢复、无目标不发射、打空清理和最近目标选择。
- Editor.log 中观察到 ArrowAction 调用 EnemyHealth 扣血、敌人死亡及全部波次完成。
- 存活筛选补丁及获批命名润色后，主 C# 项目编译成功，0 warning / 0 error；GUID 与场景引用已静态核对。
- AI 未独立复跑最终 Play Mode；高速薄墙、同时多目标及多 Collider 没有逐项专项测试证据；未新增自动化测试或可执行构建。
- 当前发射配置校验尚未统一覆盖缺失 Prefab、缺失组件和速度异常停机，作为后续健壮性工作保留。

实现由作者在教程和 AI API/代码示例辅助下完成，AI 补充存活筛选并做获批命名润色。美术由 imagegen 生成，参见 [素材记录](../media/source/day25-art-notes.md)，不计为手绘或动画成果。

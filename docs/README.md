# Docs

这里保存对外可展示的工程文档。

## Current Documents

- [Combat 系统现状与取舍](combat-system.md)：四条技能调用链、状态归属、扩展成本及已认可但尚未实施的组件组合方案。
- [范围减速](area-slow.md)：刷新规则、生命周期、测试参数和验证边界。

- [Enemy FSM Design](enemy-fsm-design.md)：状态与转移及方案选择；Idle / Chase / Attack / Dead 已接入工程。
- [Player Projectile](player-projectile.md)：最近存活目标查询、直线飞行、命中规则、配置与验证范围。
- [Combat Architecture](combat-architecture-options.md)：战斗组件职责与接口取舍。
- [Data-Driven Enemy Configuration](data-driven-enemy-configuration.md)：说明敌人配置、Prefab、逻辑组件与运行实例状态的边界，并记录 HeavyEnemy 的零 C# 扩展示例。
- [Grid A* Pathfinding](grid-a-star-pathfinding.md)：记录有限网格、A* 输入输出契约、数据结构、复杂度、测试入口和后续场景适配边界。

建议后续文件：

- `architecture.md`
- `enemy-ai-and-pathfinding.md`
- `performance.md`
- `testing.md`
- `ai-workflow.md`
- `interview-notes.md`

文档必须对应真实代码、测试或运行证据。不要把学习计划中的目标当成已经完成的项目成果。

# Tactical Roguelite Systems Lab

这是 8 周项目驱动学习系统的真实工程仓库。目标是通过一个小而完整的 Unity 6.x 2D tactical roguelite/action game，展示可迁移的软件工程能力，而不是只展示 Unity 熟悉度。

## Repository Layout

```text
tactical-roguelite/
  README.md
  .gitignore
  docs/
  unity-game/
  cpp-systems-lab/
  media/
```

| Path | Purpose |
|---|---|
| `unity-game/` | Unity 6.x 2D 主项目。用 Unity Hub 创建项目时选择这个目录 |
| `cpp-systems-lab/` | FSM、A*、Object Pool、Event System、Spatial Partitioning 小实验 |
| `docs/` | 架构、性能、测试、AI workflow 和面试说明 |
| `media/` | 截图、GIF、Demo video、Profiler capture 等作品集证据 |

## Current Status

- Unity project: Unity 6000.3.22f1 project present under `unity-game/`
- `TestArena`：玩家移动、近战、自动索敌投射物、血量显示、死亡停机及 R 重开。
- 敌人：三种配置、Idle / Chase / Attack / Dead 状态机、网格 A* 绕障与路径线显示；波次生成与清波推进。
- C++ systems lab：[独立 C++17 A*](cpp-systems-lab/astar/README.md) 已实现，历史测试 7/7、CTest 1/1 通过；本次未重跑。
- 最新进度：投射物主流程由作者完成运行验证；最终 C# 编译 0 warning / 0 error。改名后的 Unity Play Mode 未独立复验，高速薄墙专项尚无逐项证据。
- 当前交付为编辑器原型，尚未提供可执行游戏版本。角色和环境 AI 素材已导入但尚未替换场景，投射物素材已接入。
- [投射物实现与验证范围](docs/player-projectile.md) · [工程文档](docs/README.md) · [AI 素材来源](media/source/day25-art-notes.md)

## Unity Project Setup

克隆仓库后，用 Unity Hub 打开已有项目：

1. 安装 Unity 6000.3.22f1。
2. 在 Unity Hub 选择添加磁盘中的项目，定位本仓库的 `unity-game/`，无需重新创建项目。
3. 等待依赖解析和资源导入，打开 `Assets/_Game/Scenes/TestArena.unity`，点击 Play。
4. WASD 移动，Space 近战，Q 向最近敌人发射，死亡后 R 重开。

项目目录包含：

```text
unity-game/
  Assets/
  Packages/
  ProjectSettings/
```

不要在 `unity-game/` 内再次 `git init`。Git repo 根目录已经是 `tactical-roguelite/`。

## Git Rule

提交 Unity 项目时应跟踪：

- `unity-game/Assets/`
- `unity-game/Packages/`
- `unity-game/ProjectSettings/`

不要提交：

- `unity-game/Library/`
- `unity-game/Temp/`
- `unity-game/Logs/`
- build cache
- IDE user files


脚本职责：CameraFollow：确保主视角中心永远是玩家本身
EnemyAttack：在敌人进入 Attack 状态时，按配置的攻击范围、伤害和冷却主动攻击玩家
EnemyHealth：管理敌人血量，提供外部修改血量入口，血量总数可调，死亡后销毁对象
Playerattack：负责读取键盘输入，判断是否存在敌人和敌人血量改变入口的调用
Playerhealth：管理玩家血量，血量为0后禁止玩家操作
Enemycontroller：管理敌人的 Idle、Chase、Attack、Dead 状态及对应移动，相关参数可调
PlayerController：玩家可以控制主对象移动，速度可调
PlayerRestartController：玩家血量为0后按R重启游戏
WaveSpawner：比较复杂，我已经自己写过完整需求了。大体上就是控制波次生成
PlayerHealthDisplay：ui设置
PlayerShoot：读取 Q 输入、维护独立冷却、选择最近存活敌人并生成投射物
ArrowAction：初始化方向和参数、直线飞行、触发命中及距离结束

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
- Player movement: BLU-01 engineering complete in `TestArena`; runtime checklist and screenshot recorded
- C++ systems lab: scaffold only
- GitHub remote: `origin` configured and `main` synchronized through `1e4545c`
- Current baseline target: BLU-02；BLU-01 remains completed history
- Detected Unity editor: `D:\Unity\Editors\6000.3.22f1`
- Latest feature commit: `1e4545c feat: add basic player movement`

## Unity Project Setup

用 Unity Hub 创建项目时：

1. 选择 Unity 6.x。
2. 选择 2D 模板。
3. Project name 使用 `unity-game`。
4. Location 选择本仓库根目录：`D:\学习计划\engineering\tactical-roguelite`。
5. 创建后应出现：

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

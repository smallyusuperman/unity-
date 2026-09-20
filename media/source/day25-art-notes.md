# DAY_25 游戏素材

2026-09-19 使用内置 imagegen 生成。用户指出首次玩家俯拍视角不适合游戏，最终采用正面、完整身体、紧凑比例的静态角色。旧俯拍玩家和甲虫草图未导入工程；甲虫新版生成两次被工具拦截，敌人改为简单史莱姆。

文件位于 `unity-game/Assets/_Game/Art/`：

| 文件 | 用途 | 导入最大尺寸 | PPU |
|---|---|---:|---:|
| Characters/player-front.png | 正面蓝衣玩家 | 256 | 1254 |
| Enemies/slime-normal.png | 红色圆形普通敌人 | 256 | 1254 |
| Enemies/slime-fast.png | 琥珀色细长快速敌人 | 256 | 1254 |
| Enemies/slime-heavy.png | 紫色宽体石甲重型敌人 | 256 | 1254 |
| Environment/stone-floor.png | 灰石地面底图 | 512 | 313.5 |
| Environment/stone-block.png | 正面石块障碍 | 256 | 1254 |

PNG 原图均为 1254×1254；Unity 在导入时限制纹理尺寸，文件仍保留原始细节。PPU 按源图像素设置，因此角色和石块画布约为 1×1 世界单位，地面约为 4×4。透明留白使可见轮廓略小于画布。角色大小、脚底对齐和 Collider 需要接入场景时再调；显示大小不能直接当成碰撞范围。

角色与石块是透明 PNG，地面是不透明 PNG。单 Sprite、Bilinear、关闭 mipmap。地面以单张底图交付，虽生成时要求可平铺，但尚未验证拼接接缝。尚未替换场景或 Prefab，没有动画或运行验收。

## 生图提示词摘要

- 玩家：front-facing full-body compact chibi adventurer, visible face, blue hood and cyan trim, flat camera, clean cartoon shading, transparent background; no overhead perspective.
- 普通敌人：front-facing round rust-red jelly slime, two eyes, scalloped base, simple outlined cartoon sprite, transparent background.
- 快速敌人：same slime style, golden amber, slender teardrop silhouette and swept tip, front view, transparent background.
- 重型敌人：same slime style, broad squat purple body, large rounded stone armor plates, front view, transparent background.
- 地面：low-contrast desaturated gray stone paving, orthographic surface, edge-to-edge opaque texture, no props or vignette.
- 障碍：single squat rectangular gray stone block, front face with narrow top surface, simple shading, transparent background.

以上是提示词摘要，完整请求见本次对话的 imagegen 调用。素材使用 AI 生成，不计为学生手绘成果。

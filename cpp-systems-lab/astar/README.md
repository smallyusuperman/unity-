# C++ A* Lab

本目录把 Unity 项目中已经验证的 C# A* 迁移为一个独立的 C++17 命令行实验。

## 当前状态

自由函数版本已经实现，使用固定零起点网格、`std::priority_queue`、按坐标索引的最佳代价表和 parent 索引表。它不依赖 Unity。

## 文件职责

- `include/pathfinder.h`：公开坐标、队列节点、比较器和寻路 API。
- `src/pathfinder.cpp`：A* 实现。
- `tests/pathfinder_tests.cpp`：自包含命令行测试和测试入口。
- `CMakeLists.txt`：C++17、编译警告、构建目标和 CTest 入口。

## API

```cpp
bool InitializePathfinder(
    int width,
    int height,
    const std::vector<Vector2DInt>& obstaclesInput);

bool TryFindPath(
    const Vector2DInt& start,
    const Vector2DInt& goal,
    std::vector<Vector2DInt>& outPath);
```

调用者先提供宽、高和障碍初始化网格，再进行一次或多次搜索。成功时 `outPath` 包含起点和终点；失败时返回 `false` 并清空 `outPath`。

## 实现要点

- `Vector2DInt`、`Node` 和队列元素使用值语义。
- 网格配置由模块保存；每次搜索的 open set、`bestGCosts` 和 `parentIndices` 都是函数局部状态。
- `std::priority_queue` 按最低 `f = g + h` 排序，`f` 相同时优先较低 `h`。
- 找到更低的 `g` 时重新压入队列；弹出后将节点的 `g` 与 `bestGCosts` 比较，跳过 stale entry。
- parent 保存前一个格子的一维索引，不保存指向队列或 `vector` 元素的裸指针。
- 固定四邻接、单位代价、Manhattan 启发函数。

## 构建

先进入 Visual Studio Build Tools 开发者环境，再从 `cpp-systems-lab` 运行：

```powershell
cmake -S astar -B astar/build
cmake --build astar/build --config Debug
ctest --test-dir astar/build -C Debug --output-on-failure
```

成功结果应为 `7 tests, 0 failures`，CTest 应显示 100% tests passed。

## 测试

- 起点等于终点，返回单点路径。
- 空 3×3 网格，最短路为 4 步。
- 5×5 三格竖墙，最短绕行为 8 步。
- 完整竖墙使目标不可达。
- 越界、阻挡点和非法障碍输入安全失败。
- 同一配置连续执行失败、成功和反向成功。
- 同一坐标的更优队列项先处理，旧项可识别为 stale entry。

成功路径还会统一验证首尾、相邻步长、边界和障碍。

## C# / C++ 对照

| C# 实现 | C++ 实现 |
|---|---|
| `Vector2Int` | `Vector2DInt` 值类型 |
| `List<Node>` 线性查最低 `f` | `std::priority_queue<Node, ..., CompareNode>` |
| `Node Parent` 引用 | `parentIndices` 中的稳定整数索引 |
| `bool + out List`，失败为 `null` | `bool + vector&`，失败时清空 vector |
| `openList` / `closedList` 是对象字段 | 搜索状态是 `TryFindPath` 的局部变量 |
| GC 管理节点对象 | 标准容器离开作用域自动释放资源 |

## 复杂度与限制

设可达格子数为 `V`，邻接边数为 `E`，实际入队条目数为 `Q`。坐标表访问为 `O(1)`，队列 push/pop 为 `O(log Q)`，路径回溯为 `O(L)`。stale entry 会让 `Q` 可能大于 `V`。

当前模块只有一份全局网格配置，不支持同时存在多个 Pathfinder，也不是线程安全接口。实验不处理动态障碍、加权地形、对角移动、局部避障或 Unity 世界坐标。

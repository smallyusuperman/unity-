using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 有限网格上的 A* 寻路核心（纯 C#，不依赖场景对象，可在 EditMode 测试中直接构造）。
/// 四方向移动、单位代价，启发函数为 Manhattan 距离——对四方向网格不会高估，可保证最短路径。
/// open 集合用 List 线性取最小 f，单次寻路最坏约 O(V²)；当前网格规模下优先可读与可测。
/// </summary>
public class Pathfinding
{
    private readonly int widthMin;
    private readonly int heightMin;
    private readonly int widthMax;
    private readonly int heightMax;
    private readonly Vector2Int[] obstaclePositions;

    // 以下私有搜索状态在 TryFindPath 调用期间充当"方法参数"，由各 helper 方法共享。
    // 因此同一个实例不可并发或递归调用；一个网格对应一个实例。
    private Vector2Int goalPosition;
    private Vector2Int startPosition;
    private List<Node> openList;
    private List<Node> closedList;

    /// <summary>A* 搜索节点：位置、父节点与 g/h 代价，f = g + h。</summary>
    public class Node
    {
        public Vector2Int Position;
        public Node Parent;

        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;

        /// <summary>按 Manhattan 距离估算到终点的剩余代价；四方向单位代价下不会高估。</summary>
        public void CalculateHCost(Vector2Int goal)
        {
            HCost =
                Mathf.Abs(Position.x - goal.x)
                + Mathf.Abs(Position.y - goal.y);
        }
    }

    public Pathfinding(
        int widthMin,
        int heightMin,
        int widthMax,
        int heightMax,
        Vector2Int[] obstaclePositions)
    {
        this.widthMin = widthMin;
        this.heightMin = heightMin;
        this.widthMax = widthMax;
        this.heightMax = heightMax;
        this.obstaclePositions = obstaclePositions;
    }

    private bool IsConfigurationValid()
    {
        if (widthMin >= widthMax || heightMin >= heightMax)
        {
            Debug.LogError("Invalid grid dimensions.");
            return false;
        }

        if (startPosition.x < widthMin || startPosition.x > widthMax || startPosition.y < heightMin || startPosition.y > heightMax)
        {
            Debug.LogError("Start position is outside the grid boundaries.");
            return false;
        }

        if (goalPosition.x < widthMin || goalPosition.x > widthMax || goalPosition.y < heightMin || goalPosition.y > heightMax)
        {
            Debug.LogError("Destination position is outside the grid boundaries.");
            return false;
        }

        for (int i = 0; i < obstaclePositions.Length; i++)
        {
            Vector2Int obstacle = obstaclePositions[i];
            if (obstacle.x < widthMin || obstacle.x > widthMax || obstacle.y < heightMin || obstacle.y > heightMax)
            {
                Debug.LogError($"Illegal obstacle point {obstacle} is outside the grid boundaries.");
                return false;
            }

            if (obstacle == startPosition || obstacle == goalPosition)
            {
                Debug.LogError("Start or destination position cannot be an illegal obstacle point.");
                return false;
            }
        }

        return true;
    }

    private bool IsWalkable(Vector2Int position)
    {
        if (position.x < widthMin || position.x > widthMax || position.y < heightMin || position.y > heightMax)
        {
            return false;
        }

        foreach (Vector2Int obstacle in obstaclePositions)
        {
            if (position == obstacle)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 求一条同时包含起点与终点的最短路径。
    /// 网格配置非法（越界、起点或终点落在障碍上）或路径不可达时，返回 false 并把 path 置为 null。
    /// </summary>
    public bool TryFindPath(
        Vector2Int startPosition,
        Vector2Int goalPosition,
        out List<Vector2Int> path)
    {
        this.startPosition = startPosition;
        this.goalPosition = goalPosition;
        path = null;

        if (!IsConfigurationValid())
        {
            Debug.LogError("Grid setup is invalid.");
            return false;
        }

        InitializeSearch();
        while (openList.Count > 0)
        {
            Node currentNode = FindLowestFCostNode();
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (IsGoal(currentNode))
            {
                path = ReconstructPath(currentNode);
                return true;
            }

            Node[] neighbors = new Node[4];
            InitializeNeighbors(currentNode, neighbors);

            foreach (Node neighbor in neighbors)
            {
                if (!IsWalkable(neighbor.Position) || closedList.Exists(n => n.Position == neighbor.Position))
                {
                    continue;
                }

                Node openNode = openList.Find(n => n.Position == neighbor.Position);
                if (openNode == null)
                {
                    openList.Add(neighbor);
                }
                else if (neighbor.GCost < openNode.GCost)
                {
                    openNode.GCost = neighbor.GCost;
                    openNode.Parent = currentNode;
                }
            }
        }

        return false;
    }

    private List<Vector2Int> ReconstructPath(Node destinationNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = destinationNode;

        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private void InitializeSearch()
    {
        Node startNode = new Node();

        startNode.Position = startPosition;
        startNode.GCost = 0;
        startNode.CalculateHCost(goalPosition);
        startNode.Parent = null;

        openList = new List<Node>();
        closedList = new List<Node>();

        openList.Add(startNode);
    }

    private Node FindLowestFCostNode()
    {
        Node lowestFCostNode = openList[0];
        foreach (Node node in openList)
        {
            if (node.FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    private bool IsGoal(Node node)
    {
        return node.Position == goalPosition;
    }

    private void InitializeNeighbors(Node currentNode, Node[] neighbors)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int i = 0; i < neighbors.Length; i++)
        {
            neighbors[i] = new Node();
            neighbors[i].Position = currentNode.Position + directions[i];
            neighbors[i].Parent = currentNode;
            neighbors[i].GCost = currentNode.GCost + 1;
            neighbors[i].CalculateHCost(goalPosition);
        }
    }
}

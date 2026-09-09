using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private readonly int widthMin;
    private readonly int heightMin;
    private readonly int widthMax;
    private readonly int heightMax;
    private readonly Vector2Int[] obstaclePositions;

    private Vector2Int goalPosition;
    private Vector2Int startPosition;
    private List<Node> openList;
    private List<Node> closedList;

    public class Node
    {
        public Vector2Int Position;
        public Node Parent;

        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;

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
    /// Finds a shortest path that includes both endpoints. Returns false with a null path when no valid path exists.
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
                Debug.Log("Destination reached!");
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

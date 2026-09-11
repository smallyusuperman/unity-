using System.Collections.Generic;
using UnityEngine;

public class ScenePathfindingGrid : MonoBehaviour
{
    private class GridStructure
    {
        public Vector2 Origin;
        public float CellSize;
        public Vector2Int[] Obstacles;

        public int WidthMax;
        public int HeightMax;
        public int WidthMin;
        public int HeightMin;

    }

    private GridStructure gridStructure;
    private Pathfinding pathfinding;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (gridStructure != null && pathfinding != null)
        {
            return;
        }

        gridStructure = InitializeGrid();
        pathfinding = new Pathfinding(
            gridStructure.WidthMin,
            gridStructure.HeightMin,
            gridStructure.WidthMax,
            gridStructure.HeightMax,
            gridStructure.Obstacles);
    }

    private GridStructure InitializeGrid()
    {
        return new GridStructure
        {
            Origin = Vector2.zero,
            CellSize = 1f,
            Obstacles = new Vector2Int[]
            {
                new Vector2Int(3, 1),
                new Vector2Int(3, 2),
                new Vector2Int(3, 3),
                new Vector2Int(4, 1),
                new Vector2Int(4, 2),
                new Vector2Int(4, 3)
            },
            WidthMin = -9,
            WidthMax = 9,
            HeightMin = -3,
            HeightMax = 3
        };
    }

    public Vector2Int WorldToCell(Vector2 worldPosition)
    {
        EnsureInitialized();

        Vector2 localPosition = worldPosition - gridStructure.Origin;

        return new Vector2Int(
            Mathf.RoundToInt(localPosition.x / gridStructure.CellSize),
            Mathf.RoundToInt(localPosition.y / gridStructure.CellSize));
    }

    public Vector2 CellToWorld(Vector2Int cellPosition)
    {
        EnsureInitialized();

        return gridStructure.Origin
            + new Vector2(
                cellPosition.x * gridStructure.CellSize,
                cellPosition.y * gridStructure.CellSize);
    }

    private bool IsInsideGrid(Vector2Int cellPosition)
    {
        return cellPosition.x >= gridStructure.WidthMin
            && cellPosition.x <= gridStructure.WidthMax
            && cellPosition.y >= gridStructure.HeightMin
            && cellPosition.y <= gridStructure.HeightMax;
    }

    public bool TryGetWaypoints(
        Vector2 currentWorldPosition,
        Vector2 targetWorldPosition,
        out List<Vector2> waypoints)
    {
        waypoints = null;

        Vector2Int currentCell = WorldToCell(currentWorldPosition);
        Vector2Int targetCell = WorldToCell(targetWorldPosition);

        if (!IsInsideGrid(currentCell) || !IsInsideGrid(targetCell))
        {
            Debug.LogError(
                "One or both pathfinding positions are outside the grid.",
                this);
            return false;
        }

        if (!pathfinding.TryFindPath(
                currentCell,
                targetCell,
                out List<Vector2Int> cellPath))
        {
            return false;
        }

        waypoints = new List<Vector2>(cellPath.Count);

        foreach (Vector2Int cell in cellPath)
        {
            waypoints.Add(CellToWorld(cell));
        }

        return true;
    }
}

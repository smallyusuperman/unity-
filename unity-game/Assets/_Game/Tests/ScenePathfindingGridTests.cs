using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ScenePathfindingGridTests
{
    private GameObject gridObject;
    private ScenePathfindingGrid grid;

    [SetUp]
    public void SetUp()
    {
        gridObject = new GameObject("PathfindingGridTest");
        grid = gridObject.AddComponent<ScenePathfindingGrid>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gridObject);
    }

    [Test]
    public void WorldToCell_AtOrigin_ReturnsOriginCell()
    {
        Vector2Int cell = grid.WorldToCell(Vector2.zero);

        Assert.That(cell, Is.EqualTo(Vector2Int.zero));
    }

    [Test]
    public void WorldToCell_WithNegativePosition_RoundsToNearestCell()
    {
        Vector2Int cell = grid.WorldToCell(new Vector2(-2.4f, -1.6f));

        Assert.That(cell, Is.EqualTo(new Vector2Int(-2, -2)));
    }

    [Test]
    public void CellConversion_RoundTrip_PreservesCell()
    {
        Vector2Int originalCell = new Vector2Int(-9, 3);

        Vector2 worldPosition = grid.CellToWorld(originalCell);
        Vector2Int convertedCell = grid.WorldToCell(worldPosition);

        Assert.That(convertedCell, Is.EqualTo(originalCell));
    }

    [Test]
    public void TryGetWaypoints_AroundConfiguredObstacle_ReturnsWalkableWorldPath()
    {
        Vector2Int[] obstacles =
        {
            new Vector2Int(3, 1),
            new Vector2Int(3, 2),
            new Vector2Int(3, 3),
            new Vector2Int(4, 1),
            new Vector2Int(4, 2),
            new Vector2Int(4, 3)
        };

        bool found = grid.TryGetWaypoints(
            new Vector2(7f, 3f),
            Vector2.zero,
            out List<Vector2> waypoints);

        Assert.That(found, Is.True);
        Assert.That(waypoints, Is.Not.Null);
        Assert.That(waypoints[0], Is.EqualTo(new Vector2(7f, 3f)));
        Assert.That(waypoints[waypoints.Count - 1], Is.EqualTo(Vector2.zero));

        foreach (Vector2 waypoint in waypoints)
        {
            CollectionAssert.DoesNotContain(
                obstacles,
                grid.WorldToCell(waypoint));
        }
    }

    [Test]
    public void TryGetWaypoints_WhenPositionIsOutsideGrid_ReturnsFalseAndNull()
    {
        LogAssert.Expect(
            LogType.Error,
            "One or both pathfinding positions are outside the grid.");

        bool found = grid.TryGetWaypoints(
            new Vector2(10f, 0f),
            Vector2.zero,
            out List<Vector2> waypoints);

        Assert.That(found, Is.False);
        Assert.That(waypoints, Is.Null);
    }
}

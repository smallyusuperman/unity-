using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PathfindingTests
{
    [Test]
    public void TryFindPath_WhenStartEqualsGoal_ReturnsSinglePointPath()
    {
        Vector2Int[] obstacles = new Vector2Int[0];

        Pathfinding pathfinding = new Pathfinding(
            0,
            0,
            2,
            2,
            obstacles);

        bool found = pathfinding.TryFindPath(
            new Vector2Int(1, 1),
            new Vector2Int(1, 1),
            out List<Vector2Int> path);

        Assert.That(found, Is.True);
        Assert.That(path, Is.Not.Null);
        Assert.That(path.Count, Is.EqualTo(1));
        Assert.That(path[0], Is.EqualTo(new Vector2Int(1, 1)));
        AssertPathIsValid(path, 0, 0, 2, 2, obstacles);
    }

    [Test]
    public void TryFindPath_OnEmptyGrid_ReturnsShortestPath()
    {
        Vector2Int[] obstacles = new Vector2Int[0];

        Pathfinding pathfinding = new Pathfinding(
            0,
            0,
            2,
            2,
            obstacles);

        bool found = pathfinding.TryFindPath(
            new Vector2Int(0, 0),
            new Vector2Int(2, 2),
            out List<Vector2Int> path);

        Assert.That(found, Is.True);
        Assert.That(path, Is.Not.Null);
        Assert.That(path[0], Is.EqualTo(new Vector2Int(0, 0)));
        Assert.That(path[path.Count - 1], Is.EqualTo(new Vector2Int(2, 2)));
        Assert.That(path.Count - 1, Is.EqualTo(4));
        AssertPathIsValid(path, 0, 0, 2, 2, obstacles);
    }

    [Test]
    public void TryFindPath_WhenGoalIsBlocked_ReturnsFalse()
    {
        Vector2Int[] obstacles =
        {
            new Vector2Int(2, 2)
        };

        Pathfinding pathfinding = new Pathfinding(
            0,
            0,
            2,
            2,
            obstacles);

        LogAssert.Expect(LogType.Error, "Start or destination position cannot be an illegal obstacle point.");

        LogAssert.Expect(LogType.Error, "Grid setup is invalid.");

        bool found = pathfinding.TryFindPath(
            new Vector2Int(0, 0),
            new Vector2Int(2, 2),
            out List<Vector2Int> path);

        Assert.That(found, Is.False);
        Assert.That(path, Is.Null);
    }

    [Test]
    public void TryFindPath_WhenDirectRouteIsBlocked_FindsShortestDetour()
    {
        Vector2Int[] obstacles =
        {
            new Vector2Int(2, 1),
            new Vector2Int(2, 2),
            new Vector2Int(2, 3)
        };

        Pathfinding pathfinding = new Pathfinding(
            0,
            0,
            4,
            4,
            obstacles);

        bool found = pathfinding.TryFindPath(
            new Vector2Int(0, 2),
            new Vector2Int(4, 2),
            out List<Vector2Int> path);

        Assert.That(found, Is.True);
        Assert.That(path, Is.Not.Null);
        Assert.That(path[0], Is.EqualTo(new Vector2Int(0, 2)));
        Assert.That(path[path.Count - 1], Is.EqualTo(new Vector2Int(4, 2)));
        Assert.That(path.Count - 1, Is.EqualTo(8));
        AssertPathIsValid(path, 0, 0, 4, 4, obstacles);
    }

    [Test]
    public void TryFindPath_WhenGoalIsUnreachable_ReturnsFalse()
    {
        Vector2Int[] obstacles =
        {
            new Vector2Int(2, 0),
            new Vector2Int(2, 1),
            new Vector2Int(2, 2),
            new Vector2Int(2, 3),
            new Vector2Int(2, 4)
        };

        Pathfinding pathfinding = new Pathfinding(
            0,
            0,
            4,
            4,
            obstacles);

        bool found = pathfinding.TryFindPath(
            new Vector2Int(0, 2),
            new Vector2Int(4, 2),
            out List<Vector2Int> path);

        Assert.That(found, Is.False);
        Assert.That(path, Is.Null);
    }

    [Test]
    public void TryFindPath_WhenCalledRepeatedly_DoesNotKeepOldSearchState()
    {
        Vector2Int[] obstacles = new Vector2Int[0];

        Pathfinding pathfinding = new Pathfinding(
            0,
            0,
            2,
            2,
            obstacles);

        LogAssert.Expect(
            LogType.Error,
            "Start position is outside the grid boundaries.");

        LogAssert.Expect(
            LogType.Error,
            "Grid setup is invalid.");

        bool firstFound = pathfinding.TryFindPath(
            new Vector2Int(-1, 0),
            new Vector2Int(2, 2),
            out List<Vector2Int> firstPath);

        bool secondFound = pathfinding.TryFindPath(
            new Vector2Int(0, 0),
            new Vector2Int(2, 2),
            out List<Vector2Int> secondPath);

        bool thirdFound = pathfinding.TryFindPath(
            new Vector2Int(2, 2),
            new Vector2Int(0, 0),
            out List<Vector2Int> thirdPath);

        Assert.That(firstFound, Is.False);
        Assert.That(firstPath, Is.Null);

        Assert.That(secondFound, Is.True);
        Assert.That(secondPath.Count - 1, Is.EqualTo(4));
        AssertPathIsValid(secondPath, 0, 0, 2, 2, obstacles);

        Assert.That(thirdFound, Is.True);
        Assert.That(thirdPath.Count - 1, Is.EqualTo(4));
        AssertPathIsValid(thirdPath, 0, 0, 2, 2, obstacles);
    }

    [Test]
    public void TryFindPath_WhenStartIsBlocked_ReturnsFalse()
    {
        Vector2Int[] obstacles =
        {
            new Vector2Int(0, 0)
        };

        Pathfinding pathfinding = new Pathfinding(
            0,
            0,
            2,
            2,
            obstacles);

        LogAssert.Expect(
            LogType.Error,
            "Start or destination position cannot be an illegal obstacle point.");

        LogAssert.Expect(
            LogType.Error,
            "Grid setup is invalid.");

        bool found = pathfinding.TryFindPath(
            new Vector2Int(0, 0),
            new Vector2Int(2, 2),
            out List<Vector2Int> path);

        Assert.That(found, Is.False);
        Assert.That(path, Is.Null);
    }

    private static void AssertPathIsValid(
        List<Vector2Int> path,
        int widthMin,
        int heightMin,
        int widthMax,
        int heightMax,
        Vector2Int[] obstacles)
    {
        Assert.That(path, Is.Not.Null);

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int point = path[i];
            Assert.That(point.x, Is.GreaterThanOrEqualTo(widthMin));
            Assert.That(point.x, Is.LessThanOrEqualTo(widthMax));
            Assert.That(point.y, Is.GreaterThanOrEqualTo(heightMin));
            Assert.That(point.y, Is.LessThanOrEqualTo(heightMax));
            CollectionAssert.DoesNotContain(obstacles, point);

            if (i == 0)
            {
                continue;
            }

            Vector2Int step = point - path[i - 1];
            int manhattanStep = Mathf.Abs(step.x) + Mathf.Abs(step.y);
            Assert.That(manhattanStep, Is.EqualTo(1));
        }
    }
}

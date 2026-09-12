#include <algorithm>
#include <cstdlib>
#include <exception>
#include <iostream>
#include <queue>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

#include "pathfinder.h"

namespace
{
using TestFunction = void (*)();

void Require(bool condition, const std::string& message)
{
    if (!condition)
    {
        throw std::runtime_error(message);
    }
}

bool ContainsPosition(
    const std::vector<Vector2DInt>& positions,
    const Vector2DInt& target)
{
    return std::find(positions.begin(), positions.end(), target)
        != positions.end();
}

void RequirePathIsValid(
    const std::vector<Vector2DInt>& path,
    int width,
    int height,
    const std::vector<Vector2DInt>& obstacles)
{
    Require(!path.empty(), "successful path must not be empty");

    for (std::size_t index = 0; index < path.size(); ++index)
    {
        const Vector2DInt& position = path[index];
        Require(position.x >= 0 && position.x < width, "path x is outside grid");
        Require(position.y >= 0 && position.y < height, "path y is outside grid");
        Require(!ContainsPosition(obstacles, position), "path crosses an obstacle");

        if (index == 0)
        {
            continue;
        }

        const Vector2DInt& previous = path[index - 1];
        const int stepDistance = std::abs(position.x - previous.x)
            + std::abs(position.y - previous.y);
        Require(stepDistance == 1, "path contains a non-adjacent step");
    }
}

void TestStartEqualsGoal()
{
    const std::vector<Vector2DInt> obstacles{};
    Require(InitializePathfinder(3, 3, obstacles), "grid initialization failed");

    std::vector<Vector2DInt> path{};
    const bool found = TryFindPath(Vector2DInt{1, 1}, Vector2DInt{1, 1}, path);

    Require(found, "start equals goal should succeed");
    Require(path.size() == 1, "start equals goal should return one point");
    Require(path.front() == Vector2DInt{1, 1}, "single point is incorrect");
    RequirePathIsValid(path, 3, 3, obstacles);
}

void TestEmptyThreeByThree()
{
    const std::vector<Vector2DInt> obstacles{};
    Require(InitializePathfinder(3, 3, obstacles), "grid initialization failed");

    std::vector<Vector2DInt> path{};
    const bool found = TryFindPath(Vector2DInt{0, 0}, Vector2DInt{2, 2}, path);

    Require(found, "empty grid should have a path");
    Require(path.front() == Vector2DInt{0, 0}, "path start is incorrect");
    Require(path.back() == Vector2DInt{2, 2}, "path goal is incorrect");
    Require(path.size() - 1 == 4, "empty grid path should have four steps");
    RequirePathIsValid(path, 3, 3, obstacles);
}

void TestFiveByFiveDetour()
{
    const std::vector<Vector2DInt> obstacles{
        Vector2DInt{2, 1},
        Vector2DInt{2, 2},
        Vector2DInt{2, 3}};
    Require(InitializePathfinder(5, 5, obstacles), "grid initialization failed");

    std::vector<Vector2DInt> path{};
    const bool found = TryFindPath(Vector2DInt{0, 2}, Vector2DInt{4, 2}, path);

    Require(found, "detour grid should have a path");
    Require(path.front() == Vector2DInt{0, 2}, "detour path start is incorrect");
    Require(path.back() == Vector2DInt{4, 2}, "detour path goal is incorrect");
    Require(path.size() - 1 == 8, "detour path should have eight steps");
    RequirePathIsValid(path, 5, 5, obstacles);
}

void TestUnreachableGoal()
{
    const std::vector<Vector2DInt> obstacles{
        Vector2DInt{2, 0},
        Vector2DInt{2, 1},
        Vector2DInt{2, 2},
        Vector2DInt{2, 3},
        Vector2DInt{2, 4}};
    Require(InitializePathfinder(5, 5, obstacles), "grid initialization failed");

    std::vector<Vector2DInt> path{Vector2DInt{9, 9}};
    const bool found = TryFindPath(Vector2DInt{0, 2}, Vector2DInt{4, 2}, path);

    Require(!found, "unreachable goal should fail");
    Require(path.empty(), "failed search should clear the old path");
}

void TestInvalidInput()
{
    const std::vector<Vector2DInt> obstacles{Vector2DInt{2, 2}};
    Require(InitializePathfinder(3, 3, obstacles), "grid initialization failed");

    std::vector<Vector2DInt> path{Vector2DInt{9, 9}};
    Require(
        !TryFindPath(Vector2DInt{-1, 0}, Vector2DInt{1, 1}, path),
        "out-of-bounds start should fail");
    Require(path.empty(), "out-of-bounds failure should clear the old path");

    Require(
        !TryFindPath(Vector2DInt{0, 0}, Vector2DInt{3, 1}, path),
        "out-of-bounds goal should fail");
    Require(
        !TryFindPath(Vector2DInt{0, 0}, Vector2DInt{2, 2}, path),
        "blocked goal should fail");

    Require(
        InitializePathfinder(3, 3, std::vector<Vector2DInt>{Vector2DInt{0, 0}}),
        "blocked-start grid initialization failed");
    Require(
        !TryFindPath(Vector2DInt{0, 0}, Vector2DInt{2, 2}, path),
        "blocked start should fail");

    Require(
        !InitializePathfinder(3, 3, std::vector<Vector2DInt>{Vector2DInt{3, 0}}),
        "out-of-bounds obstacle should reject the grid");
    Require(
        !TryFindPath(Vector2DInt{0, 0}, Vector2DInt{2, 2}, path),
        "search with invalid grid configuration should fail");
}

void TestRepeatedCalls()
{
    const std::vector<Vector2DInt> obstacles{};
    Require(InitializePathfinder(3, 3, obstacles), "grid initialization failed");

    std::vector<Vector2DInt> firstPath{};
    std::vector<Vector2DInt> secondPath{};
    std::vector<Vector2DInt> thirdPath{};

    const bool firstFound = TryFindPath(
        Vector2DInt{-1, 0}, Vector2DInt{2, 2}, firstPath);
    const bool secondFound = TryFindPath(
        Vector2DInt{0, 0}, Vector2DInt{2, 2}, secondPath);
    const bool thirdFound = TryFindPath(
        Vector2DInt{2, 2}, Vector2DInt{0, 0}, thirdPath);

    Require(!firstFound && firstPath.empty(), "first invalid call should fail cleanly");
    Require(secondFound, "second call should succeed");
    Require(thirdFound, "third reverse call should succeed");
    Require(secondPath.size() - 1 == 4, "second path should have four steps");
    Require(thirdPath.size() - 1 == 4, "third path should have four steps");
    RequirePathIsValid(secondPath, 3, 3, obstacles);
    RequirePathIsValid(thirdPath, 3, 3, obstacles);
}

void TestPriorityQueueStaleEntry()
{
    using OpenSet = std::priority_queue<Node, std::vector<Node>, CompareNode>;
    OpenSet openSet{};

    Node oldEntry{Vector2DInt{1, 1}, 5, 2};
    Node betterEntry{Vector2DInt{1, 1}, 3, 2};
    openSet.push(oldEntry);
    openSet.push(betterEntry);

    const int bestKnownGCost = 3;
    const Node currentEntry = openSet.top();
    openSet.pop();
    Require(
        currentEntry.gCost == bestKnownGCost,
        "better entry should be processed first");

    const Node staleEntry = openSet.top();
    openSet.pop();
    Require(
        staleEntry.gCost != bestKnownGCost,
        "old entry should be recognized as stale");
}

int RunTest(const std::string& testName, TestFunction testFunction)
{
    try
    {
        testFunction();
        std::cout << "PASS: " << testName << '\n';
        return 0;
    }
    catch (const std::exception& exception)
    {
        std::cerr << "FAIL: " << testName << " - " << exception.what() << '\n';
        return 1;
    }
}
}

int main()
{
    const std::vector<std::pair<std::string, TestFunction>> tests{
        {"start equals goal", TestStartEqualsGoal},
        {"empty 3x3 shortest path", TestEmptyThreeByThree},
        {"5x5 shortest detour", TestFiveByFiveDetour},
        {"unreachable goal", TestUnreachableGoal},
        {"invalid input", TestInvalidInput},
        {"repeated calls", TestRepeatedCalls},
        {"priority queue stale entry", TestPriorityQueueStaleEntry}};

    int failureCount{};
    for (const auto& test : tests)
    {
        failureCount += RunTest(test.first, test.second);
    }

    std::cout << tests.size() << " tests, " << failureCount << " failures\n";

    return failureCount == 0 ? EXIT_SUCCESS : EXIT_FAILURE;
}

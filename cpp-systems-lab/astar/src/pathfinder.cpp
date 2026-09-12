#include "pathfinder.h"

#include <algorithm>
#include <array>
#include <cstddef>
#include <limits>
#include <queue>

namespace
{
int gridWidth{};
int gridHeight{};
bool configurationIsValid{};
std::vector<bool> blockedCells{};

bool IsInsideGrid(const Vector2DInt& position) noexcept
{
    return position.x >= 0 && position.x < gridWidth
        && position.y >= 0 && position.y < gridHeight;
}

std::size_t IndexOf(const Vector2DInt& position) noexcept
{
    return static_cast<std::size_t>(position.y)
        * static_cast<std::size_t>(gridWidth)
        + static_cast<std::size_t>(position.x);
}

Vector2DInt PositionFromIndex(std::size_t index) noexcept
{
    const std::size_t width = static_cast<std::size_t>(gridWidth);
    return Vector2DInt{
        static_cast<int>(index % width),
        static_cast<int>(index / width)};
}

bool IsWalkable(const Vector2DInt& position)
{
    return IsInsideGrid(position) && !blockedCells[IndexOf(position)];
}

bool ReconstructPath(
    const std::vector<int>& parentIndices,
    const Vector2DInt& start,
    const Vector2DInt& goal,
    std::vector<Vector2DInt>& outPath)
{
    const std::size_t startIndex = IndexOf(start);
    std::size_t currentIndex = IndexOf(goal);

    while (true)
    {
        outPath.push_back(PositionFromIndex(currentIndex));

        if (currentIndex == startIndex)
        {
            break;
        }

        const int parentIndex = parentIndices[currentIndex];
        if (parentIndex < 0)
        {
            outPath.clear();
            return false;
        }

        currentIndex = static_cast<std::size_t>(parentIndex);
    }

    std::reverse(outPath.begin(), outPath.end());
    return true;
}
}

bool InitializePathfinder(
    int width,
    int height,
    const std::vector<Vector2DInt>& obstaclesInput)
{
    gridWidth = width;
    gridHeight = height;
    configurationIsValid = width > 0 && height > 0;
    blockedCells.clear();

    if (!configurationIsValid)
    {
        return false;
    }

    const std::size_t widthValue = static_cast<std::size_t>(width);
    const std::size_t heightValue = static_cast<std::size_t>(height);
    if (widthValue > std::numeric_limits<std::size_t>::max() / heightValue)
    {
        configurationIsValid = false;
        return false;
    }

    blockedCells.assign(widthValue * heightValue, false);

    for (const Vector2DInt& obstacle : obstaclesInput)
    {
        if (!IsInsideGrid(obstacle))
        {
            configurationIsValid = false;
            blockedCells.clear();
            return false;
        }

        blockedCells[IndexOf(obstacle)] = true;
    }

    return true;
}

bool TryFindPath(
    const Vector2DInt& start,
    const Vector2DInt& goal,
    std::vector<Vector2DInt>& outPath)
{
    outPath.clear();

    if (!configurationIsValid
        || !IsWalkable(start)
        || !IsWalkable(goal))
    {
        return false;
    }

    if (start == goal)
    {
        outPath.push_back(start);
        return true;
    }

    using OpenSet = std::priority_queue<Node, std::vector<Node>, CompareNode>;
    OpenSet openSet{};

    const std::size_t cellCount = blockedCells.size();
    const int unreachableCost = std::numeric_limits<int>::max();
    std::vector<int> bestGCosts(cellCount, unreachableCost);
    std::vector<int> parentIndices(cellCount, -1);

    Node startNode{start, 0, 0};
    startNode.CalculateHCost(goal);
    bestGCosts[IndexOf(start)] = 0;
    openSet.push(startNode);

    constexpr std::array<Vector2DInt, 4> directions{{
        Vector2DInt{0, 1},
        Vector2DInt{0, -1},
        Vector2DInt{-1, 0},
        Vector2DInt{1, 0}}};

    while (!openSet.empty())
    {
        const Node currentNode = openSet.top();
        openSet.pop();

        const std::size_t currentIndex = IndexOf(currentNode.position);
        if (currentNode.gCost != bestGCosts[currentIndex])
        {
            continue;
        }

        if (currentNode.position == goal)
        {
            return ReconstructPath(parentIndices, start, goal, outPath);
        }

        for (const Vector2DInt& direction : directions)
        {
            const Vector2DInt neighborPosition{
                currentNode.position.x + direction.x,
                currentNode.position.y + direction.y};

            if (!IsWalkable(neighborPosition))
            {
                continue;
            }

            const std::size_t neighborIndex = IndexOf(neighborPosition);
            const int tentativeGCost = currentNode.gCost + 1;
            if (tentativeGCost >= bestGCosts[neighborIndex])
            {
                continue;
            }

            bestGCosts[neighborIndex] = tentativeGCost;
            parentIndices[neighborIndex] = static_cast<int>(currentIndex);

            Node neighborNode{neighborPosition, tentativeGCost, 0};
            neighborNode.CalculateHCost(goal);
            openSet.push(neighborNode);
        }
    }

    return false;
}

#pragma once

#include <cstdlib>
#include <vector>

struct Vector2DInt
{
    int x{};
    int y{};

    bool operator==(const Vector2DInt& other) const noexcept
    {
        return x == other.x && y == other.y;
    }
};

struct Node
{
    Vector2DInt position{};
    int gCost{};
    int hCost{};

    int FCost() const noexcept
    {
        return gCost + hCost;
    }

    void CalculateHCost(const Vector2DInt& goal) noexcept
    {
        hCost = std::abs(position.x - goal.x) + std::abs(position.y - goal.y);
    }
};

struct CompareNode
{
    bool operator()(const Node& left, const Node& right) const noexcept
    {
        if (left.FCost() != right.FCost())
        {
            return left.FCost() > right.FCost();
        }

        if (left.hCost != right.hCost)
        {
            return left.hCost > right.hCost;
        }

        if (left.position.y != right.position.y)
        {
            return left.position.y > right.position.y;
        }

        return left.position.x > right.position.x;
    }
};

bool InitializePathfinder(
    int width,
    int height,
    const std::vector<Vector2DInt>& obstaclesInput);

bool TryFindPath(
    const Vector2DInt& start,
    const Vector2DInt& goal,
    std::vector<Vector2DInt>& outPath);

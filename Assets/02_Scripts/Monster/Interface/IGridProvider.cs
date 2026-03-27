using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridProvider
{
    void WorldToGrid(Vector2 world, out int gx, out int gy);
    Vector2 GridToWorld(int gx, int gy);

    bool IsWalkable(int gx, int gy);

    int Width { get; }
    int Height { get; }
}
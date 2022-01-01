using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordsConverter
{
    private const float offsetX = 1.5f;
    private const float offsetY = -0.5f;
    private const float tileH = 0.5f;

    public static Vector2Int FromWorldPointToCell(Vector3 worldPoint)
    {
        int X = (int)(worldPoint.x + worldPoint.y / tileH + offsetX);
        int Y = (int)(worldPoint.x - worldPoint.y / tileH + offsetY);
        return new Vector2Int(X, Y);
    }

    public static Vector3 FromCellToWorldPoint(Vector2Int cell)
    {
        float X = tileH * (cell.x + cell.y);
        float Y = tileH / 2 * (cell.x - cell.y - 1);
        return new Vector3(X, Y, 0);
    }

    public static Vector3 FromCellToWorldPoint(int x, int y)
    {
        float X = tileH * (x + y);
        float Y = tileH / 2 * (x - y - 1);
        return new Vector3(X, Y, 0);
    }
}

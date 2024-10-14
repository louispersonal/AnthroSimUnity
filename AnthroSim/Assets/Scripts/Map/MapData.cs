using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    public MapDataPoint[,] Data;

    public MapData(int width, int height)
    {
        Data = new MapDataPoint[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Data[x, y] = new MapDataPoint();
            }
        }
    }
}

[System.Serializable]
public struct Rectangle
{
    public Rectangle(int x_lo, int x_hi, int y_lo, int y_hi)
    {
        X_lo = x_lo;
        X_hi = x_hi;
        Y_lo = y_lo;
        Y_hi = y_hi;
    }

    public int X_lo;
    public int X_hi;
    public int Y_lo;
    public int Y_hi;
}
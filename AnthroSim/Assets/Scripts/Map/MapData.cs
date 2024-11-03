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
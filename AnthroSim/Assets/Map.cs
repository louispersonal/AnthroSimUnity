using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    int _mapWidth;
    int _mapHeight;

    public MapData MapData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeMap(int width, int height)
    {
        _mapWidth = width;
        _mapHeight = height;
        MapData = new MapData(_mapWidth, _mapHeight);
    }

    public int GetLength(int dimension)
    {
        return MapData.Data.GetLength(dimension);
    }

    public float GetHeight(int x, int y)
    {
        return MapData.Data[x, y].Height;
    }

    public void SetHeight(int x, int y, float heightValue)
    {
        MapData.Data[x, y].Height = heightValue;
    }
}

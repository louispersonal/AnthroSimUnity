using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    int _mapWidth;
    int _mapHeight;

    public MapData MapData;

    public MeshFilter PlaneMesh;

    public MeshRenderer PlaneRenderer;

    public Dictionary<MapModes, Texture2D> Textures;

    [SerializeField]
    MapTextureGenerator _mapTextureGenerator;

    MapModes _currentMapMode;

    // Start is called before the first frame update
    void Start()
    {
        _currentMapMode = MapModes.Normal;
        Textures = new Dictionary<MapModes, Texture2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("ChangeMap"))
        {
            _currentMapMode = (MapModes)(((int)_currentMapMode + 1) % Enum.GetNames(typeof(MapModes)).Length);
            SetMapMode((_currentMapMode));
        }
    }

    public void InitializeMap(int width, int height)
    {
        _mapWidth = width;
        _mapHeight = height;
        MapData = new MapData(_mapWidth, _mapHeight);
    }

    public void SetMapMode(MapModes mode)
    {
        PlaneRenderer.material.mainTexture = Textures[mode];
    }

    public void AddMapMode(MapModes mode)
    {
        if (!Textures.ContainsKey(mode))
        {
            Textures.Add(mode, _mapTextureGenerator.CreateMapTexture(this, mode));
        }
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

public enum MapModes
{
    Normal,
    Temperature,
    Precipitation,
    LowVegetation,
    HighVegetation
}

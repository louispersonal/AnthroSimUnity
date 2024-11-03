using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private MapData _mapData;

    private MeshRenderer _planeRenderer;

    public Dictionary<MapModes, Texture2D> Textures;

    [SerializeField]
    MapTextureGenerator _mapTextureGenerator;

    MapModes _currentMapMode;

    [SerializeField]
    int _kilometersPerPoint;

    public int KilometersPerPoint { get { return _kilometersPerPoint; } }

    // Start is called before the first frame update
    void Start()
    {
        _planeRenderer = GetComponent<MeshRenderer>();

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
        _mapData = new MapData(width, height);
    }

    public void SetMapMode(MapModes mode)
    {
        _planeRenderer.material.mainTexture = Textures[mode];
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
        return _mapData.Data.GetLength(dimension);
    }

    public float GetHeight(int x, int z)
    {
        return _mapData.Data[x, z].Height;
    }

    public float GetHeight(float x, float z)
    {
        float h00 = _mapData.Data[Mathf.FloorToInt(x), Mathf.FloorToInt(z)].Height;
        float h10 = _mapData.Data[Mathf.CeilToInt(x), Mathf.FloorToInt(z)].Height;
        float h01 = _mapData.Data[Mathf.FloorToInt(x), Mathf.CeilToInt(z)].Height;
        float h11 = _mapData.Data[Mathf.CeilToInt(x), Mathf.CeilToInt(z)].Height;

        float h0 = Mathf.Lerp(h00, h10, x - Mathf.FloorToInt(x));
        float h1 = Mathf.Lerp(h01, h11, x - Mathf.FloorToInt(x));

        return Mathf.Lerp(h0, h1, z - Mathf.FloorToInt(z));
    }

    public void SetHeight(int x, int z, float height)
    {
        _mapData.Data[x, z].Height = height;
    }

    public float GetTemperature(int x, int z)
    {
        return _mapData.Data[x, z].Temperature;
    }

    public void SetTemperature(int x, int z, float temperature)
    {
        _mapData.Data[x, z].Temperature = temperature;
    }

    public LandWaterType GetLandWaterType(int x, int z)
    {
        return _mapData.Data[x, z].LandWaterType;
    }

    public void SetLandWaterType(int x, int z, LandWaterType landWaterType)
    {
        _mapData.Data[x, z].LandWaterType = landWaterType;
    }

    public int GetLandWaterFeatureID(int x, int z)
    {
        return _mapData.Data[x, z].LandWaterFeatureID;
    }

    public void SetLandWaterFeatureID(int x, int z, int landwaterFeatureID)
    {
        _mapData.Data[x, z].LandWaterFeatureID = landwaterFeatureID;
    }

    public GeoFeatureType GetGeoFeatureType(int x, int z)
    {
        return _mapData.Data[x, z].GeoFeatureType;
    }

    public void SetGeoFeatureType(int x, int z, GeoFeatureType geoFeatureType)
    {
        _mapData.Data[x, z].GeoFeatureType = geoFeatureType;
    }

    public int GetGeoFeatureID(int x, int z)
    {
        return _mapData.Data[x, z].GeoFeatureID;
    }

    public void SetGeoFeatureID(int x, int z, int geoFeatureID)
    {
        _mapData.Data[x, z].GeoFeatureID = geoFeatureID;
    }

    public float GetWaterProximity(int x, int z)
    {
        return _mapData.Data[x, z].WaterProximity;
    }

    public void SetWaterProximity(int x, int z, float waterProximity)
    {
        _mapData.Data[x, z].WaterProximity = waterProximity;
    }

    public float GetPrecipitation(int x, int z)
    {
        return _mapData.Data[x, z].Precipitation;
    }

    public void SetPrecipitation(int x, int z, float precipitation)
    {
        _mapData.Data[x, z].Precipitation = precipitation;
    }

    public float GetLowVegetation(int x, int z)
    {
        return _mapData.Data[x, z].LowVegetation;
    }

    public void SetLowVegetation(int x, int z, float lowVegetation)
    {
        _mapData.Data[x, z].LowVegetation = lowVegetation;
    }

    public float GetHighVegetation(int x, int z)
    {
        return _mapData.Data[x, z].HighVegetation;
    }

    public void SetHighVegetation(int x, int z, float highVegetation)
    {
        _mapData.Data[x, z].HighVegetation = highVegetation;
    }
}
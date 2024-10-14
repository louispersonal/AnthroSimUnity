using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapDataPoint
{
    public float Height;
    public float Temperature;
    public LandWaterType LandWaterType;
    public int LandWaterFeatureID;
    public GeoFeatureType GeoFeatureType;
    public int GeoFeatureID;
    public float WaterProximity;
    public float LowVegetation;
    public float HighVegetation;

    public MapDataPoint()
    {
        Height = 0f;
        Temperature = 0f;
        LandWaterType = LandWaterType.Ocean;
        LandWaterFeatureID = 0;
        GeoFeatureType = GeoFeatureType.None;
        GeoFeatureID = 0;
        WaterProximity = 0f;
        LowVegetation = 0f;
        HighVegetation = 0f;
    }
}

public enum LandWaterType
{
    Ocean,
    Continent,
    River,
    Lake
}

public enum GeoFeatureType
{
    None,
    Mountain,
    Valley,
    Volcano
}
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
    public float Precipitation;
    public float LowVegetation;
    public float HighVegetation;

    public MapDataPoint()
    {
        Height = 0f;
        Temperature = 0f;
        LandWaterType = LandWaterType.Ocean;
        LandWaterFeatureID = -1;
        GeoFeatureType = GeoFeatureType.None;
        GeoFeatureID = -1;
        WaterProximity = 0f;
        Precipitation = 0f;
        LowVegetation = 0f;
        HighVegetation = 0f;
    }
}
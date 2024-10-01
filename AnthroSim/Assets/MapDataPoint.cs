using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
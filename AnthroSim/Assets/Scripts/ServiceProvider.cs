using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceProvider
{
    private static LandwaterAtlas _landwaterAtlas;
    public static LandwaterAtlas LandwaterAtlas { get { if (_landwaterAtlas == null) { _landwaterAtlas = GameObject.FindObjectOfType<LandwaterAtlas>(); } return _landwaterAtlas; } set { _landwaterAtlas = value; } }

    private static GeoFeatureAtlas _geoFeatureAtlas;
    public static GeoFeatureAtlas GeoFeatureAtlas { get { if (_geoFeatureAtlas == null) { _geoFeatureAtlas = GameObject.FindObjectOfType<GeoFeatureAtlas>(); } return _geoFeatureAtlas; } set { _geoFeatureAtlas = value; } }
}

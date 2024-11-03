using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValleyGenerator
{
    public static void CarveValley(Map map, Vector2Int bottomLocation, int valleyRadius, int valleyID, float valleyDepth)
    {
        // Coefficients for controlling the spread of the parabola
        float a = 1f, b = 1f;
        // Iterate over every element in the 2D array
        for (int y = bottomLocation.y - valleyRadius; y < bottomLocation.y + valleyRadius; y++)
        {
            for (int x = bottomLocation.x - valleyRadius; x < bottomLocation.x + valleyRadius; x++)
            {
                if (map.GetGeoFeatureType(x, y) == GeoFeatureType.None)
                {
                    // Calculate the distance from the center of the peak
                    float dx = ((float)x - (float)bottomLocation.x);
                    float dy = ((float)y - (float)bottomLocation.y);

                    // Apply the parabolic formula
                    float value = valleyDepth + ((a * dx * dx + b * dy * dy) / (valleyRadius * valleyRadius));

                    // Clamp the value between minHeight and maxHeight
                    value = Mathf.Min(map.GetHeight(x, y), Mathf.Min(valleyDepth, value));

                    // Add this peak value to the array
                    map.SetHeight(x, y, value);
                    map.SetGeoFeatureType(x, y, GeoFeatureType.Valley);
                    map.SetGeoFeatureID(x, y, valleyID);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValleyGenerator
{
    public static void CarveValley(Map map, Vector2Int bottomLocation, int valleyRadius, int valleyID, float valleyDepth)
    {
        // Iterate over every element in the 2D array
        for (int y = bottomLocation.y - valleyRadius; y < bottomLocation.y + valleyRadius; y++)
        {
            for (int x = bottomLocation.x - valleyRadius; x < bottomLocation.x + valleyRadius; x++)
            {
                if (map.GetGeoFeatureType(x, y) == GeoFeatureType.None || map.GetGeoFeatureID(x, y) == valleyID)
                {
                    // Calculate the distance from the center of the peak
                    float dx = ((float)x - (float)bottomLocation.x) / (float)valleyRadius;
                    float dy = ((float)y - (float)bottomLocation.y) / (float)valleyRadius;

                    // Apply the parabolic formula
                    float value = CalculateValleyDepthAtPoint(dx, dy, valleyDepth, 0.1f);

                    value = Mathf.Min(map.GetHeight(x, y), value);

                    // Add this peak value to the array
                    map.SetHeight(x, y, value);
                    map.SetGeoFeatureType(x, y, GeoFeatureType.Valley);
                    map.SetGeoFeatureID(x, y, valleyID);
                }
            }
        }
    }

    private static float CalculateValleyDepthAtPoint(float dx, float dy, float deepestPoint, float noiseScale)
    {
        return (1 - noiseScale) * Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2)) + noiseScale * CalculateNoiseAtPoint(dx, dy) - deepestPoint;
    }

    private static float CalculateNoiseAtPoint(float dx, float dy)
    {
        float scale = 4f;

        return Mathf.PerlinNoise(dx * scale, dy * scale);
    }
}

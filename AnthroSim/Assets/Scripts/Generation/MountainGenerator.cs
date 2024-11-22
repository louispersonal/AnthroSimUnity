using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MountainGenerator
{
    public static void AddMountainRange(Map map, Rectangle continentBounds)
    {
        int mountainRadius = Random.Range(GlobalParameters.MinimumMountainRadius, GlobalParameters.MaximumMountainRadius);
        Vector2Int firstMountainPeak = FindMountainPeak(map, continentBounds, mountainRadius);
        int numMountains = 1;
        float peakHeight = 1f;
        AddMountain(map, firstMountainPeak, mountainRadius, peakHeight);
        Vector2 rangeDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rangeDirection = rangeDirection.normalized;
        while (CheckSpaceForMountain(map, firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * mountainRadius)), mountainRadius) && numMountains < GlobalParameters.MaxMountainsPerRange)
        {
            AddMountain(map, firstMountainPeak, mountainRadius, peakHeight);
            numMountains++;
            firstMountainPeak = firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * mountainRadius));
            mountainRadius = Random.Range(GlobalParameters.MinimumMountainRadius, GlobalParameters.MaximumMountainRadius);
            rangeDirection = RandomWalkVector.RotateVector2(rangeDirection, Random.Range(-30f, 30f));
        }
    }

    private static void AddMountain(Map map, Vector2Int peakLocation, int mountainRadius, float peakHeight)
    {
        map.SetHeight(peakLocation.x, peakLocation.y, peakHeight);
        map.SetGeoFeatureType(peakLocation.x, peakLocation.y, GeoFeatureType.Mountain);
        int mountainID = ServiceProvider.GeoFeatureAtlas.GetAvailableMountainID();
        map.SetGeoFeatureID(peakLocation.x, peakLocation.y, mountainID);
        FormMountain2(map, peakLocation, mountainRadius, mountainID, peakHeight);
        if (Random.Range(0f, 1f) < GlobalParameters.ChanceOfRiverSourceOnMountain)
        {
            RiverGenerator.AddRiver(map, peakLocation);
        }
    }

    private static void FormMountain(Map map, Vector2Int peakLocation, int mountainRadius, int mountainID, float peakHeight)
    {
        // Coefficients for controlling the spread of the parabola
        float a = 1.0f, b = 1.0f;

        // Iterate over every element in the 2D array
        for (int y = peakLocation.y - mountainRadius; y < peakLocation.y + mountainRadius; y++)
        {
            for (int x = peakLocation.x - mountainRadius; x < peakLocation.x + mountainRadius; x++)
            {
                // Calculate the distance from the center of the peak
                float dx = ((float)x - (float)peakLocation.x);
                float dy = ((float)y - (float)peakLocation.y);

                // Apply the parabolic formula
                float value = peakHeight - ((a * dx * dx + b * dy * dy) / (mountainRadius * mountainRadius));

                // Clamp the value between minHeight and maxHeight
                value = Mathf.Max(map.GetHeight(x, y), Mathf.Min(peakHeight, value));

                if (value != map.GetHeight(x, y))
                {
                    // Add this peak value to the array
                    map.SetHeight(x, y, value);
                    map.SetGeoFeatureType(x, y, GeoFeatureType.Mountain);
                    map.SetGeoFeatureID(x, y, mountainID);
                }
            }
        }
    }

    private static void FormMountain2(Map map, Vector2Int peakLocation, int mountainRadius, int mountainID, float peakHeight)
    {
        for (int y = peakLocation.y - mountainRadius; y < peakLocation.y + mountainRadius; y++)
        {
            for (int x = peakLocation.x - mountainRadius; x < peakLocation.x + mountainRadius; x++)
            {
                // Calculate the distance from the center of the peak
                float dx = ((float)x - (float)peakLocation.x) / (2 * (float)mountainRadius);
                float dy = ((float)y - (float)peakLocation.y) / (2 * (float)mountainRadius);

                // Apply the parabolic formula
                float value = CalculateMountainHeightAtPoint(dx, dy, peakHeight, 0.1f);

                if (value > map.GetHeight(x, y))
                {
                    // Add this peak value to the array
                    map.SetHeight(x, y, value);
                    map.SetGeoFeatureType(x, y, GeoFeatureType.Mountain);
                    map.SetGeoFeatureID(x, y, mountainID);
                }
            }
        }
    }

    private static float CalculateMountainHeightAtPoint(float dx, float dy, float peakHeight, float noiseScale)
    {
        return peakHeight - ( 1 - noiseScale ) * Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2)) + noiseScale * CalculateNoiseAtPoint(dx, dy);
    }

    private static float CalculateNoiseAtPoint(float dx, float dy)
    {
        float scale = 4f;

        return Mathf.PerlinNoise(dx * scale, dy * scale);
    }

    private static Vector2Int FindMountainPeak(Map map, Rectangle continentBounds, int mountainRadius)
    {
        bool spaceForMountain;
        Vector2Int randomPoint;
        do
        {
            // pick random point
            randomPoint = new Vector2Int(Random.Range(continentBounds.X_lo, continentBounds.X_hi), Random.Range(continentBounds.Y_lo, continentBounds.Y_hi));
            spaceForMountain = CheckSpaceForMountain(map, randomPoint, mountainRadius);
        } while (!spaceForMountain);
        return randomPoint;
    }

    private static bool CheckSpaceForMountain(Map map, Vector2Int peakLocation, int mountainRadius)
    {
        bool roomOnLand = map.GetLandWaterType(peakLocation.x + mountainRadius, peakLocation.y) == LandWaterType.Continent
            && map.GetLandWaterType(peakLocation.x - mountainRadius, peakLocation.y) == LandWaterType.Continent
            && map.GetLandWaterType(peakLocation.x, peakLocation.y + mountainRadius) == LandWaterType.Continent
            && map.GetLandWaterType(peakLocation.x, peakLocation.y - mountainRadius) == LandWaterType.Continent;

        bool noConflictingFeature = map.GetGeoFeatureType(peakLocation.x + mountainRadius, peakLocation.y) == GeoFeatureType.None
            && map.GetGeoFeatureType(peakLocation.x - mountainRadius, peakLocation.y) == GeoFeatureType.None
            && map.GetGeoFeatureType(peakLocation.x, peakLocation.y + mountainRadius) == GeoFeatureType.None
            && map.GetGeoFeatureType(peakLocation.x, peakLocation.y - mountainRadius) == GeoFeatureType.None;

        return roomOnLand && noConflictingFeature;
    }
}

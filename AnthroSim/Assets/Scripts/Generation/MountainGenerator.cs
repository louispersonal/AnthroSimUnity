using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MountainGenerator
{
    private static void AddMountainRange(MapGenerator mapGenerator, Map map, Rectangle continentBounds)
    {
        int mountainRadius = Random.Range(mapGenerator.GenerationParameters._minimumMountainRadius, mapGenerator.GenerationParameters._maximumMountainRadius);
        Vector2Int firstMountainPeak = FindMountainPeak(map, continentBounds, mountainRadius);
        int numMountains = 1;
        float peakHeight = 1f;
        AddMountain(mapGenerator, map, firstMountainPeak, mountainRadius, peakHeight);
        Vector2 rangeDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rangeDirection = rangeDirection.normalized;
        while (CheckSpaceForMountain(map, firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * (mountainRadius * 2))), mountainRadius) && numMountains < mapGenerator.GenerationParameters._maxMountainsPerRange)
        {
            AddMountain(mapGenerator, map, firstMountainPeak, mountainRadius, peakHeight);
            numMountains++;
            firstMountainPeak = firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * (mountainRadius * 2)));
        }
    }

    private static void AddMountain(MapGenerator mapGenerator, Map map, Vector2Int peakLocation, int mountainRadius, float peakHeight)
    {
        map.SetHeight(peakLocation.x, peakLocation.y, peakHeight);
        map.SetGeoFeatureType(peakLocation.x, peakLocation.y, GeoFeatureType.Mountain);
        int mountainID = mapGenerator.GeoFeatureAtlas.GetAvailableMountainID();
        map.SetGeoFeatureID(peakLocation.x, peakLocation.y, mountainID);
        FormMountain(map, peakLocation, mountainRadius, mountainID, peakHeight);
        if (Random.Range(0f, 1f) < mapGenerator.GenerationParameters._chanceOfRiverSourceOnMountain)
        {
            RiverGenerator.AddRiver(mapGenerator, map, peakLocation);
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

                // Add this peak value to the array
                map.SetHeight(x, y, value);
                map.SetGeoFeatureType(x, y, GeoFeatureType.Mountain);
                map.SetGeoFeatureID(x, y, mountainID);
            }
        }
    }

    private static Vector2Int FindMountainPeak(Map map, Rectangle continentBounds, int mountainRadius)
    {
        bool spaceForMountain;
        Vector2Int randomPoint;
        do
        {
            // pick random point
            randomPoint = new Vector2Int(Random.Range(continentBounds.X_lo, continentBounds.X_hi), Random.Range(continentBounds.X_lo, continentBounds.X_hi));
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

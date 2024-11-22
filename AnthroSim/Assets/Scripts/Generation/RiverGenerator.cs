using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RiverGenerator
{
    public static void AddRiver(Map map, Vector2Int sourceLocation)
    {
        Vector2Int riverEndLocation = FindClosestCoastline(map, sourceLocation);
        int riverID = ServiceProvider.LandwaterAtlas.GetAvailableRiverID();

        List<Vector2Int> points = RandomWalkVector.RandomWalk(sourceLocation, riverEndLocation, GlobalParameters.NumRiverVerticesRatio, GlobalParameters.MaxRiverDisplacementAngle);

        int finalValleyWidth = Random.Range(GlobalParameters.MinimumValleyRadius, GlobalParameters.MaximumValleyRadius);
        int valleyID = ServiceProvider.GeoFeatureAtlas.GetAvailableValleyID();
        float valleyWidth = 1;
        float valleySpread = 0.5f;

        foreach (Vector2Int point in points)
        {
            map.SetLandWaterType(point.x, point.y, LandWaterType.River);
            map.SetLandWaterFeatureID(point.x, point.y, riverID);
            ValleyGenerator.CarveValley(map, point, (int)valleyWidth, valleyID, GlobalParameters.SeaLevel);
            if (valleyWidth < finalValleyWidth)
            {
                valleyWidth += valleySpread;
            }
        }
    }

    public static Vector2Int FindClosestCoastline(Map map, Vector2Int point)
    {
        float maxSearchDistance = 499f;
        Vector2Int currentClosestPoint = new Vector2Int(0, 0);
        Vector2Int currentPoint = new Vector2Int(0, 0);
        for (float angle = 0f; angle < 2 * Mathf.PI; angle += Mathf.PI / 6)
        {
            for (float range = 0f; range < maxSearchDistance; range++)
            {
                currentPoint.x = point.x + (int)(range * Mathf.Cos(angle));
                currentPoint.y = point.y + (int)(range * Mathf.Sin(angle));
                if (currentPoint.x < map.GetLength(0) && currentPoint.x >= 0 && currentPoint.y < map.GetLength(1) && currentPoint.y >= 0)
                {
                    if (map.GetLandWaterType(currentPoint.x, currentPoint.y) == LandWaterType.Ocean)
                    {
                        float distanceToCurrent = Vector2Int.Distance(point, currentPoint);
                        float distanceToClosest = Vector2Int.Distance(point, currentClosestPoint);
                        if (distanceToCurrent < distanceToClosest)
                        {
                            currentClosestPoint = currentPoint;
                        }
                    }
                }
            }
        }
        return currentClosestPoint;
    }
}

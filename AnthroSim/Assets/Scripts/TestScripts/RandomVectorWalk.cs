using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RandomVectorWalk : MonoBehaviour
{
    float[,] array;

    GeoFeatureAtlas _geoFeatureAtlas;
    GeoFeatureAtlas GeoFeatureAtlas { get { if (_geoFeatureAtlas == null) { _geoFeatureAtlas = FindObjectOfType<GeoFeatureAtlas>(); } return _geoFeatureAtlas; } set { _geoFeatureAtlas = value; } }

    // Start is called before the first frame update
    void Start()
    {
        /*
        array = new float[100, 100];

        float n = 4f;
        int numPoints = (int)Mathf.Pow(2, n) + 1;
        List<Vector2Int> points = new List<Vector2Int>();
        for (int c = 0; c < numPoints; c++)
        {
            points.Add(new Vector2Int(0, 0));
        }

        AddStartEndPoints(points);

        SubdivideRecursive(points, 0, numPoints - 1, 30);

        InterpolatePoints(array, points);

        WriteArrayToCSV(array, "output.csv");
        */
    }

    public void SubdivideRecursive(List<Vector2Int> list, int start, int end, float maxAngleDisplacement)
    {
        if (end - start <= 1)
        {
            return;
        }

        int midPointIndex = (start + end) / 2;
        Vector2Int midPoint = FindMidpoint(list[start], list[end]);
        Vector2Int displacedMidpoint = DisplacePoint(list[start], midPoint, maxAngleDisplacement);
        list[midPointIndex] = displacedMidpoint;

        if (maxAngleDisplacement > 1f)
        {
            maxAngleDisplacement--;
        }

        SubdivideRecursive(list, start, midPointIndex, maxAngleDisplacement);  // Left section
        SubdivideRecursive(list, midPointIndex, end, maxAngleDisplacement);  // Right section
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddStartEndPoints(List<Vector2Int> points)
    {
        Vector2Int startPoint = new Vector2Int(2, 2);
        Vector2Int endPoint = new Vector2Int(98, 98);

        points[0] = startPoint;
        points[points.Count - 1] = endPoint;
    }

    Vector2Int FindMidpoint(Vector2Int startPoint, Vector2Int endPoint)
    {
        Vector2Int midpoint = startPoint + new Vector2Int((endPoint.x - startPoint.x) / 2, (endPoint.y - startPoint.y) / 2);
        return midpoint;
    }

    Vector2Int DisplacePoint(Vector2Int fixedPoint, Vector2Int displacePoint, float maxAngleDisplacement)
    {
        Vector2Int vector = displacePoint - fixedPoint;
        float displacementAngle = UnityEngine.Random.Range(-maxAngleDisplacement, maxAngleDisplacement);
        Vector2Int displacementVector = RotateVector2Int(vector, displacementAngle);
        return fixedPoint + displacementVector;
    }

    Vector2Int RotateVector2Int(Vector2Int originalVector, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 original = originalVector;

        float cosAngle = Mathf.Cos(angleInRadians);
        float sinAngle = Mathf.Sin(angleInRadians);

        Vector2 rotated = new Vector2(original.x * cosAngle - original.y * sinAngle, original.x * sinAngle + original.y * cosAngle);

        return new Vector2Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.y));
    }

    // Interpolate the points in the 2D array by adding 1s between each consecutive pair of Vector2Int points
    public void InterpolateRiverPoints(Map map, List<Vector2Int> points, int riverID)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2Int start = points[i];
            Vector2Int end = points[i + 1];

            // Use Bresenham's Line Algorithm to get the points between the start and end
            foreach (Vector2Int point in GetLinePoints(start, end))
            {
                // Ensure we are within the array bounds
                if (point.x >= 0 && point.x < map.GetLength(0) && point.y >= 0 && point.y < map.GetLength(1))
                {
                    map.SetLandWaterType(point.x, point.y, LandWaterType.River);
                    map.SetLandWaterFeatureID(point.x, point.y, riverID);
                    CarveValley(map, point, 7, GeoFeatureAtlas.GetAvailableValleyID(), 0.5f);
                }
            }
        }
    }

    public void CarveValley(Map map, Vector2Int bottomLocation, int valleyRadius, int valleyID, float valleyDepth)
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

    // Bresenham's Line Algorithm to get all points on the line between start and end
    private List<Vector2Int> GetLinePoints(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> linePoints = new List<Vector2Int>();

        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            linePoints.Add(new Vector2Int(x0, y0));

            if (x0 == x1 && y0 == y1) break;

            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return linePoints;
    }

    public void WriteArrayToCSV(float[,] array, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                string[] rowValues = new string[cols];

                for (int j = 0; j < cols; j++)
                {
                    // Convert each integer to string
                    rowValues[j] = array[i, j].ToString();
                }

                // Write the row as a comma-separated string
                writer.WriteLine(string.Join(",", rowValues));
            }
        }

        Console.WriteLine($"CSV file written to {filePath}");
    }
}

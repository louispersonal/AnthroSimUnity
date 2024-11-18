using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class ContinentGenerator
{
    public static List<Rectangle> GetAllContinentBounds(Map map, int numContinents)
    {
        List<Rectangle> rectangles = new List<Rectangle>();

        int horizontalSlices = 1;
        int verticalSlices = 1;
        int step = 0;
        while (horizontalSlices * verticalSlices < numContinents)
        {
            if (step % 2 == 0)
            {
                horizontalSlices++;
            }
            else
            {
                verticalSlices++;
            }
        }

        int edgeOffset = map.GetLength(0) / 5;

        int continentWidth = (map.GetLength(0) - edgeOffset * 2) / horizontalSlices;
        int continentHeight = (map.GetLength(1) - edgeOffset * 2) / verticalSlices;

        for (int h = 0; h < horizontalSlices; h++)
        {
            for (int v = 0; v < verticalSlices; v++)
            {
                rectangles.Add(new Rectangle(edgeOffset + (h * continentWidth), edgeOffset + ((h + 1) * continentWidth), edgeOffset + (v * continentHeight), edgeOffset + ((v + 1) * continentHeight)));
            }
        }

        return rectangles;
    }

    public static void CreateContinent(Map map, Rectangle bounds)
    {
        int continentID = ServiceProvider.LandwaterAtlas.GetAvailableContinentID();
        GenerateOutline(map, bounds, continentID);
        Vector2 containingPoint = FindContainingPoint(bounds);
        FloodFill(map, containingPoint, continentID);
        AddPerlinNoise(map, continentID);

        int numMountainRanges = (int)(ServiceProvider.LandwaterAtlas.Continents[continentID].area * GlobalParameters.MountainRangeContinentAreaRatio);
        for (int m = 0; m < numMountainRanges; m++)
        {
            MountainGenerator.AddMountainRange(map, bounds);
        }
    }

    public static void AddPerlinNoise(Map map, int continentID)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map.GetLandWaterFeatureID(x, y) == continentID)
                {
                    float displacement = CalculateHeight(x, y, width, height) * 0.5f;
                    map.SetHeight(x, y, map.GetHeight(x, y) + displacement);
                }
            }
        }

    }
    public static float CalculateHeight(int x, int y, int width, int height)
    {
        float scale = 45f;
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    public static Vector2 FindContainingPoint(Rectangle bounds)
    {
        return new Vector2(((bounds.X_hi - bounds.X_lo) / 2) + bounds.X_lo, ((bounds.Y_hi - bounds.Y_lo) / 2) + bounds.Y_lo);
    }

    public static void GenerateOutline(Map map, Rectangle bounds, int continentID)
    {
        int x_mid = ((bounds.X_hi - bounds.X_lo) / 2) + bounds.X_lo;
        int y_mid = ((bounds.Y_hi - bounds.Y_lo) / 2) + bounds.Y_lo;

        // divide bound into 4 quadrants and pick a point in each quadrant
        List<Vector2Int> corners = new List<Vector2Int>();
        corners.Add(new Vector2Int(Random.Range(bounds.X_lo, x_mid), Random.Range(y_mid, bounds.Y_hi)));
        corners.Add(new Vector2Int(Random.Range(x_mid, bounds.X_hi), Random.Range(y_mid, bounds.Y_hi)));
        corners.Add(new Vector2Int(Random.Range(x_mid, bounds.X_hi), Random.Range(bounds.Y_lo, y_mid)));
        corners.Add(new Vector2Int(Random.Range(bounds.X_lo, x_mid), Random.Range(bounds.Y_lo, y_mid)));

        List<Vector2Int> outline = RandomWalkVector.RandomWalk(corners[0], corners[1], GlobalParameters.NumContinentEdgeVerticesRatio, GlobalParameters.MaxContinentEdgeDisplacementAngle);
        outline.AddRange(RandomWalkVector.RandomWalk(corners[1], corners[2], GlobalParameters.NumContinentEdgeVerticesRatio, GlobalParameters.MaxContinentEdgeDisplacementAngle));
        outline.AddRange(RandomWalkVector.RandomWalk(corners[2], corners[3], GlobalParameters.NumContinentEdgeVerticesRatio, GlobalParameters.MaxContinentEdgeDisplacementAngle));
        outline.AddRange(RandomWalkVector.RandomWalk(corners[3], corners[0], GlobalParameters.NumContinentEdgeVerticesRatio, GlobalParameters.MaxContinentEdgeDisplacementAngle));

        RemoveConsecutiveDuplicatePoints(outline);

        foreach (Vector2Int outlinePoint in outline)
        {
            map.SetHeight(outlinePoint.x, outlinePoint.y, 0.5f);
            map.SetLandWaterType(outlinePoint.x, outlinePoint.y, LandWaterType.Continent);
            map.SetLandWaterFeatureID(outlinePoint.x, outlinePoint.y, continentID);
        }
    }

    public static void RemoveConsecutiveDuplicatePoints(List<Vector2Int> outline)
    {
        int count = outline.Count - 1;
        List<int> duplicateIndeces = new List<int>();
        for(int i = 0; i < count; i++)
        {
            Vector2Int thisPoint = outline[i];
            Vector2Int nextPoint = outline[i + 1];
            if (nextPoint == thisPoint)
            {
                duplicateIndeces.Add(i + 1);
            }
        }
        for(int d = duplicateIndeces.Count - 1; d >=0; d--)
        {
            outline.RemoveAt(duplicateIndeces[d]);
        }
    }

    public static void RemoveLoops(List<Vector2Int> outline)
    {

    }

    public static bool CheckForDuplicatePoints(List<Vector2Int> outline, bool excludeFinalPoint)
    {
        int offset = 0;
        if (excludeFinalPoint)
        {
            offset = 1;
        }
        var distinct = outline.Distinct().ToList();
        return distinct.Count() != outline.Count() - offset;
    }

    public static void FloodFill(Map map, Vector2 containintPoint, int continentID)
    {
        // Get the dimensions of the grid
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        int startX = (int)containintPoint.x;
        int startY = (int)containintPoint.y;

        // Use a stack to simulate the recursion
        Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            // Pop a point from the stack
            var (x, y) = stack.Pop();

            // Fill the point with 0.5
            map.SetHeight(x, y, 0.5f);
            map.SetLandWaterType(x, y, LandWaterType.Continent);
            map.SetLandWaterFeatureID(x, y, continentID);
            ServiceProvider.LandwaterAtlas.Continents[continentID].area++;

            // Check all four directions and push valid points onto the stack
            if (x + 1 < rows && map.GetLandWaterFeatureID(x + 1, y) != continentID) stack.Push((x + 1, y));  // Down
            if (x - 1 >= 0 && map.GetLandWaterFeatureID(x - 1, y) != continentID) stack.Push((x - 1, y));    // Up
            if (y + 1 < cols && map.GetLandWaterFeatureID(x, y + 1) != continentID) stack.Push((x, y + 1));  // Right
            if (y - 1 >= 0 && map.GetLandWaterFeatureID(x, y - 1) != continentID) stack.Push((x, y - 1));    // Left
        }
    }

}

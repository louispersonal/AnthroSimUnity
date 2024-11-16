using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static void CreateContinent(MapGenerator mapGenerator, Map map, Rectangle bounds)
    {
        int continentID = mapGenerator.LandwaterAtlas.GetAvailableContinentID();
        GenerateOutline(mapGenerator, map, bounds, continentID);
        Vector2 containingPoint = FindContainingPoint(bounds);
        FloodFill(map, containingPoint, continentID);
        AddPerlinNoise(map, continentID);

        for (int m = 0; m < 10; m++)
        {
            //MountainGenerator.AddMountainRange(mapGenerator, map, bounds);
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
        float scale = 20f;
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    public static Vector2 FindContainingPoint(Rectangle bounds)
    {
        return new Vector2(((bounds.X_hi - bounds.X_lo) / 2) + bounds.X_lo, ((bounds.Y_hi - bounds.Y_lo) / 2) + bounds.Y_lo);
    }

    public static void GenerateOutline(MapGenerator mapGenerator, Map map, Rectangle bounds, int continentID)
    {
        int x_mid = ((bounds.X_hi - bounds.X_lo) / 2) + bounds.X_lo;
        int y_mid = ((bounds.Y_hi - bounds.Y_lo) / 2) + bounds.Y_lo;

        // divide bound into 4 quadrants and pick a point in each quadrant
        List<Vector2Int> corners = new List<Vector2Int>();
        corners.Add(new Vector2Int(Random.Range(bounds.X_lo, x_mid), Random.Range(y_mid, bounds.Y_hi)));
        corners.Add(new Vector2Int(Random.Range(x_mid, bounds.X_hi), Random.Range(y_mid, bounds.Y_hi)));
        corners.Add(new Vector2Int(Random.Range(x_mid, bounds.X_hi), Random.Range(bounds.Y_lo, y_mid)));
        corners.Add(new Vector2Int(Random.Range(bounds.X_lo, x_mid), Random.Range(bounds.Y_lo, y_mid)));

        List<Vector2Int> outline = RandomWalkVector.RandomWalk(corners[0], corners[1], mapGenerator.GenerationParameters.NumContinentEdgeVerticesRatio, mapGenerator.GenerationParameters.MaxContinentEdgeDisplacementAngle);
        outline.AddRange(RandomWalkVector.RandomWalk(corners[1], corners[2], mapGenerator.GenerationParameters.NumContinentEdgeVerticesRatio, mapGenerator.GenerationParameters.MaxContinentEdgeDisplacementAngle));
        outline.AddRange(RandomWalkVector.RandomWalk(corners[2], corners[3], mapGenerator.GenerationParameters.NumContinentEdgeVerticesRatio, mapGenerator.GenerationParameters.MaxContinentEdgeDisplacementAngle));
        outline.AddRange(RandomWalkVector.RandomWalk(corners[3], corners[0], mapGenerator.GenerationParameters.NumContinentEdgeVerticesRatio, mapGenerator.GenerationParameters.MaxContinentEdgeDisplacementAngle));

        foreach (Vector2Int outlinePoint in outline)
        {
            map.SetHeight(outlinePoint.x, outlinePoint.y, 0.5f);
            map.SetLandWaterType(outlinePoint.x, outlinePoint.y, LandWaterType.Continent);
            map.SetLandWaterFeatureID(outlinePoint.x, outlinePoint.y, continentID);
        }
    }

    public static Vector2 RotateVectorRandomly(Vector2 vector)
    {
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0:
                return vector;
            case 1:
                return new Vector2(vector.y * -1, vector.x);
            case 2:
                return new Vector2(vector.y, vector.x * -1);
        }
        return vector;
    }

    public static bool CheckStepInBounds(int x, int y, Rectangle bounds, Vector2 step)
    {
        x += (int)step.x;
        y += (int)step.y;
        if (y < bounds.Y_lo || y > bounds.Y_hi || x < bounds.X_lo || x > bounds.X_hi)
        {
            return false;
        }
        return true;
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

            // Check all four directions and push valid points onto the stack
            if (x + 1 < rows && map.GetLandWaterFeatureID(x + 1, y) != continentID) stack.Push((x + 1, y));  // Down
            if (x - 1 >= 0 && map.GetLandWaterFeatureID(x - 1, y) != continentID) stack.Push((x - 1, y));    // Up
            if (y + 1 < cols && map.GetLandWaterFeatureID(x, y + 1) != continentID) stack.Push((x, y + 1));  // Right
            if (y - 1 >= 0 && map.GetLandWaterFeatureID(x, y - 1) != continentID) stack.Push((x, y - 1));    // Left
        }
    }

}

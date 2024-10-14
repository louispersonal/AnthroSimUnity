using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Timeline;
using UnityEngine;
using Color = UnityEngine.Color;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    int _minimumMountainRadius;

    [SerializeField]
    int _maximumMountainRadius;

    [SerializeField]
    int _maxMountainsPerRange;

    [SerializeField]
    float _chanceOfRiverSourceOnMountain;

    LandwaterAtlas _landwaterAtlas;
    LandwaterAtlas LandwaterAtlas { get { if (_landwaterAtlas == null) { _landwaterAtlas = FindObjectOfType<LandwaterAtlas>(); } return _landwaterAtlas; } set { _landwaterAtlas = value; } }

    GeoFeatureAtlas _geoFeatureAtlas;
    GeoFeatureAtlas GeoFeatureAtlas { get { if (_geoFeatureAtlas == null) { _geoFeatureAtlas = FindObjectOfType<GeoFeatureAtlas>(); } return _geoFeatureAtlas; } set { _geoFeatureAtlas = value; } }

    void Start()
    {

    }

    public void GenerateMap(Map map, int width, int height, float mapResolution, int numContinents)
    {
        map.InitializeMap(width, height);
        for (int c = 0; c < numContinents; c++)
        {
            Rectangle bounds = GetContinentBounds(map);
            CreateContinent(map, bounds);
            for (int m = 0; m < 5; m++)
            {
                AddMountainRange(map, bounds);
            }
        }
        CreateSpriteFromMap(map, mapResolution);
    }

    public Rectangle GetContinentBounds(Map map)
    {
        int startOffset = map.GetLength(0) / 5;
        return new Rectangle(startOffset, map.GetLength(0) - startOffset, startOffset, map.GetLength(1) - startOffset);
    }

    void CreateContinent(Map map, Rectangle bounds)
    {
        int continentID = LandwaterAtlas.GetAvailableContinentID();
        Vector2 startPoint = new Vector2(bounds.X_lo, bounds.Y_lo);
        GenerateOutline(map, bounds, startPoint, continentID);
        Vector2 containingPoint = FindContainingPoint(bounds);
        map = FloodFill(map, containingPoint, continentID);
    }

    Vector2 FindContainingPoint(Rectangle bounds)
    {
        return new Vector2(((bounds.X_hi - bounds.X_lo) / 2) + bounds.X_lo, ((bounds.Y_hi - bounds.Y_lo) / 2) + bounds.Y_lo);
    }

    void GenerateOutline(Map map, Rectangle bounds, Vector2 startPoint, int continentID)
    {
        // assume start point is the bottom left corner (lo x, lo y)
        // initial general direction will be +x
        // directions can be (1,0) (0,-1) (-1,0) (0,1)
        // pick an angle of 0, -90 or +90 from the direction vector

        // to rotate 90 degrees, swap components and multiply new x by -1
        // to rotate -90 degrees, swap components and multiply new y by -1

        float pixelValue = 0.5f;

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);
        int x = (int) startPoint.x;
        int y = (int) startPoint.y;
        map.SetHeight(x, y, pixelValue);

        List<Vector2> cardinalDirections = new List<Vector2>();
        cardinalDirections.Add(new Vector2(1, 0));
        cardinalDirections.Add(new Vector2(0, 1));
        cardinalDirections.Add(new Vector2(-1, 0));
        cardinalDirections.Add(new Vector2(0, -1));

        // left
        while (x < bounds.X_hi)
        {
            Vector2 step = RotateVectorRandomly(cardinalDirections[0]);
            if (CheckStepInBounds(x, y, bounds, step))
            {
                x += (int)step.x;
                y += (int)step.y;
                map.SetHeight(x, y, pixelValue);
                map.MapData.Data[x, y].LandWaterType = LandWaterType.Continent;
                map.MapData.Data[x, y].LandWaterFeatureID = continentID;
            }
        }


        // up
        while (y < bounds.Y_hi)
        {
            Vector2 step = RotateVectorRandomly(cardinalDirections[1]);
            if (CheckStepInBounds(x, y, bounds, step))
            {
                x += (int)step.x;
                y += (int)step.y;
                map.SetHeight(x, y, pixelValue);
                map.MapData.Data[x, y].LandWaterType = LandWaterType.Continent;
                map.MapData.Data[x, y].LandWaterFeatureID = continentID;
            }
        }

        // right
        while (x > bounds.X_lo)
        {
            Vector2 step = RotateVectorRandomly(cardinalDirections[2]);
            if (CheckStepInBounds(x, y, bounds, step))
            {
                x += (int)step.x;
                y += (int)step.y;
                map.SetHeight(x, y, pixelValue);
                map.MapData.Data[x, y].LandWaterType = LandWaterType.Continent;
                map.MapData.Data[x, y].LandWaterFeatureID = continentID;
            }
        }

        // down
        while (y > bounds.Y_lo)
        {
            Vector2 step = RotateVectorRandomly(cardinalDirections[3]);
            if (CheckStepInBounds(x, y, bounds, step))
            {
                x += (int)step.x;
                y += (int)step.y;
                map.SetHeight(x, y, pixelValue);
                map.MapData.Data[x, y].LandWaterType = LandWaterType.Continent;
                map.MapData.Data[x, y].LandWaterFeatureID = continentID;
            }
        }
    }

    Vector2 RotateVectorRandomly(Vector2 vector)
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

    bool CheckStepInBounds(int x, int y, Rectangle bounds, Vector2 step)
    {
        x += (int)step.x;
        y += (int)step.y;
        if (y < bounds.Y_lo || y > bounds.Y_hi || x < bounds.X_lo || x > bounds.X_hi)
        {
            return false;
        }
        return true;
    }

    Map FloodFill(Map map, Vector2 containintPoint, int continentID)
    {
        // Get the dimensions of the grid
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        int startX = (int)containintPoint.x;
        int startY = (int)containintPoint.y;

        // If the starting point is not valid (already 1 or out of bounds), return
        if (startX < 0 || startX >= rows || startY < 0 || startY >= cols || map.GetHeight(startX, startY) != 0)
        {
            return map;
        }

        // Use a stack to simulate the recursion
        Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            // Pop a point from the stack
            var (x, y) = stack.Pop();

            // Fill the point with 0.5
            map.SetHeight(x, y, 0.5f);
            map.MapData.Data[x, y].LandWaterType = LandWaterType.Continent;
            map.MapData.Data[x, y].LandWaterFeatureID = continentID;

            // Check all four directions and push valid points onto the stack
            if (x + 1 < rows && map.GetHeight(x + 1, y) == 0) stack.Push((x + 1, y));  // Down
            if (x - 1 >= 0 && map.GetHeight(x - 1, y) == 0) stack.Push((x - 1, y));    // Up
            if (y + 1 < cols && map.GetHeight(x, y + 1) == 0) stack.Push((x, y + 1));  // Right
            if (y - 1 >= 0 && map.GetHeight(x, y - 1) == 0) stack.Push((x, y - 1));    // Left
        }
        return map;
    }

    void CreateSpriteFromMap(Map map, float mapResolution)
    {
        // Create a new Texture2D
        Texture2D texture = new Texture2D(map.GetLength(0), map.GetLength(1));

        // Loop through the array and set pixels
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Color color;
                if (map.MapData.Data[x, y].LandWaterType == LandWaterType.Ocean || map.MapData.Data[x, y].LandWaterType == LandWaterType.River)
                {
                    color = new Color(0, 0.3f, 0.7f);
                }
                else
                {
                    float value = map.GetHeight(x, y);
                    color = new Color(0, value, 0);
                }
                texture.SetPixel(x, y, color);
            }
        }

        // Apply all SetPixel calls
        texture.Apply();
        // Specify the pixels per unit (e.g., 5000 if you want it to appear larger or 50 if you want it smaller)
        float pixelsPerUnit = mapResolution;
        // Create a new sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, map.GetLength(0), map.GetLength(1)), new Vector2(0.5f, 0.5f), pixelsPerUnit);

        // Create a GameObject and add a SpriteRenderer
        SpriteRenderer spriteRenderer = map.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    void AddMountainRange(Map map, Rectangle continentBounds)
    {
        int mountainRadius = Random.Range(_minimumMountainRadius, _maximumMountainRadius);
        Vector2Int firstMountainPeak = FindMountainPeak(map, continentBounds, mountainRadius);
        int numMountains = 1;
        AddMountain(map, firstMountainPeak,  mountainRadius);
        Vector2 rangeDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rangeDirection = rangeDirection.normalized;
        while (CheckSpaceForMountain(map, firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * (mountainRadius * 2))), mountainRadius) && numMountains < _maxMountainsPerRange)
        {
            AddMountain(map, firstMountainPeak, mountainRadius);
            numMountains++;
            firstMountainPeak = firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * (mountainRadius * 2)));
        }
    }

    void AddMountain(Map map, Vector2Int peakLocation, int mountainRadius)
    {
        MapDataPoint point = map.MapData.Data[peakLocation.x, peakLocation.y];
        point.Height = 1f;
        point.GeoFeatureType = GeoFeatureType.Mountain;
        int mountainID = GeoFeatureAtlas.GetAvailableMountainID();
        point.GeoFeatureID = mountainID;
        TwoDimensionalMidpointDisplacement(map, 1f, peakLocation, mountainRadius, mountainID);
        //FormMountain(map, peakLocation, mountainRadius, mountainID);
        if (Random.Range(0f, 1f) < _chanceOfRiverSourceOnMountain)
        {
            AddRiver(map, peakLocation);
        }
    }

    void FormMountain(Map map, Vector2Int peakLocation, int mountainRadius, int mountainID, float minHeight = 0.5f, float maxHeight = 1.0f)
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
                float value = maxHeight - ((a * dx * dx + b * dy * dy) / (mountainRadius * mountainRadius));

                // Clamp the value between minHeight and maxHeight
                value = Mathf.Max(minHeight, Mathf.Min(maxHeight, value));

                // Add this peak value to the array
                map.MapData.Data[x, y].Height = value;
                map.MapData.Data[x, y].GeoFeatureType = GeoFeatureType.Mountain;
                map.MapData.Data[x, y].GeoFeatureID = mountainID;
            }
        }
    }

    Vector2Int FindMountainPeak(Map map, Rectangle continentBounds, int mountainRadius)
    {
        bool spaceForMountain = false;
        Vector2Int randomPoint;
        do
        {
            // pick random point
            randomPoint = new Vector2Int(Random.Range(continentBounds.X_lo, continentBounds.X_hi), Random.Range(continentBounds.X_lo, continentBounds.X_hi));
            spaceForMountain = CheckSpaceForMountain(map, randomPoint, mountainRadius);
        } while (!spaceForMountain);
        return randomPoint;
    }

    bool CheckSpaceForMountain(Map map, Vector2Int peakLocation, int mountainRadius)
    {
        bool roomOnLand =  map.MapData.Data[peakLocation.x + mountainRadius, peakLocation.y].LandWaterType == LandWaterType.Continent
            && map.MapData.Data[peakLocation.x - mountainRadius, peakLocation.y].LandWaterType == LandWaterType.Continent
            && map.MapData.Data[peakLocation.x, peakLocation.y + mountainRadius].LandWaterType == LandWaterType.Continent
            && map.MapData.Data[peakLocation.x, peakLocation.y - mountainRadius].LandWaterType == LandWaterType.Continent;

        bool noConflictingMountain = map.MapData.Data[peakLocation.x + mountainRadius, peakLocation.y].GeoFeatureType == GeoFeatureType.None
            && map.MapData.Data[peakLocation.x - mountainRadius, peakLocation.y].GeoFeatureType == GeoFeatureType.None
            && map.MapData.Data[peakLocation.x, peakLocation.y + mountainRadius].GeoFeatureType == GeoFeatureType.None
            && map.MapData.Data[peakLocation.x, peakLocation.y - mountainRadius].GeoFeatureType == GeoFeatureType.None;

        return roomOnLand && noConflictingMountain;
    }

    void AddRiver(Map map, Vector2Int sourceLocation)
    {
        Vector2Int riverEndLocation = FindClosestCoastline(map, sourceLocation);
        //int maxRiverWidth = 100;
        map.MapData.Data[sourceLocation.x, sourceLocation.y].LandWaterType = LandWaterType.River;
        int riverID = LandwaterAtlas.GetAvailableRiverID();
        map.MapData.Data[sourceLocation.x, sourceLocation.y].LandWaterFeatureID = riverID;
        Vector2Int currentPosition = sourceLocation;
        while (currentPosition != riverEndLocation)
        {
            if (Random.Range(0, 2) == 0) // Move horizontally
            {
                if (currentPosition.x < riverEndLocation.x)
                {
                    currentPosition.x++;
                }
                else if (currentPosition.x > riverEndLocation.x)
                {
                    currentPosition.x--;
                }
            }
            else // Move vertically
            {
                if (currentPosition.y < riverEndLocation.y)
                {
                    currentPosition.y++;
                }
                else if (currentPosition.y > riverEndLocation.y)
                {
                    currentPosition.y--;
                }
            }
            map.MapData.Data[currentPosition.x, currentPosition.y].LandWaterType = LandWaterType.River;
            map.MapData.Data[currentPosition.x, currentPosition.y].LandWaterFeatureID = riverID;
        }
    }

    Vector2Int FindClosestCoastline(Map map, Vector2Int point)
    {
        float maxSearchDistance = 499f;
        Vector2Int currentClosestPoint = new Vector2Int(0, 0);
        Vector2Int currentPoint = new Vector2Int(0, 0);
        for (float angle = 0f; angle < 2 * Mathf.PI; angle += Mathf.PI / 6)
        {
            for(float range = 0f; range < maxSearchDistance; range++)
            {
                currentPoint.x = point.x + (int)(range * Mathf.Cos(angle));
                currentPoint.y = point.y + (int)(range * Mathf.Sin(angle));
                if (currentPoint.x < map.GetLength(0) && currentPoint.x >= 0 && currentPoint.y < map.GetLength(1) && currentPoint.y >= 0)
                {
                    if (map.MapData.Data[currentPoint.x, currentPoint.y].LandWaterType == LandWaterType.Ocean)
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

    int[] OneDimensionalMidpointDisplacement(int size, int maxDisplacement)
    {
        int[] output = new int[size];
        int displacement = maxDisplacement;
        int step_size = size - 1;
        while (step_size > 1)
        {
            for (int x = 0; x < size / step_size; x++)
            {
                output[(x * step_size) + step_size / 2] = ((output[x*step_size] + output[(x*step_size) + step_size]) / 2) + Random.Range(-displacement, displacement);
            }
            if (displacement > 0)
            {
                displacement--;
            }
            step_size /= 2;
        }
        return output;
    }

    public void TwoDimensionalMidpointDisplacement(Map map, float roughness, Vector2Int peakLocation, int mountainRadius, int mountainID)
    {
        int size = mountainRadius - 1;
        int stepSize = size - 1; // Step size between points

        while (stepSize > 1)
        {
            int halfStep = stepSize / 2;

            // Diamond step
            for (int x = peakLocation.x - mountainRadius; x < size - 1; x += stepSize)
            {
                for (int y = peakLocation.y - mountainRadius; y < size - 1; y += stepSize)
                {
                    DiamondStep(map, x, y, stepSize, roughness);
                }
            }

            // Square step
            for (int x = peakLocation.x - mountainRadius; x < size - 1; x += halfStep)
            {
                for (int y = (x + halfStep) % stepSize; y < size - 1; y += stepSize)
                {
                    SquareStep(map, x, y, halfStep, roughness);
                }
            }

            // Reduce the roughness for the next iteration
            roughness *= 0.5f;

            // Halve the step size
            stepSize /= 2;
        }
    }

    private void DiamondStep(Map map, int x, int y, int stepSize, float roughness)
    {
        int halfStep = stepSize / 2;
        float avg = (map.MapData.Data[x, y].Height +
                     map.MapData.Data[x + stepSize, y].Height +
                     map.MapData.Data[x, y + stepSize].Height +
                     map.MapData.Data[x + stepSize, y + stepSize].Height) / 4.0f;

        map.MapData.Data[x + halfStep, y + halfStep].Height = avg + RandomOffset(roughness);
    }

    private void SquareStep(Map map, int x, int y, int stepSize, float roughness)
    {
        int halfStep = stepSize / 2;
        int size = map.GetLength(0);

        float avg = 0f;
        int count = 0;

        if (x - halfStep >= 0)
        {
            avg += map.MapData.Data[x - halfStep, y].Height;
            count++;
        }
        if (x + halfStep < size)
        {
            avg += map.MapData.Data[x + halfStep, y].Height;
            count++;
        }
        if (y - halfStep >= 0)
        {
            avg += map.MapData.Data[x, y - halfStep].Height;
            count++;
        }
        if (y + halfStep < size)
        {
            avg += map.MapData.Data[x, y + halfStep].Height;
            count++;
        }

        avg /= count;

        map.MapData.Data[x, y].Height = avg + RandomOffset(roughness);
    }

    private float RandomOffset(float roughness)
    {
        return (Random.Range(0f, 1f) * 2f - 1f) * roughness;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    Map _map;

    [SerializeField]
    int _mapWidth;

    [SerializeField]
    int _mapHeight;

    [SerializeField]
    [Tooltip("Refers to the number of pixels per world unit")]
    float _mapResolution;

    [SerializeField]
    int _minimumMountainRange;

    [SerializeField]
    int _maximumMountainRange;

    LandwaterAtlas _landwaterAtlas;
    LandwaterAtlas LandwaterAtlas { get { if (_landwaterAtlas == null) { _landwaterAtlas = FindObjectOfType<LandwaterAtlas>(); } return _landwaterAtlas; } set { _landwaterAtlas = value; } }

    void Start()
    {
        _map.InitializeMap(_mapWidth, _mapHeight);
        int startOffset = _mapWidth / 5;
        Rectangle bounds = new Rectangle(startOffset, _map.GetLength(0) - startOffset, startOffset, _map.GetLength(1) - startOffset);
        _map = CreateContinent(_map, bounds);
        _map = AddMountainRange(_map, bounds);
        CreateSpriteFromMap(_map);
    }

    Map CreateContinent(Map map, Rectangle bounds)
    {
        int continentID = LandwaterAtlas.GetAvailableContinentID();
        Vector2 startPoint = new Vector2(bounds.X_lo, bounds.Y_lo);
        map = GenerateOutline(map, bounds, startPoint, continentID);
        Vector2 containingPoint = FindContainingPoint(bounds);
        map = FloodFill(map, containingPoint, continentID);
        return map;
    }

    Vector2 FindContainingPoint(Rectangle bounds)
    {
        return new Vector2(((bounds.X_hi - bounds.X_lo) / 2) + bounds.X_lo, ((bounds.Y_hi - bounds.Y_lo) / 2) + bounds.Y_lo);
    }

    Map GenerateOutline(Map map, Rectangle bounds, Vector2 startPoint, int continentID)
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


        return map;
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

    void CreateSpriteFromMap(Map map)
    {
        // Create a new Texture2D
        Texture2D texture = new Texture2D(map.GetLength(0), map.GetLength(1));

        // Loop through the array and set pixels
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                float value = map.GetHeight(x, y); // Assuming your values are in the range 0-255
                Color color = new Color(value, value, value); // Create a grayscale color
                texture.SetPixel(x, y, color);
            }
        }

        // Apply all SetPixel calls
        texture.Apply();
        // Specify the pixels per unit (e.g., 5000 if you want it to appear larger or 50 if you want it smaller)
        float pixelsPerUnit = _mapResolution;
        // Create a new sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, map.GetLength(0), map.GetLength(1)), new Vector2(0.5f, 0.5f), pixelsPerUnit);

        // Create a GameObject and add a SpriteRenderer
        SpriteRenderer spriteRenderer = map.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    Map AddMountainRange(Map map, Rectangle continentBounds)
    {
        int mountainRadius = Random.Range(_minimumMountainRange, _maximumMountainRange);
        Vector2Int firstMountainPeak = FindMountainPeak(map, continentBounds, mountainRadius);
        map = AddMountain(map, firstMountainPeak,  mountainRadius);
        Vector2 rangeDirection = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        rangeDirection = rangeDirection.normalized;
        while (CheckSpaceForMountain(map, firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * (mountainRadius * 2))), mountainRadius))
        {
            map = AddMountain(map, firstMountainPeak, mountainRadius);
            firstMountainPeak = firstMountainPeak + Vector2Int.CeilToInt((rangeDirection * (mountainRadius * 2)));
        }
        return map;
    }

    Map AddMountain(Map map, Vector2Int peakLocation, int mountainRadius)
    {
        MapDataPoint point = map.MapData.Data[peakLocation.x, peakLocation.y];
        point.Height = 1f;
        point.GeoFeatureType = GeoFeatureType.Mountain;
        AddParabolicPeak(map, peakLocation, mountainRadius);
        return map;
    }

    public static void AddParabolicPeak(Map map, Vector2Int peakLocation, int mountainRadius, float minHeight = 0.5f, float maxHeight = 1.0f)
    {
        // Coefficients for controlling the spread of the parabola
        float a = 1.0f, b = 1.0f;

        // Iterate over every element in the 2D array
        for (int y = peakLocation.y - mountainRadius; y < peakLocation.y + mountainRadius; y++)
        {
            for (int x = peakLocation.x - mountainRadius; x < peakLocation.x - mountainRadius; x++)
            {
                // Calculate the distance from the center of the peak
                float dx = ((float)x - (float)peakLocation.x);
                float dy = ((float)y - (float)peakLocation.y);

                // Apply the parabolic formula
                float value = maxHeight - ((a * dx * dx + b * dy * dy) / (mountainRadius * mountainRadius));

                // Clamp the value between minHeight and maxHeight
                value = Mathf.Max(minHeight, Mathf.Min(maxHeight, value));

                // Add this peak value to the array
                map.MapData.Data[y, x].Height = value;
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
        return map.MapData.Data[peakLocation.x + mountainRadius, peakLocation.y].LandWaterType == LandWaterType.Continent
            && map.MapData.Data[peakLocation.x - mountainRadius, peakLocation.y].LandWaterType == LandWaterType.Continent
            && map.MapData.Data[peakLocation.x, peakLocation.y + mountainRadius].LandWaterType == LandWaterType.Continent
            && map.MapData.Data[peakLocation.x, peakLocation.y - mountainRadius].LandWaterType == LandWaterType.Continent;
    }
}

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;
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

    RandomVectorWalk _randomVectorWalk;

    RandomVectorWalk RandomVectorWalk { get { if (_randomVectorWalk == null) { _randomVectorWalk = FindObjectOfType<RandomVectorWalk>(); } return _randomVectorWalk; } set { _randomVectorWalk = value; } }

    private Mesh mesh;

    [SerializeField]
    Material yourMaterial;

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
        }
        //CreateSpriteFromMap(map, mapResolution);
        CreateTiledMesh(map);
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
        FloodFill(map, containingPoint, continentID);
        AddPerlinNoise(map);

        for (int m = 0; m < 10; m++)
        {
            AddMountainRange(map, bounds);
        }


        CloudPass(map);

        for (int x = bounds.X_lo; x < bounds.X_hi; x++)
        {
            for (int y = bounds.Y_lo; y < bounds.Y_hi; y++)
            {
                //map.MapData.Data[x,y].WaterProximity = ComputeWaterProximity(map, x, y);
                map.MapData.Data[x, y].Temperature = ComputeTemperature(map, x, y);
                map.MapData.Data[x, y].LowVegetation = ComputeLowVegetation(map, x, y);
                map.MapData.Data[x, y].HighVegetation = ComputeHighVegetation(map, x, y);
            }
        }
    }

    void AddPerlinNoise(Map map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float displacement = CalculateHeight(x, y, width, height) * 0.2f;
                map.MapData.Data[x, y].Height += displacement;
            }
        }

    }
    float CalculateHeight(int x, int y, int width, int height)
    {
        float scale = 20f;
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
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

    void FloodFill(Map map, Vector2 containintPoint, int continentID)
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
            map.MapData.Data[x, y].LandWaterType = LandWaterType.Continent;
            map.MapData.Data[x, y].LandWaterFeatureID = continentID;

            // Check all four directions and push valid points onto the stack
            if (x + 1 < rows && map.GetHeight(x + 1, y) == 0) stack.Push((x + 1, y));  // Down
            if (x - 1 >= 0 && map.GetHeight(x - 1, y) == 0) stack.Push((x - 1, y));    // Up
            if (y + 1 < cols && map.GetHeight(x, y + 1) == 0) stack.Push((x, y + 1));  // Right
            if (y - 1 >= 0 && map.GetHeight(x, y - 1) == 0) stack.Push((x, y - 1));    // Left
        }
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
                    color = new Color(0.76f, 1f, 0.6f);
                    if (map.MapData.Data[x,y].LowVegetation > 0f)
                    {
                        float value = map.MapData.Data[x, y].LowVegetation;
                        color = new Color(0, value, 0);
                    }
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
        FormMountain(map, peakLocation, mountainRadius, mountainID, 1f);
        if (Random.Range(0f, 1f) < _chanceOfRiverSourceOnMountain)
        {
            AddRiver(map, peakLocation);
        }
    }

    void FormMountain(Map map, Vector2Int peakLocation, int mountainRadius, int mountainID, float peakHeight)
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
                value = Mathf.Max(map.MapData.Data[x, y].Height, Mathf.Min(peakHeight, value));

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

        int numPoints = 20;
        List<Vector2Int> points = new List<Vector2Int>();
        for (int c = 0; c < numPoints; c++)
        {
            points.Add(new Vector2Int(0, 0));
        }

        points[0] = sourceLocation;
        points[numPoints - 1] = riverEndLocation;

        RandomVectorWalk.SubdivideRecursive(points, 0, numPoints - 1, 30f);

        RandomVectorWalk.InterpolatePoints(map, points, riverID);
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

    void CloudPass(Map map)
    {
        // Starting at the top of the map, going right, the cloud picks up water over water, and drops water over land
        // cloud direction changes at 0.25x the map height and 0.75x the map height

        int overlap = map.GetLength(1) / 16;

        Cloud cloud = new Cloud();
        for (int y = 0; y < (map.GetLength(1) / 4) + overlap; y++)
        {
            cloud.Reset();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map.MapData.Data[x, y].LandWaterType == LandWaterType.Continent)
                {
                    map.MapData.Data[x, y].Precipitation += cloud.Rain();
                }
                else
                {
                    cloud.Evaporate();
                }
            }
        }
        for (int y = map.GetLength(1) / 4; y < (map.GetLength(1) * 3 / 4) + overlap; y++)
        {
            cloud.Reset();
            for (int x = map.GetLength(0) - 1; x >= 0; x--)
            {
                if (map.MapData.Data[x, y].LandWaterType == LandWaterType.Continent)
                {
                    map.MapData.Data[x, y].Precipitation += cloud.Rain();
                }
                else
                {
                    cloud.Evaporate();
                }
            }
        }
        for (int y = map.GetLength(1) * 3 / 4; y < map.GetLength(1); y++)
        {
            cloud.Reset();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map.MapData.Data[x, y].LandWaterType == LandWaterType.Continent)
                {
                    map.MapData.Data[x, y].Precipitation += cloud.Rain();
                }
                else
                {
                    cloud.Evaporate();
                }
            }
        }
    }

    float ComputeWaterProximity(Map map, int x, int y)
    {
        int maxSearchDistance = 10;
        for (int dx = -maxSearchDistance; dx <= maxSearchDistance; dx++)
        {
            for (int dy = -maxSearchDistance; dy <= maxSearchDistance; dy++)
            {
                if (map.MapData.Data[x + dx, y + dy].LandWaterType != LandWaterType.Continent)
                {
                    return 1f;
                }
            }
        }
        return map.MapData.Data[x, y].WaterProximity;
    }

    float ComputeTemperature(Map map, int x, int y)
    {
        float maxTemperature = 40f;
        float minTemperature = -20f;
        int worldHeight = map.GetLength(1);

        float tempCurveCoefficient = (4 * (minTemperature - maxTemperature)) / (-1 * worldHeight * worldHeight);

        float distanceFromEquator = Mathf.Abs((worldHeight / 2) - y);

        return maxTemperature - (tempCurveCoefficient * distanceFromEquator * distanceFromEquator);
    }

    float ComputeLowVegetation(Map map, int x, int y)
    {
        // thrives in more extreme temperatures
        float optimumTemperature = 20f;
        float vegCurveCoefficient = 0.003f;
        float currTemperature = map.MapData.Data[x, y].Temperature;

        if (map.MapData.Data[x, y].WaterProximity > 0f)
        {
            return 1f - vegCurveCoefficient * Mathf.Pow(optimumTemperature - currTemperature, 2);
        }
        return 0f;
    }

    float ComputeHighVegetation(Map map, int x, int y)
    {
        // requires mild temperatures
        float optimumTemperature = 20f;
        float vegCurveCoefficient = 0.02f;
        float currTemperature = map.MapData.Data[x, y].Temperature;

        if (map.MapData.Data[x, y].WaterProximity > 0f)
        {
            return 1f - vegCurveCoefficient * Mathf.Pow(optimumTemperature - currTemperature, 2);
        }
        return 0f;
    }

    void CreateTiledMesh(Map map)
    {
        int tileSize = 100; // Or another size within the 65,536 limit
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        float scale = 10f;

        for (int y = 0; y < height; y += tileSize)
        {
            for (int x = 0; x < width; x += tileSize)
            {
                int currentTileWidth = Mathf.Min(tileSize, width - x);
                int currentTileHeight = Mathf.Min(tileSize, height - y);
                CreateMeshTile(map, x, y, currentTileWidth, currentTileHeight, scale);
            }
        }
    }

    void CreateMeshTile(Map map, int startX, int startY, int tileWidth, int tileHeight, float scale)
    {
        Mesh tileMesh = new Mesh();
        Vector3[] vertices = new Vector3[tileWidth * tileHeight];
        int[] triangles = new int[(tileWidth - 1) * (tileHeight - 1) * 6];

        // Generate vertices and triangles for this tile
        for (int z = 0; z < tileHeight; z++)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                float y = map.MapData.Data[startX + x, startY + z].Height * scale;
                vertices[z * tileWidth + x] = new Vector3(startX + x, y, startY + z);
            }
        }

        // Set up triangles (similar to your existing code)
        // Generate triangles
        int triIndex = 0;
        for (int z = 0; z < tileHeight - 1; z++)
        {
            for (int x = 0; x < tileWidth - 1; x++)
            {
                int topLeft = z * tileWidth + x;
                int bottomLeft = (z + 1) * tileWidth + x;

                // Triangle 1
                triangles[triIndex] = topLeft;
                triangles[triIndex + 1] = bottomLeft;
                triangles[triIndex + 2] = topLeft + 1;

                // Triangle 2
                triangles[triIndex + 3] = bottomLeft;
                triangles[triIndex + 4] = bottomLeft + 1;
                triangles[triIndex + 5] = topLeft + 1;

                triIndex += 6;
            }
        }

        tileMesh.vertices = vertices;
        tileMesh.triangles = triangles;
        tileMesh.RecalculateNormals();

        // Assign this tileMesh to a new GameObject or part of a parent object
        GameObject tileObject = new GameObject($"Tile_{startX}_{startY}");
        tileObject.AddComponent<MeshFilter>().mesh = tileMesh;
        MeshRenderer renderer = tileObject.AddComponent<MeshRenderer>();
        renderer.material = yourMaterial;
        tileObject.transform.parent = map.transform; // Optional: attach to a parent
    }
}

public class Cloud
{
    float _waterLevel;
    float _waterLevelChange = 0.99f;

    public Cloud()
    {
        Reset();
    }

    public void Reset()
    {
        _waterLevel = 1;
    }

    public float Rain()
    {
        _waterLevel *= _waterLevelChange;
        _waterLevel = Mathf.Max(0f, _waterLevel);
        if (_waterLevel > 0)
        {
            return _waterLevel;
        }
        return 0;
    }

    public void Evaporate()
    {
        _waterLevel /= _waterLevelChange;
        _waterLevel = Mathf.Min(1f, _waterLevel);
    }
}
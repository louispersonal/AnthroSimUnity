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
    public GenerationParameters GenerationParameters;

    LandwaterAtlas _landwaterAtlas;
    public LandwaterAtlas LandwaterAtlas { get { if (_landwaterAtlas == null) { _landwaterAtlas = FindObjectOfType<LandwaterAtlas>(); } return _landwaterAtlas; } set { _landwaterAtlas = value; } }

    GeoFeatureAtlas _geoFeatureAtlas;
    public GeoFeatureAtlas GeoFeatureAtlas { get { if (_geoFeatureAtlas == null) { _geoFeatureAtlas = FindObjectOfType<GeoFeatureAtlas>(); } return _geoFeatureAtlas; } set { _geoFeatureAtlas = value; } }

    [SerializeField]
    Material yourMaterial;

    [SerializeField]
    MeshFilter _planeMesh;

    [SerializeField]
    MeshRenderer _planeRenderer;

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
        float planeWorldWidth = 100;
        float planeWorldHeight = 100;
        _planeMesh.mesh = CreatePlane(planeWorldWidth, planeWorldHeight, (width/5) - 1, (height/5) - 1);
        ModifyVertices(map, _planeMesh.mesh);

        map.AddMapMode(MapModes.Normal);
        map.AddMapMode(MapModes.Temperature);
        map.AddMapMode(MapModes.Precipitation);
        map.AddMapMode(MapModes.LowVegetation);
        map.AddMapMode(MapModes.HighVegetation);

        map.SetMapMode(MapModes.Normal);
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
            //AddMountainRange(map, bounds);
        }


        CloudPass(map);

        for (int x = bounds.X_lo; x < bounds.X_hi; x++)
        {
            for (int y = bounds.Y_lo; y < bounds.Y_hi; y++)
            {
                map.SetWaterProximity(x, y, ComputeWaterProximity(map, x, y));
                map.SetTemperature(x, y, ComputeTemperature(map, x, y));
                map.SetLowVegetation(x, y, ComputeLowVegetation(map, x, y));
                map.SetHighVegetation(x, y, ComputeHighVegetation(map, x, y));
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
                float displacement = CalculateHeight(x, y, width, height) * 0.5f;
                map.SetHeight(x, y, map.GetHeight(x, y) + displacement);
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
                map.SetLandWaterType(x, y, LandWaterType.Continent);
                map.SetLandWaterFeatureID(x, y, continentID);
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
                map.SetLandWaterType(x, y, LandWaterType.Continent);
                map.SetLandWaterFeatureID(x, y, continentID);
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
                map.SetLandWaterType(x, y, LandWaterType.Continent);
                map.SetLandWaterFeatureID(x, y, continentID);
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
                map.SetLandWaterType(x, y, LandWaterType.Continent);
                map.SetLandWaterFeatureID(x, y, continentID);
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
            map.SetLandWaterType(x, y, LandWaterType.Continent);
            map.SetLandWaterFeatureID(x, y, continentID);

            // Check all four directions and push valid points onto the stack
            if (x + 1 < rows && map.GetHeight(x + 1, y) == 0) stack.Push((x + 1, y));  // Down
            if (x - 1 >= 0 && map.GetHeight(x - 1, y) == 0) stack.Push((x - 1, y));    // Up
            if (y + 1 < cols && map.GetHeight(x, y + 1) == 0) stack.Push((x, y + 1));  // Right
            if (y - 1 >= 0 && map.GetHeight(x, y - 1) == 0) stack.Push((x, y - 1));    // Left
        }
    }

    public Mesh CreatePlane(float width, float height, int widthSegments, int heightSegments)
    {
        Mesh mesh = new Mesh();

        int verticesX = widthSegments + 1;
        int verticesZ = heightSegments + 1;
        Vector3[] vertices = new Vector3[verticesX * verticesZ];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[widthSegments * heightSegments * 6];

        float xStep = width / widthSegments;
        float zStep = height / heightSegments;

        // Generate vertices and UVs
        int vertIndex = 0;
        for (int z = 0; z < verticesZ; z++)
        {
            for (int x = 0; x < verticesX; x++)
            {
                float xPos = x * xStep - width / 2;
                float zPos = z * zStep - height / 2;
                vertices[vertIndex] = new Vector3(xPos, 0, zPos);
                uv[vertIndex] = new Vector2((float)x / widthSegments, (float)z / heightSegments);
                vertIndex++;
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int z = 0; z < heightSegments; z++)
        {
            for (int x = 0; x < widthSegments; x++)
            {
                int topLeft = z * verticesX + x;
                int bottomLeft = (z + 1) * verticesX + x;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = bottomLeft + 1;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomLeft + 1;
                triangles[triIndex++] = topLeft + 1;
            }
        }

        // Assign mesh properties
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    void ModifyVertices(Map map, Mesh mesh)
    {
        float heightScale = 1f;

        Vector3[] vertices = mesh.vertices;

        int resolution = 5;

        int width = map.GetLength(0) / resolution;
        int height = map.GetLength(1) / resolution;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = z * width + x; // Calculate the vertex index
                Vector3 vertex = vertices[index];
                vertex.y = map.GetHeight(x * resolution, z * resolution) * heightScale;
                vertices[index] = vertex; // Assign the updated vertex
            }
        }

        // Assign the modified vertices back to the mesh
        mesh.vertices = vertices;

        // Recalculate normals and bounds for proper lighting and rendering
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
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
                if (map.GetLandWaterType(x, y) == LandWaterType.Continent)
                {
                    map.SetPrecipitation(x, y, map.GetPrecipitation(x, y) + cloud.Rain());
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
                if (map.GetLandWaterType(x, y) == LandWaterType.Continent)
                {
                    map.SetPrecipitation(x, y, map.GetPrecipitation(x, y) + cloud.Rain());
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
                if (map.GetLandWaterType(x, y) == LandWaterType.Continent)
                {
                    map.SetPrecipitation(x, y, map.GetPrecipitation(x, y) + cloud.Rain());
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
                if (map.GetLandWaterType(x + dx, y + dy) != LandWaterType.Continent)
                {
                    return 1f;
                }
            }
        }
        return map.GetWaterProximity(x, y);
    }

    float ComputeTemperature(Map map, int x, int y)
    {
        float maxTemperature = 40f;
        float minTemperature = -20f;
        int worldHeight = map.GetLength(1);

        float tempCurveCoefficient = (4 * (minTemperature - maxTemperature)) / (-1 * worldHeight * worldHeight * 0.5f);

        float distanceFromEquator = Mathf.Abs((worldHeight / 2) - y);

        return maxTemperature - (tempCurveCoefficient * distanceFromEquator * distanceFromEquator);
    }

    float ComputeLowVegetation(Map map, int x, int y)
    {
        // thrives in more extreme temperatures
        float optimumTemperature = 20f;
        float vegCurveCoefficient = 0.003f;
        float currTemperature = map.GetTemperature(x, y);

        if (map.GetWaterProximity(x, y) > 0f || map.GetPrecipitation(x, y) > 0.5f)
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
        float currTemperature = map.GetTemperature(x, y);

        if (map.GetWaterProximity(x, y) > 0f || map.GetPrecipitation(x, y) > 0.2f)
        {
            return 1f - vegCurveCoefficient * Mathf.Pow(optimumTemperature - currTemperature, 2);
        }
        return 0f;
    }
}

public class Cloud
{
    float _waterLevel;
    float _waterLevelChange = 0.995f;

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
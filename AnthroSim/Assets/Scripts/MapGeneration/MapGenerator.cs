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
    MapTile _mapTilePrefab;

    private const int MAX_VERTICES_WIDTH_PLANE = 256;

    void Start()
    {

    }

    public void GenerateMap(Map map)
    {
        int worldWidth = GenerationParameters.WorldSizeKilometers / GenerationParameters.KilometersPerDataPoint;

        map.InitializeMap(worldWidth, worldWidth);

        List<Rectangle> continentBounds = ContinentGenerator.GetAllContinentBounds(map, GenerationParameters.NumContinents);

        foreach (Rectangle continentBound in continentBounds)
        {
            ContinentGenerator.CreateContinent(this, map, continentBound);
        }

        int widthVertices = GenerationParameters.WorldSizeKilometers / GenerationParameters.VertexSizeKilometers;

        int numPlanes = Mathf.CeilToInt((float)widthVertices / (float)MAX_VERTICES_WIDTH_PLANE);

        float tileWidth = (float)worldWidth / (float)numPlanes;

        int planeWorldWidth = widthVertices;
        int planeWorldHeight = widthVertices;

        map.MapTiles = new MapTile[numPlanes, numPlanes];

        for (int x = 0; x < numPlanes; x++)
        {
            for (int y = 0; y < numPlanes; y++)
            {
                map.MapTiles[x, y] = Instantiate(_mapTilePrefab);
                map.MapTiles[x, y].PlaneMesh.mesh = CreatePlane(tileWidth, tileWidth, MAX_VERTICES_WIDTH_PLANE, MAX_VERTICES_WIDTH_PLANE);
                ModifyVertices(map, map.MapTiles[x, y].PlaneMesh.mesh);
            }
        }

        map.AddMapMode(MapModes.Normal);
        map.AddMapMode(MapModes.Temperature);
        map.AddMapMode(MapModes.Precipitation);
        map.AddMapMode(MapModes.LowVegetation);
        map.AddMapMode(MapModes.HighVegetation);

        map.SetMapMode(MapModes.Normal);
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
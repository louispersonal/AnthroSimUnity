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

    void Start()
    {

    }

    public void GenerateMap(Map map, int width, int height)
    {
        map.InitializeMap(width, height);

        List<Rectangle> continentBounds = ContinentGenerator.GetAllContinentBounds(map, GenerationParameters.NumContinents);

        foreach (Rectangle continentBound in continentBounds)
        {
            ContinentGenerator.CreateContinent(this, map, continentBound);
        }

        float planeTileWidth = 100;
        float planeTileHeight = 100;

        float tilesX = width / planeTileWidth;
        float tilesY = height / planeTileHeight;

        map.MapTiles = new MapTile[(int)tilesX, (int)tilesY];

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                Vector2Int startPoint = new Vector2Int(x * (int)planeTileWidth, y * (int)planeTileHeight);
                Vector2Int endPoint = new Vector2Int((x + 1) * (int)planeTileWidth, (y + 1) * (int)planeTileHeight);
                map.MapTiles[x, y] = Instantiate(map.MapTilePrefab, new Vector3(x * planeTileWidth, 0, y * planeTileHeight), Quaternion.identity);
                map.MapTiles[x, y].StartPoint = startPoint;
                map.MapTiles[x, y].EndPoint = endPoint;
                map.MapTiles[x, y].PlaneMesh.mesh = CreatePlane(planeTileWidth, planeTileHeight, (int)planeTileWidth - 1, (int)planeTileHeight - 1);
                ModifyVertices(map, map.MapTiles[x, y].PlaneMesh.mesh, startPoint, endPoint, 1);
            }
        }

        map.AddMapMode(MapModes.Normal);
        //map.AddMapMode(MapModes.Temperature);
        //map.AddMapMode(MapModes.Precipitation);
        //map.AddMapMode(MapModes.LowVegetation);
        //map.AddMapMode(MapModes.HighVegetation);

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

    void ModifyVertices(Map map, Mesh mesh, Vector2Int startPoint, Vector2Int endPoint, int resolution)
    {
        float heightScale = 10f;

        Vector3[] vertices = mesh.vertices;

        for (int z = 0; z < endPoint.y - startPoint.y; z++)
        {
            for (int x = 0; x < endPoint.x - startPoint.x; x++)
            {
                int index = z * (endPoint.x - startPoint.x) + x; // Calculate the vertex index
                Vector3 vertex = vertices[index];
                vertex.y = map.GetHeight((x + startPoint.x) * resolution, (z + startPoint.y) * resolution) * heightScale;
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
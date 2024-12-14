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

        List<Rectangle> continentBounds = ContinentGenerator.GetAllContinentBounds(map, GlobalParameters.NumContinents);

        foreach (Rectangle continentBound in continentBounds)
        {
            ContinentGenerator.CreateContinent(map, continentBound);
            for (int x = continentBound.X_lo; x < continentBound.X_hi; x++)
            {
                for (int y = continentBound.Y_lo; y < continentBound.Y_hi; y++)
                {
                    map.SetWaterProximity(x, y, ComputeWaterProximity(map, x, y));
                    map.SetTemperature(x, y, ComputeTemperature(map, x, y));
                    map.SetLowVegetation(x, y, ComputeLowVegetation(map, x, y));
                    map.SetHighVegetation(x, y, ComputeHighVegetation(map, x, y));
                }
            }
        }

        CloudGenerator.CloudPass(map);

        float planeTileDataWidth = 100;
        float planeTileDataHeight = 100;

        float dataPointsPerWorldUnit = 10;
        float dataPointsPerVertex = 0.5f;

        float planeTileWorldWidth = planeTileDataWidth / dataPointsPerWorldUnit;
        float planeTileWorldHeight = planeTileDataHeight / dataPointsPerWorldUnit;

        float tilesX = width / planeTileDataWidth;
        float tilesY = height / planeTileDataHeight;

        map.MapTiles = new MapTile[(int)tilesX, (int)tilesY];

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                Vector2Int startPoint = new Vector2Int(x * (int)planeTileDataWidth, y * (int)planeTileDataHeight);
                Vector2Int endPoint = new Vector2Int((x + 1) * (int)planeTileDataWidth, (y + 1) * (int)planeTileDataHeight);
                map.MapTiles[x, y] = Instantiate(map.MapTilePrefab, new Vector3(x * planeTileWorldWidth, 0, y * planeTileWorldHeight), Quaternion.identity, map.transform);
                map.MapTiles[x, y].StartPoint = startPoint;
                map.MapTiles[x, y].EndPoint = endPoint;
                map.MapTiles[x, y].PlaneMesh.mesh = CreatePlane(planeTileWorldWidth, planeTileWorldHeight, (int)(planeTileDataWidth / dataPointsPerVertex), (int)(planeTileDataHeight / dataPointsPerVertex));
                ModifyVertices(map, map.MapTiles[x, y].PlaneMesh.mesh, startPoint, endPoint);
            }
        }

        map.AddMapMode(MapModes.Normal);
        map.AddMapMode(MapModes.Temperature);
        map.AddMapMode(MapModes.Precipitation);
        map.AddMapMode(MapModes.LowVegetation);
        map.AddMapMode(MapModes.HighVegetation);

        map.SetMapMode(MapModes.Normal);
    }

    public Mesh CreatePlane(float worldWidth, float worldHeight, int verticesX, int verticesZ)
    {
        Mesh mesh = new Mesh();

        int xDataPoints = verticesX - 1;
        int yDataPoints = verticesZ - 1;

        Vector3[] vertices = new Vector3[verticesX * verticesZ];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[xDataPoints * yDataPoints * 6];

        float xStep = worldWidth / xDataPoints;
        float zStep = worldHeight / yDataPoints;

        // Generate vertices and UVs
        int vertIndex = 0;
        for (int z = 0; z < verticesZ; z++)
        {
            for (int x = 0; x < verticesX; x++)
            {
                float xPos = x * xStep - worldWidth / 2;
                float zPos = z * zStep - worldHeight / 2;
                vertices[vertIndex] = new Vector3(xPos, 0, zPos);
                uv[vertIndex] = new Vector2((float)x / xDataPoints, (float)z / yDataPoints);
                vertIndex++;
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int z = 0; z < yDataPoints; z++)
        {
            for (int x = 0; x < xDataPoints; x++)
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

    void ModifyVertices(Map map, Mesh mesh, Vector2Int startPoint, Vector2Int endPoint)
    {
        float heightScale = 1f;

        Vector3[] vertices = mesh.vertices;
        float vertexDimension = Mathf.Sqrt(vertices.Length);

        // Calculate the scaling factors to map heightmap data to mesh vertices
        float xScale = (float)(endPoint.x - startPoint.x) / vertexDimension;
        float zScale = (float)(endPoint.y - startPoint.y) / vertexDimension;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            // Map the vertex index "i" to grid coordinates in the vertex grid
            int vertexX = i % (int)vertexDimension; // Column index
            int vertexZ = i / (int)vertexDimension; // Row index

            // Map the vertex grid coordinates to heightmap coordinates
            int mapX = Mathf.Clamp(startPoint.x + Mathf.RoundToInt((float)vertexX / (vertexDimension - 1) * (endPoint.x - startPoint.x)), startPoint.x, endPoint.x - 1);
            int mapZ = Mathf.Clamp(startPoint.y + Mathf.RoundToInt((float)vertexZ / (vertices.Length / vertexDimension - 1) * (endPoint.y - startPoint.y)), startPoint.y, endPoint.y - 1);

            // Update the vertex height based on the heightmap
            vertex.y = map.GetHeight(mapX, mapZ) * heightScale;

            // Assign the modified vertex back to the array
            vertices[i] = vertex;
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
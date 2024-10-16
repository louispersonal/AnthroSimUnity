using UnityEngine;

public class HeightMapToMesh : MonoBehaviour
{
    [SerializeField]
    Material _meshMaterial;

    public int width = 1000; // Width of the heightmap
    public int height = 1000; // Height of the heightmap
    public float[,] heightMap; // The 2D heightmap array
    public float scale = 10f; // Scale of the terrain in the Y axis

    private Mesh mesh;

    void Start()
    {
        // Initialize heightmap if not assigned
        if (heightMap == null)
        {
            heightMap = new float[width, height];
            // You can assign random heights or a specific heightmap here
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    heightMap[x, z] = Mathf.PerlinNoise(x * 0.1f, z * 0.1f); // Example: Perlin noise
                }
            }
        }

        // Create the mesh and apply it to the MeshFilter
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
        GetComponent<MeshRenderer>().sharedMaterial = _meshMaterial;
    }

    void CreateMesh()
    {
        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6]; // 2 triangles per quad, 3 vertices per triangle

        // Generate vertices
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float y = heightMap[x, z] * scale; // Apply height scale
                vertices[z * width + x] = new Vector3(x, y, z);
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int topLeft = z * width + x;
                int bottomLeft = (z + 1) * width + x;

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

        // Apply mesh properties
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}


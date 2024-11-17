using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField]
    public MeshFilter PlaneMesh;

    private MeshRenderer _planeRenderer;

    public Dictionary<MapModes, Texture2D> Textures;

    private Vector2Int _startPoint;

    public Vector2Int StartPoint { get { return _startPoint; } set { _startPoint = value; } }

    private Vector2Int _endPoint;

    public Vector2Int EndPoint { get { return _endPoint; } set { _endPoint = value; } }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMapMode(MapModes mode)
    {
        GetComponent<MeshRenderer>().material.mainTexture = Textures[mode];
    }

    public void AddMapMode(Map map, MapModes mode, MapTextureGenerator mapTextureGenerator)
    {
        if (Textures == null)
        {
            Textures = new Dictionary<MapModes, Texture2D>();
        }
        if (!Textures.ContainsKey(mode))
        {
            Textures.Add(mode, mapTextureGenerator.CreateMapTexture(map, mode, StartPoint, EndPoint));
        }
    }
}

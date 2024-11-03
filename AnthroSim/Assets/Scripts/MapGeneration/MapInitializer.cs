using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    [SerializeField]
    bool _generateNewMap;

    [SerializeField]
    MapGenerator _mapGenerator;

    [SerializeField]
    Map _map;

    [SerializeField]
    int _mapWidth;

    [SerializeField]
    int _mapHeight;

    [SerializeField]
    [Tooltip("Refers to the number of pixels per world unit")]
    float _mapResolution;

    // Start is called before the first frame update
    void Start()
    {
        if (_generateNewMap)
        {
            _mapGenerator.GenerateMap(_map, _mapWidth, _mapHeight);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

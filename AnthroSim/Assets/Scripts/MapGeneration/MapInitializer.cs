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
    [Tooltip("Width of map in data points")]
    int _mapWidth;

    [SerializeField]
    [Tooltip("Height of map in data points")]
    int _mapHeight;

    private bool _initiated = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_generateNewMap && !_initiated)
        {
            _mapGenerator.GenerateMap(_map, _mapWidth, _mapHeight);
            _initiated = true;
        }
    }
}

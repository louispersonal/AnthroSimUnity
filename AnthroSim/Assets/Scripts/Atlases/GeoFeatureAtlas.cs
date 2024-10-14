using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoFeatureAtlas : MonoBehaviour
{
    public Dictionary<int, Mountain> Mountains;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetAvailableMountainID()
    {
        Mountains = new Dictionary<int, Mountain>();
        int lastID = Mountains.Count;
        Mountains.Add(lastID, new Mountain());
        return lastID;
    }
}

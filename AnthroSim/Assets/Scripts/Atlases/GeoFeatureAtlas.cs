using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoFeatureAtlas : MonoBehaviour
{
    public Dictionary<int, Mountain> Mountains;
    public Dictionary<int, Valley> Valleys;

    // Start is called before the first frame update
    void Start()
    {
        Mountains = new Dictionary<int, Mountain>();
        Valleys = new Dictionary<int, Valley>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetAvailableMountainID()
    {
        int lastID = Mountains.Count;
        Mountains.Add(lastID, new Mountain());
        return lastID;
    }

    public int GetAvailableValleyID()
    {
        int lastID = Valleys.Count;
        Valleys.Add(lastID, new Valley());
        return lastID;
    }
}

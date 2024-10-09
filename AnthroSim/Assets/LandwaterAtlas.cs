using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandwaterAtlas : MonoBehaviour
{
    public Dictionary<int, Continent> Continents;
    public Dictionary<int, River> Rivers;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetAvailableContinentID()
    {
        if (Continents == null)
        {
            Continents = new Dictionary<int, Continent>();
        }
        int lastID = Continents.Count;
        Continents.Add(lastID, new Continent());
        return lastID;
    }

    public int GetAvailableRiverID()
    {
        if (Rivers == null)
        {
            Rivers = new Dictionary<int, River>();
        }
        int lastID = Rivers.Count;
        Rivers.Add(lastID, new River());
        return lastID;
    }
}

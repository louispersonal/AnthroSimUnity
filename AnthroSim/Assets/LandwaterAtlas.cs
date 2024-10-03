using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandwaterAtlas : MonoBehaviour
{
    public Dictionary<int, Continent> Continents;

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
        Continents = new Dictionary<int, Continent>();
        int lastID = Continents.Count;
        Continents.Add(lastID, new Continent());
        return lastID;
    }
}

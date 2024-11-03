using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud
{
    float _waterLevel;
    float _waterLevelChange = 0.995f;

    public Cloud()
    {
        Reset();
    }

    public void Reset()
    {
        _waterLevel = 1;
    }

    public float Rain()
    {
        _waterLevel *= _waterLevelChange;
        _waterLevel = Mathf.Max(0f, _waterLevel);
        if (_waterLevel > 0)
        {
            return _waterLevel;
        }
        return 0;
    }

    public void Evaporate()
    {
        _waterLevel /= _waterLevelChange;
        _waterLevel = Mathf.Min(1f, _waterLevel);
    }
}

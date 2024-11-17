using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterManager : MonoBehaviour
{
    public GenerationParameters _generationParameters;

    // Start is called before the first frame update
    void Start()
    {
        GlobalParameters.WorldSizeKilometers = _generationParameters.WorldSizeKilometers;
        GlobalParameters.KilometersPerDataPoint = _generationParameters.KilometersPerDataPoint;
        GlobalParameters.KilometersPerWorldUnit = _generationParameters.KilometersPerWorldUnit;
        GlobalParameters.VertexSizeKilometers = _generationParameters.VertexSizeKilometers;
        GlobalParameters.NumContinents = _generationParameters.NumContinents;
        GlobalParameters.NumContinentEdgeVerticesRatio = _generationParameters.NumContinentEdgeVerticesRatio;
        GlobalParameters.MaxContinentEdgeDisplacementAngle = _generationParameters.MaxContinentEdgeDisplacementAngle;
        GlobalParameters.MinimumMountainRadius = _generationParameters.MinimumMountainRadius;
        GlobalParameters.MaximumMountainRadius = _generationParameters.MaximumMountainRadius;
        GlobalParameters.MountainRangeContinentAreaRatio = _generationParameters.MountainRangeContinentAreaRatio;
        GlobalParameters.MinimumValleyRadius = _generationParameters.MinimumValleyRadius;
        GlobalParameters.MaximumValleyRadius = _generationParameters.MaximumValleyRadius;
        GlobalParameters.MaxMountainsPerRange = _generationParameters.MaxMountainsPerRange;
        GlobalParameters.ChanceOfRiverSourceOnMountain = _generationParameters.ChanceOfRiverSourceOnMountain;
        GlobalParameters.NumRiverVerticesRatio = _generationParameters.NumRiverVerticesRatio;
        GlobalParameters.MaxRiverDisplacementAngle = _generationParameters.MaxRiverDisplacementAngle;
}

    // Update is called once per frame
    void Update()
    {
        
    }
}

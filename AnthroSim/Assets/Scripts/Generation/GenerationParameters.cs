using UnityEngine;

[CreateAssetMenu(fileName = "GenerationParameters", menuName = "Parameters")]
public class GenerationParameters : ScriptableObject
{
    public int WorldSizeKilometers;
    public int KilometersPerDataPoint;
    public int KilometersPerWorldUnit;
    public int VertexSizeKilometers;
    public int NumContinents;
    public float NumContinentEdgeVerticesRatio;
    public int MaxContinentEdgeDisplacementAngle;
    public int MinimumMountainRadius;
    public int MaximumMountainRadius;
    public float MountainRangeContinentAreaRatio;
    public int MinimumValleyRadius;
    public int MaximumValleyRadius;
    public int MaxMountainsPerRange;
    public float ChanceOfRiverSourceOnMountain;
    public float NumRiverVerticesRatio;
    public float MaxRiverDisplacementAngle;
    public float SeaLevel;
}

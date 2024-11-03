using UnityEngine;

[CreateAssetMenu(fileName = "GenerationParameters", menuName = "Parameters")]
public class GenerationParameters : ScriptableObject
{
    public int WorldSizeKilometers;
    public int KilometersPerWorldUnit;
    public int NumContinents;
    public int MinimumMountainRadius;
    public int MaximumMountainRadius;
    public int MinimumValleyRadius;
    public int MaximumValleyRadius;
    public int MaxMountainsPerRange;
    public float ChanceOfRiverSourceOnMountain;
    public int NumRiverVertices;
    public float MaxRiverDisplacementAngle;
}

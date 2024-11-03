using UnityEngine;

[CreateAssetMenu(fileName = "GenerationParameters", menuName = "Parameters")]
public class GenerationParameters : ScriptableObject
{
    public int _worldSizeKilometers;
    public int _kilometersPerWorldUnit;
    public int _minimumMountainRadius;
    public int _maximumMountainRadius;
    public int _maxMountainsPerRange;
    public float _chanceOfRiverSourceOnMountain;
}

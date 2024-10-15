using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class DiamondSquare : MonoBehaviour
{
    public void DiamondSquareAlgorithm(Map map, Vector2Int peakLocation, int mountainRadiusCoefficient, float peakHeight)
    {
        int mountainDiameter = (int)Mathf.Pow(2, mountainRadiusCoefficient) + 1;
        int step_size = mountainDiameter - 1;

        while (step_size > 1)
        {
            int half_step = step_size / 2;

            // Diamond Step
            for (int x = 0; x < mountainDiameter - 1; x += step_size)
            {
                for (int y = 0; y < mountainDiameter - 1; y += step_size)
                {
                    DiamondStep(map, peakLocation.x - mountainDiameter + x, peakLocation.y - mountainDiameter + y, step_size);
                }
            }

            // Square Step
            for (int x = 0; x < mountainDiameter - 1; x += half_step)
            {
                for (int y = (x + half_step) % step_size; y < mountainDiameter - 1; y += step_size)
                {
                    SquareStep(map, peakLocation.x - mountainDiameter + x, peakLocation.y - mountainDiameter + y, half_step, peakLocation, mountainDiameter);
                }
            }
        }
    }

    private void DiamondStep(Map map, int x, int y, int stepSize)
    {
        int halfStep = stepSize / 2;
        float avg = (map.MapData.Data[x, y].Height +
                     map.MapData.Data[x + stepSize, y].Height +
                     map.MapData.Data[x, y + stepSize].Height +
                     map.MapData.Data[x + stepSize, y + stepSize].Height) / 4.0f;

        map.MapData.Data[x + halfStep, y + halfStep].Height = avg;
    }

    private void SquareStep(Map map, int x, int y, int stepSize, Vector2Int peakLocation, int mountainDiameter)
    {
        int halfStep = stepSize / 2;

        float avg = 0f;
        int count = 0;

        if (x - halfStep >= peakLocation.x - mountainDiameter)
        {
            avg += map.MapData.Data[x - halfStep, y].Height;
            count++;
        }
        if (x + halfStep < peakLocation.x + mountainDiameter)
        {
            avg += map.MapData.Data[x + halfStep, y].Height;
            count++;
        }
        if (y - halfStep >= peakLocation.y - mountainDiameter)
        {
            avg += map.MapData.Data[x, y - halfStep].Height;
            count++;
        }
        if (y + halfStep < peakLocation.y + mountainDiameter)
        {
            avg += map.MapData.Data[x, y + halfStep].Height;
            count++;
        }

        avg /= count;

        map.MapData.Data[x, y].Height = avg;
    }
}

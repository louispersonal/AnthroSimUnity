using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CloudGenerator
{
    public static void CloudPass(Map map)
    {
        // Starting at the top of the map, going right, the cloud picks up water over water, and drops water over land
        // cloud direction changes at 0.25x the map height and 0.75x the map height

        int overlap = map.GetLength(1) / 16;

        Cloud cloud = new Cloud();
        for (int y = 0; y < (map.GetLength(1) / 4) + overlap; y++)
        {
            cloud.Reset();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map.GetLandWaterType(x, y) == LandWaterType.Continent)
                {
                    map.SetPrecipitation(x, y, map.GetPrecipitation(x, y) + cloud.Rain());
                }
                else
                {
                    cloud.Evaporate();
                }
            }
        }
        for (int y = map.GetLength(1) / 4; y < (map.GetLength(1) * 3 / 4) + overlap; y++)
        {
            cloud.Reset();
            for (int x = map.GetLength(0) - 1; x >= 0; x--)
            {
                if (map.GetLandWaterType(x, y) == LandWaterType.Continent)
                {
                    map.SetPrecipitation(x, y, map.GetPrecipitation(x, y) + cloud.Rain());
                }
                else
                {
                    cloud.Evaporate();
                }
            }
        }
        for (int y = map.GetLength(1) * 3 / 4; y < map.GetLength(1); y++)
        {
            cloud.Reset();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map.GetLandWaterType(x, y) == LandWaterType.Continent)
                {
                    map.SetPrecipitation(x, y, map.GetPrecipitation(x, y) + cloud.Rain());
                }
                else
                {
                    cloud.Evaporate();
                }
            }
        }
    }
}

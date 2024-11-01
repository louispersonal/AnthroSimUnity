using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MapTextureGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Texture2D CreateMapTexture(Map map, MapModes mode)
    {
        switch (mode)
        {
            case MapModes.Normal:
                return CreateNormalTexture(map);
            case MapModes.Temperature:
                return CreateTemperatureTexture(map);
            case MapModes.Precipitation:
                return CreatePrecipitationTexture(map);
            case MapModes.LowVegetation:
                return CreateLowVegetationTexture(map);
            case MapModes.HighVegetation:
                return CreateHighVegetationTexture(map);
        }
        return null;
    }

    public Texture2D CreateNormalTexture(Map map)
    {
        // Create a new Texture2D
        Texture2D texture = new Texture2D(map.GetLength(0), map.GetLength(1));

        // Loop through the array and set pixels
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Color color;
                if (map.GetLandWaterType(x, y) == LandWaterType.Ocean || map.GetLandWaterType(x, y) == LandWaterType.River)
                {
                    color = new Color(0, 0.3f, 0.7f);
                }
                else
                {
                    if (map.GetLowVegetation(x, y) < 0.1f && map.GetHighVegetation(x, y) < 0.1f)
                    {
                        if (map.GetTemperature(x, y) > 30f)
                        {
                            //desert
                            color = new Color(0.7f, 0.5f, 0f);
                        }
                        else
                        {
                            //tundra
                            color = new Color(0.45f, 0.45f, 0.45f);
                        }
                    }
                    else
                    {
                        if (map.GetLowVegetation(x, y) > map.GetHighVegetation(x, y))
                        {
                            // grassland
                            color = new Color(0.36f, 0.6f, 0.33f);
                        }
                        else
                        {
                            // forest
                            color = new Color(0.2f, 0.35f, 0.2f);
                        }
                    }
                }
                texture.SetPixel(x, y, color);
            }
        }

        // Apply all SetPixel calls
        texture.Apply();

        return texture;
    }

    public Texture2D CreateTemperatureTexture(Map map)
    {
        Texture2D texture = new Texture2D(map.GetLength(0), map.GetLength(1));

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                float value = (map.GetTemperature(x, y) + 20) / 60;
                Color color = new (value, 0, 0);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
    public Texture2D CreatePrecipitationTexture(Map map)
    {
        Texture2D texture = new Texture2D(map.GetLength(0), map.GetLength(1));

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Color color = new (0, 0, map.GetPrecipitation(x, y));
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
    public Texture2D CreateLowVegetationTexture(Map map)
    {
        Texture2D texture = new Texture2D(map.GetLength(0), map.GetLength(1));

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Color color = new (0, map.GetLowVegetation(x, y), 0);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
    public Texture2D CreateHighVegetationTexture(Map map)
    {
        Texture2D texture = new Texture2D(map.GetLength(0), map.GetLength(1));

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Color color = new (0, map.GetHighVegetation(x, y), 0);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
}

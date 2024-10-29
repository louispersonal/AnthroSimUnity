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
                if (map.MapData.Data[x, y].LandWaterType == LandWaterType.Ocean || map.MapData.Data[x, y].LandWaterType == LandWaterType.River)
                {
                    color = new Color(0, 0.3f, 0.7f);
                }
                else
                {
                    color = new Color(0.76f, 1f, 0.6f);
                    if (map.MapData.Data[x, y].LowVegetation > 0f)
                    {
                        float value = map.MapData.Data[x, y].LowVegetation;
                        color = new Color(0, value, 0);
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
                float value = (map.MapData.Data[x, y].Temperature + 20) / 60;
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
                Color color = new (0, 0, map.MapData.Data[x, y].Precipitation);
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
                Color color = new (0, map.MapData.Data[x, y].LowVegetation, 0);
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
                Color color = new (0, map.MapData.Data[x, y].HighVegetation, 0);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
}

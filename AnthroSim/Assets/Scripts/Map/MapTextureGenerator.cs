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

    public Texture2D CreateMapTexture(Map map, MapModes mode, Vector2Int startPoint, Vector2Int endPoint)
    {
        switch (mode)
        {
            case MapModes.Normal:
                return CreateNormalTexture(map, startPoint, endPoint);
            case MapModes.Temperature:
                return CreateTemperatureTexture(map, startPoint, endPoint);
            case MapModes.Precipitation:
                return CreatePrecipitationTexture(map, startPoint, endPoint);
            case MapModes.LowVegetation:
                return CreateLowVegetationTexture(map, startPoint, endPoint);
            case MapModes.HighVegetation:
                return CreateHighVegetationTexture(map, startPoint, endPoint);
        }
        return null;
    }

    public Texture2D CreateNormalTexture(Map map, Vector2Int startPoint, Vector2Int endPoint)
    {
        // Create a new Texture2D
        Texture2D texture = new Texture2D(endPoint.x - startPoint.x, endPoint.y - startPoint.y);

        // Loop through the array and set pixels
        for (int y = 0; y < endPoint.y - startPoint.y; y++)
        {
            for (int x = 0; x < endPoint.x - startPoint.x; x++)
            {
                Color color;
                if (map.GetLandWaterType(x + startPoint.x, y + startPoint.y) == LandWaterType.Ocean || map.GetLandWaterType(x + startPoint.x, y + startPoint.y) == LandWaterType.River)
                {
                    color = new Color(0, 0.3f, 0.7f);
                }
                else
                {
                    if (map.GetLowVegetation(x + startPoint.x, y + startPoint.y) < 0.1f && map.GetHighVegetation(x + startPoint.x, y + startPoint.y) < 0.1f)
                    {
                        if (map.GetTemperature(x + startPoint.x, y + startPoint.y) > 30f)
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
                        if (map.GetLowVegetation(x + startPoint.x, y + startPoint.y) > map.GetHighVegetation(x + startPoint.x, y + startPoint.y))
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

    public Texture2D CreateTemperatureTexture(Map map, Vector2Int startPoint, Vector2Int endPoint)
    {
        Texture2D texture = new Texture2D(endPoint.x - startPoint.x, endPoint.y - startPoint.y);

        for (int y = 0; y < endPoint.y - startPoint.y; y++)
        {
            for (int x = 0; x < endPoint.x - startPoint.x; x++)
            {
                float value = (map.GetTemperature(x + startPoint.x, y + startPoint.y) + 20) / 60;
                Color color = new (value, 0, 0);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
    public Texture2D CreatePrecipitationTexture(Map map, Vector2Int startPoint, Vector2Int endPoint)
    {
        Texture2D texture = new Texture2D(endPoint.x - startPoint.x, endPoint.y - startPoint.y);

        for (int y = 0; y < endPoint.y - startPoint.y; y++)
        {
            for (int x = 0; x < endPoint.x - startPoint.x; x++)
            {
                Color color = new (0, 0, map.GetPrecipitation(x + startPoint.x, y + startPoint.y));
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
    public Texture2D CreateLowVegetationTexture(Map map, Vector2Int startPoint, Vector2Int endPoint)
    {
        Texture2D texture = new Texture2D(endPoint.x - startPoint.x, endPoint.y - startPoint.y);

        for (int y = 0; y < endPoint.y - startPoint.y; y++)
        {
            for (int x = 0; x < endPoint.x - startPoint.x; x++)
            {
                Color color = new (0, map.GetLowVegetation(x + startPoint.x, y + startPoint.y), 0);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
    public Texture2D CreateHighVegetationTexture(Map map, Vector2Int startPoint, Vector2Int endPoint)
    {
        Texture2D texture = new Texture2D(endPoint.x - startPoint.x, endPoint.y - startPoint.y);

        for (int y = 0; y < endPoint.y - startPoint.y; y++)
        {
            for (int x = 0; x < endPoint.x - startPoint.x; x++)
            {
                Color color = new (0, map.GetHighVegetation(x + startPoint.x, y + startPoint.y), 0);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }
}

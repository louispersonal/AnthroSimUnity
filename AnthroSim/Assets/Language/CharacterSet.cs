using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class CharacterSet
{
    Dictionary<int, Character> characters;

    public void LoadCharactersFromFile(string filename)
    {
        characters = new Dictionary<int, Character>();
        string filePath = Path.Combine(Application.streamingAssetsPath, filename);
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] values = line.Split(',');
            characters.Add(int.Parse(values[0]), new Character(values[1]));
        }
    }

    public int GetIndex(Character character)
    {
        foreach (var pair in characters)
        {
            if (pair.Value.Content == character.Content)
            {
                return pair.Key;
            }
        }

        // Return -1 if the value is not found
        return -1;
    }

    public Character GetCharacter(int index)
    {
        return characters[index];
    }
}

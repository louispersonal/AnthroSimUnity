using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Language
{
    public CharacterSet CharacterSet;
    int[,] languageData;
    const int totalWeightPerRow = 100;
    const int maxWordLength = 8;
    public void LoadLanguageDataFromFile(string filename)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, filename);
        languageData = ReadCsvTo2DArray(filePath);
    }

    static int[,] ReadCsvTo2DArray(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        int rows = lines.Length;
        int columns = lines[0].Split(',').Length;

        int[,] array = new int[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            string[] values = lines[i].Split(',');

            for (int j = 0; j < columns; j++)
            {
                array[i, j] = int.Parse(values[j]);
            }
        }

        return array;
    }

    public string BuildWord()
    {
        Character currChar = new Character("START");
        Character nextChar = PickNextChar(currChar);
        string word = "";
        int wordLength = 0;
        while (nextChar.Content != "END" && nextChar.Content != "NOTFOUND" && wordLength < maxWordLength)
        {
            currChar = nextChar;
            word = word + currChar.Content;
            wordLength++;
            nextChar = PickNextChar(nextChar);
        }
        return word;
    }

    public Character PickNextChar(Character currChar)
    {
        int charIndex = CharacterSet.GetIndex(currChar);
        int random = Random.Range(1, totalWeightPerRow + 1);
        int sum = 0;
        for (int column = 0; column < languageData.GetLength(1); column++)
        {
            sum += languageData[charIndex, column];
            if (random <= sum)
            {
                return CharacterSet.GetCharacter(column);
            }
        }
        return new Character("NOTFOUND");
    }
}

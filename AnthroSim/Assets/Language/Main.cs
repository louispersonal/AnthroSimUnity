using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    Language language;
    CharacterSet characterSet;

    void Start()
    {
        language = new Language();
        language.LoadLanguageDataFromFile("languageData.csv");

        characterSet = new CharacterSet();
        characterSet.LoadCharactersFromFile("characters.csv");

        language.CharacterSet = characterSet;

        for (int i = 0;  i < 10; i++)
        {
            Debug.Log(language.BuildWord());
        }
    }
}

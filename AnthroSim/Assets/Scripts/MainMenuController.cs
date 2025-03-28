using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnNewGameSelected()
    {

    }

    public void OnLoadGameSelected()
    {

    }

    public void OnQuitGameSelected()
    {
        Application.Quit(); // Quits the game

        // If running in the Unity Editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

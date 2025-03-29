using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnNewGameSelected()
    {
        GameManager.Instance.LoadLoadingScreen();
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

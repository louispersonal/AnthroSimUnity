using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject _newGameSubMenu;

    public void OnNewGameSelected()
    {
        ToggleNewGameSubMenu();
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

    public void ToggleNewGameSubMenu()
    {
        _newGameSubMenu.SetActive(!_newGameSubMenu.activeSelf);
    }

    public void OnNewMapSelected()
    {

    }

    public void OnSavedMapSelected()
    {

    }
}

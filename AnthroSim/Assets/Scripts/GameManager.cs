using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Keep GameManager alive across scenes
            // Initially load the Main Menu scene
            LoadMainMenu();
            LoadMusic();
        }
        else
        {
            Destroy(gameObject);  // Destroy any duplicates
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Additive);
    }

    public void LoadMusic()
    {
        SceneManager.LoadScene("MusicScene", LoadSceneMode.Additive);
    }
}

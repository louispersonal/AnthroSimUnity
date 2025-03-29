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
    public void LoadLoadingScreen()
    {
        StartCoroutine(LoadLoadingScreenAsync());
    }

    private IEnumerator LoadLoadingScreenAsync()
    {
        // Load the loading screen additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreenScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // After the loading screen is done loading, load the main game scene
        UnloadMainMenuScene();
        LoadMainScene();
    }

    private void UnloadMainMenuScene()
    {
        SceneManager.UnloadSceneAsync("MainMenuScene");
    }

    private void LoadMainScene()
    {
        StartCoroutine(LoadMainSceneAsync());
    }

    private IEnumerator LoadMainSceneAsync()
    {
        // Start loading the main game scene in the background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainGameScene", LoadSceneMode.Additive);
        // Small wait so we can see if the loading screen is working
        yield return new WaitForSeconds(5f);
        while (!asyncLoad.isDone)
        {
            // You can update the loading screen UI here, if needed (e.g., a progress bar)
            yield return null;
        }

        // Unload the loading screen once everything is done
        SceneManager.UnloadSceneAsync("LoadingScreenScene");
    }
}

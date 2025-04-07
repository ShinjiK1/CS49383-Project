using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Call this to load any scene by name
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Optional: Call this to quit the game
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
    }
}

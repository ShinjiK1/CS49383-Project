using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public InputActionReference changeSceneButton;
    private int numScenes = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        changeSceneButton.action.Enable();
        changeSceneButton.action.performed += NextScene;
    }

    private void NextScene(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % numScenes);
    }
}

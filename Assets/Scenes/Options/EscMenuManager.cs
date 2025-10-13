using UnityEngine;
using UnityEngine.SceneManagement;

public class EscMenuManager : MonoBehaviour
{
    [SerializeField] private string optionsSceneName = "MenuScene"; // nome da tua cena de menu
    private bool isMenuOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isMenuOpen)
            {
                SceneManager.LoadScene(optionsSceneName, LoadSceneMode.Additive);
                isMenuOpen = true;
            }
            else
            {
                SceneManager.UnloadSceneAsync(optionsSceneName);
                isMenuOpen = false;
            }
        }
    }
}

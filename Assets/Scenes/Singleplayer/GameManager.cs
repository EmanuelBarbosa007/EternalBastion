using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Button restartButton;

    [Header("Audio Game Over")]
    public AudioSource audioSource; // Arrastar o Audio Source (pode ser o da camara ou um novo)
    public AudioClip defeatVoice;   // O som "Base Destroyed"

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    public void GameOver()
    {
        // Toca o áudio ANTES de pausar o tempo, embora o Time.timeScale = 0 
        if (audioSource != null && defeatVoice != null)
        {
            audioSource.PlayOneShot(defeatVoice);
        }

        Time.timeScale = 0f; // pausa o jogo

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // volta ao normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
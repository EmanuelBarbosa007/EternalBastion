using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

public class GameManager : MonoBehaviour
{
    [Header("UI Game Over")]
    public GameObject gameOverPanel;
    public Button restartButton;

    public TextMeshProUGUI currentWaveText; 
    public TextMeshProUGUI recordText;      

    [Header("Referências")]
    public EnemySpawner enemySpawner;

    [Header("Audio Game Over")]
    public AudioSource audioSource;
    public AudioClip defeatVoice;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    public void GameOver()
    {
        // Toca o áudio
        if (audioSource != null && defeatVoice != null)
        {
            audioSource.PlayOneShot(defeatVoice);
        }


        int waveReached = 0;
        if (enemySpawner != null)
        {
            waveReached = enemySpawner.GetCurrentWave();
        }

        // Cria uma chave única baseada no nome da cena (Ex: "Recorde_EasyMode", "Recorde_HardMode")
        string sceneName = SceneManager.GetActiveScene().name;
        string saveKey = "Recorde_" + sceneName;

        // Verifica o recorde antigo
        int bestWave = PlayerPrefs.GetInt(saveKey, 0);

        // Se a onda atual for maior que o recorde, atualiza e salva
        if (waveReached > bestWave)
        {
            bestWave = waveReached;
            PlayerPrefs.SetInt(saveKey, bestWave);
            PlayerPrefs.Save(); // Garante que fica guardado
        }

        // Atualiza os textos na tela de Game Over
        if (currentWaveText != null)
            currentWaveText.text = "Wave Reached: " + waveReached;

        if (recordText != null)
            recordText.text = "Current Record: " + bestWave;

        Time.timeScale = 0f; // pausa o jogo

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
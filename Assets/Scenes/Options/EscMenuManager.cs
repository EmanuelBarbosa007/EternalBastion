using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EscMenuManager : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Cena do Menu")]
    [SerializeField] private string mainMenuSceneName = "MenuScene";

    [Header("Referências Externas")]
    [SerializeField] private GameSpeedManager gameSpeedManager;

    private bool isPaused = false;
    public static bool IsGamePaused { get; private set; }

    void Start()
    {
        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (optionsPanel.activeSelf)
                    CloseOptions();
                else
                    ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        isPaused = false;
        IsGamePaused = false;

        // VERIFICAÇÃO IMPORTANTE: Só mexe no tempo se for Singleplayer
        if (!IsMultiplayerSession())
        {
            if (gameSpeedManager != null)
            {
                gameSpeedManager.ApplyCurrentSpeed();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    private void PauseGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        isPaused = true;
        IsGamePaused = true;


        // Se for Multiplayer, o menu abre mas o jogo continua.
        if (!IsMultiplayerSession())
        {
            Time.timeScale = 0f;
        }
    }

    // Função auxiliar para detetar se estamos em Multiplayer
    private bool IsMultiplayerSession()
    {
        // Se o NetworkManager existir E estivermos conectados (como Host ou Client)
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            return true;
        }
        return false;
    }

    public void OpenOptions()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Garante que o tempo volta ao normal antes de sair
        isPaused = false;
        IsGamePaused = false;

        if (NetworkManager.Singleton != null)
        {
            GameObject networkManagerGo = NetworkManager.Singleton.gameObject;
            NetworkManager.Singleton.Shutdown();
            Destroy(networkManagerGo);
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
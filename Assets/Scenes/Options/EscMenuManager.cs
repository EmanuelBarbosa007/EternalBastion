using UnityEngine;
using UnityEngine.SceneManagement;

public class EscMenuManager : MonoBehaviour
{
    [Header("Pain�is")]
    [SerializeField] private GameObject pauseMenuPanel; // painel principal de pausa
    [SerializeField] private GameObject optionsPanel;   // painel de op��es

    [Header("Cena do Menu")]
    [SerializeField] private string mainMenuSceneName = "MenuScene";

    private bool isPaused = false;

    // Propriedade est�tica para que outros scripts saibam se o jogo est� pausado
    public static bool IsGamePaused { get; private set; }

    void Start()
    {
        // Garante que o jogo come�a a correr e com os menus fechados
        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Se o menu de op��es estiver aberto, ESC fecha
                if (optionsPanel.activeSelf)
                {
                    CloseOptions();
                }
                else
                {
                    // Se s� o menu de pausa estiver aberto, fecha tudo e continua o jogo
                    ResumeGame();
                }
            }
            else
            {
                // Se o jogo n�o estava pausado, pausa-o
                PauseGame();
            }
        }
    }


    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false); // Garante que as op��es tamb�m fecham

        Time.timeScale = 1f; // Faz o tempo do jogo voltar ao normal
        isPaused = false;
        IsGamePaused = false;
    }

    public void OpenOptions()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false); // Esconde o menu de pausa

        if (optionsPanel != null)
            optionsPanel.SetActive(true);  // Mostra o de op��es
    }

    //bot�o "Back" (Voltar) DENTRO do painel de op��es.
 
    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false); // Esconde as op��es

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);  // Mostra o de pausa
    }

    // Esta � para o bot�o "Go to Main Menu".
    public void GoToMainMenu()
    {
        //despausar o jogo antes de sair da cena
        Time.timeScale = 1f;
        isPaused = false;
        IsGamePaused = false;

        // Carrega a cena do menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Esta fun��o � chamada pela primeira vez que se prime ESC.
    private void PauseGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f; // PAUSA O JOGO
        isPaused = true;
        IsGamePaused = true;
    }
}
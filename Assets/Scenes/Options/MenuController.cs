using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class MenuManager : MonoBehaviour
{
    [Header("Painéis Principais")]
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject gameModesMenu;

    [Header("Painéis de História")]
    public GameObject singleplayerStoryPanel;
    public GameObject multiplayerStoryPanel;

    [Header("UI Recordes (Arrastar os textos do Menu)")]
    public TextMeshProUGUI easyRecordText;   // Texto recorde Fácil
    public TextMeshProUGUI mediumRecordText; // Texto recorde Médio
    public TextMeshProUGUI hardRecordText;   // Texto recorde Difícil

    private void Start()
    {
        // Atualiza os recordes assim que o jogo abre
        UpdateRecordUI();
    }

    // Função para ler o PlayerPrefs e meter nos textos
    private void UpdateRecordUI()
    {
        // EASY MODE
        if (easyRecordText != null)
        {
            int record = PlayerPrefs.GetInt("Recorde_EasyMode", 0);
            easyRecordText.text = "Recorde: Onda " + record;
        }

        // MEDIUM MODE 
        if (mediumRecordText != null)
        {
            int record = PlayerPrefs.GetInt("Recorde_SampleScene", 0);
            mediumRecordText.text = "Recorde: Onda " + record;
        }

        // HARD MODE
        if (hardRecordText != null)
        {
            int record = PlayerPrefs.GetInt("Recorde_HardMode", 0);
            hardRecordText.text = "Recorde: Onda " + record;
        }
    }

    public void OpenGameModes()
    {
        mainMenu.SetActive(false);
        gameModesMenu.SetActive(true);

        // Atualiza quando abre este menu 
        UpdateRecordUI();
    }

    public void CloseGameModes()
    {
        gameModesMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    // --- Restante do teu código original igual ---

    public void OpenSingleplayerStory()
    {
        gameModesMenu.SetActive(false);
        singleplayerStoryPanel.SetActive(true);
    }

    public void OpenMultiplayerStory()
    {
        gameModesMenu.SetActive(false);
        multiplayerStoryPanel.SetActive(true);
    }

    public void BackToGameModes()
    {
        singleplayerStoryPanel.SetActive(false);
        multiplayerStoryPanel.SetActive(false);
        gameModesMenu.SetActive(true);
    }

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void BackToMain()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void StartStandardGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void EasyGame()
    {
        SceneManager.LoadScene("EasyMode");
    }

    public void HardGame()
    {
        SceneManager.LoadScene("HardMode");
    }

    public void MultiplayerGame()
    {
        SceneManager.LoadScene("MultiplayerScene");
    }

    public void PveGame()
    {
        SceneManager.LoadScene("PvEScene");
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}
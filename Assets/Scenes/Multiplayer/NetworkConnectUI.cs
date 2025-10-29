using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkConnectUI : MonoBehaviour
{

    public GameObject connectionPanel;
    public Button hostButton;
    public Button clientButton;

    void Start()
    {
        // Garante que o painel está visível no início
        if (connectionPanel != null)
            connectionPanel.SetActive(true);

        if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
        if (clientButton != null) clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        Debug.Log("Iniciando como Host...");
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartHost();
        else
            Debug.LogError("NetworkManager Singleton não encontrado!");

        HideConnectionPanel(); 
    }

    private void OnClientClicked()
    {
        Debug.Log("Conectando como Cliente...");
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartClient();
        else
            Debug.LogError("NetworkManager Singleton não encontrado!");

        HideConnectionPanel();
    }


    private void HideConnectionPanel()
    {

        if (connectionPanel != null)
        {
            connectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Connection Panel não atribuído no Inspector! Escondendo apenas os botões.");
            if (hostButton != null) hostButton.gameObject.SetActive(false);
            if (clientButton != null) clientButton.gameObject.SetActive(false);
        }
    }
}
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
        // Garante que o painel est� vis�vel no in�cio
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
            Debug.LogError("NetworkManager Singleton n�o encontrado!");

        HideConnectionPanel(); 
    }

    private void OnClientClicked()
    {
        Debug.Log("Conectando como Cliente...");
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartClient();
        else
            Debug.LogError("NetworkManager Singleton n�o encontrado!");

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
            Debug.LogWarning("Connection Panel n�o atribu�do no Inspector! Escondendo apenas os bot�es.");
            if (hostButton != null) hostButton.gameObject.SetActive(false);
            if (clientButton != null) clientButton.gameObject.SetActive(false);
        }
    }
}
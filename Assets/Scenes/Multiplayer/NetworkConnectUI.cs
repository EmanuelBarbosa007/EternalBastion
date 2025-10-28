using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkConnectUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;

    void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        Debug.Log("Iniciando como Host...");
        NetworkManager.Singleton.StartHost();
        HideButtons();
    }

    private void OnClientClicked()
    {
        Debug.Log("Conectando como Cliente...");
        NetworkManager.Singleton.StartClient();
        HideButtons();
    }

    private void HideButtons()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}
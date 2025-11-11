using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using TMPro;

public class NetworkConnectUI : MonoBehaviour
{
    [Header("Paineis")]
    public GameObject selectionPanel; // O 1º painel com "Host" e "Client"
    public GameObject hostPanel;      // O 2º painel para o Host (mostra IP)
    public GameObject clientPanel;    // O 3º painel para o Client (pede IP)

    [Header("Elementos do Painel de Seleção")]
    public Button selectHostButton;   // Botão "Host" no 1º painel
    public Button selectClientButton; // Botão "Client" no 1º painel

    [Header("Elementos do Painel de Host")]
    public TextMeshProUGUI hostInfoText; // Texto que diz "Waiting..." e o IP

    [Header("Elementos do Painel de Client")]
    public TMP_InputField ipAddressField; // Onde o cliente mete o IP
    public Button connectButton;      // O botão "Connect" no 3º painel

    void Start()
    {
        // --- Ligar os botões às funções corretas ---

        // Botões do Painel 1 (Seleção)
        if (selectHostButton != null) selectHostButton.onClick.AddListener(OnSelectHost);
        if (selectClientButton != null) selectClientButton.onClick.AddListener(OnSelectClient);

        // Botão do Painel 3 (Cliente)
        if (connectButton != null) connectButton.onClick.AddListener(OnConnect);

        // --- Estado Inicial ---
        // Garantir que SÓ o painel de seleção está visível
        selectionPanel.SetActive(true);
        hostPanel.SetActive(false);
        clientPanel.SetActive(false);

        // Definir um IP padrão (opcional)
        if (ipAddressField != null)
        {
            ipAddressField.text = "127.0.0.1";
        }
    }

    // --- Funções Chamadas pelos Botões ---

    // NOVO: Chamado pelo 'selectHostButton' (Painel 1)
    public void OnSelectHost()
    {
        Debug.Log("Modo Host selecionado. A iniciar...");

        // Trocar de painéis
        selectionPanel.SetActive(false);
        hostPanel.SetActive(true);

        // Obter IP local e mostrar no 'hostPanel'
        string localIP = GetLocalIPv4();
        if (hostInfoText != null)
        {
            hostInfoText.text = $"À espera de jogador...\nIP: {localIP}";
        }

        // Iniciar o Host
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartHost();
        else
            Debug.LogError("NetworkManager Singleton não encontrado!");
    }

    // NOVO: Chamado pelo 'selectClientButton' (Painel 1)
    public void OnSelectClient()
    {
        Debug.Log("Modo Client selecionado.");

        // Apenas troca os painéis. Não tenta ligar ainda.
        selectionPanel.SetActive(false);
        clientPanel.SetActive(true);
    }

    // MODIFICADO: Chamado pelo 'connectButton' (Painel 3)
    // Esta é a lógica que antes estava em OnClientClicked
    public void OnConnect()
    {
        // 1. Ler o IP que o jogador escreveu
        string ipAddress = "127.0.0.1";
        if (ipAddressField != null && !string.IsNullOrEmpty(ipAddressField.text))
        {
            ipAddress = ipAddressField.text;
        }

        Debug.Log($"A tentar conectar ao IP: {ipAddress}...");

        // 2. Configurar o Transport (UTP) com esse IP
        var utpTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (utpTransport != null)
        {
            utpTransport.SetConnectionData(ipAddress, (ushort)7777); // 7777 é a porta padrão
        }
        else
        {
            Debug.LogError("UnityTransport não encontrado no NetworkManager!");
            return;
        }

        // 3. Iniciar o Client
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartClient();
        else
            Debug.LogError("NetworkManager Singleton não encontrado!");


        clientPanel.SetActive(false);
    }



    // Função para descobrir o IP
    private string GetLocalIPv4()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Erro ao obter IP local: " + ex.Message);
        }
        return "N/A";
    }
}
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using System;

public class GameManagerMP : NetworkBehaviour
{
    public static GameManagerMP Instance;

    [Header("Refer�ncias de Jogo")]
    // Arraste as bases de JogadorA e JogadorB aqui
    public BaseHealthMP baseJogadorA;
    public BaseHealthMP baseJogadorB;

    [Header("Controlo de Estado")]
    // Esta vari�vel sincroniza com os clientes
    public NetworkVariable<bool> jogoIniciado = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> jogoAcabou = new NetworkVariable<bool>(false);

    [Header("UI Fim de Jogo (Server-side)")]
    public GameObject fimDeJogoPanel;
    public TextMeshProUGUI textoVencedor;
    public Button restartButton;

    [Header("UI de Espera (Client-side)")]
    public GameObject painelEsperandoJogador;

    // --- Configura��o do Singleton ---
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // --- L�gica de Conex�o (s� funciona no Host/Server) ---

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            // O Host (Jogador A) conecta-se
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            // Configura a sua pr�pria base
            baseJogadorA.donoDaBaseClientId = NetworkManager.Singleton.LocalClientId; // 0
        }

        if (IsClient)
        {
            // Todos os clientes (incluindo o Host) observam esta vari�vel
            jogoIniciado.OnValueChanged += HandleJogoIniciado;
            HandleJogoIniciado(false, jogoIniciado.Value); // Verifica o estado inicial
        }

        if (IsServer)
        {
            // O Servidor configura o UI de fim de jogo
            if (fimDeJogoPanel != null) fimDeJogoPanel.SetActive(false);
            if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Esta fun��o S� CORRE NO SERVER
        if (!IsServer) return;

        Debug.Log($"Cliente {clientId} conectado. Total: {NetworkManager.Singleton.ConnectedClients.Count}");

        // Se o Jogador B (clientId != 0) se conectou
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            // Configura a base do Jogador B
            baseJogadorB.donoDaBaseClientId = clientId;

            Debug.Log("Ambos os jogadores est�o conectados. Iniciando o jogo!");

            // Inicia o jogo para todos
            jogoIniciado.Value = true;
        }
    }

    // --- L�gica de UI (s� corre nos Clientes) ---

    private void HandleJogoIniciado(bool anterior, bool novo)
    {
        // Esta fun��o corre em TODOS OS CLIENTES
        if (novo == true)
        {
            // O jogo come�ou! Esconde o painel de espera.
            if (painelEsperandoJogador != null)
                painelEsperandoJogador.SetActive(false);
        }
        else
        {
            // O jogo ainda n�o come�ou. Mostra o painel.
            if (painelEsperandoJogador != null)
                painelEsperandoJogador.SetActive(true);
        }
    }

    // --- L�gica de Fim de Jogo (s� corre no Server) ---

    public void BaseDestruida(ulong clientIdDoJogadorQuePerdeu)
    {
        if (!IsServer || jogoAcabou.Value) return;

        jogoAcabou.Value = true;

        ulong vencedorClientId = (clientIdDoJogadorQuePerdeu == baseJogadorA.donoDaBaseClientId)
            ? baseJogadorB.donoDaBaseClientId
            : baseJogadorA.donoDaBaseClientId;

        string nomeVencedor = (vencedorClientId == 0) ? "Jogador A (Host)" : $"Jogador B (Client {vencedorClientId})";

        // Como o UI est� na cena do server, podemos ativ�-lo diretamente
        if (fimDeJogoPanel != null)
        {
            fimDeJogoPanel.SetActive(true);
            if (textoVencedor != null)
                textoVencedor.text = $"O {nomeVencedor} VENCEU!";
        }

        // Tamb�m podemos enviar um ClientRpc se o UI for mais complexo
        NotificarClientesDoVencedorClientRpc(nomeVencedor);
    }

    [ClientRpc]
    private void NotificarClientesDoVencedorClientRpc(string nomeVencedor)
    {
        // Este c�digo corre em TODOS os clientes
        Debug.Log($"Fim de Jogo! Vencedor: {nomeVencedor}");
        Time.timeScale = 0f; // Pausa o jogo para todos

        // Se o Host/Server tiver o seu pr�prio UI, ele n�o o deve mostrar aqui
        if (IsServer) return;

        // Clientes puros mostram o seu UI
        if (fimDeJogoPanel != null)
        {
            fimDeJogoPanel.SetActive(true);
            if (textoVencedor != null)
                textoVencedor.text = $"O {nomeVencedor} VENCEU!";
        }
    }

    // O Restart tem de ser gerido pelo NetworkManager
    private void RestartGame()
    {
        if (!IsServer) return;

        // Reinicia o timescale
        Time.timeScale = 1f;

        // Recarrega a cena para todos
        // NOTA: O SceneManager do Netcode � um pouco diferente
        NetworkManager.Singleton.SceneManager.LoadScene(
            SceneManager.GetActiveScene().name,
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
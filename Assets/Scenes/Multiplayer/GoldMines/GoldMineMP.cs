using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.EventSystems; // Necessário para OnMouseDown

// A tua struct de stats não precisa de mudar
[System.Serializable]
public class MineLevelStats
{
    [Tooltip("Custo para fazer upgrade PARA este nível")]
    public int upgradeCost;
    public int coinsPerInterval;
    public float generationInterval = 10f;
    public GameObject visualPrefab; // O prefab do modelo 3D
}

[RequireComponent(typeof(NetworkObject))]
public class GoldMineMP : NetworkBehaviour
{
    [Header("Atributos da Mina")]
    public NetworkVariable<int> level = new NetworkVariable<int>(1);
    public int maxLevel = 3;
    public MineLevelStats[] levelStats; // Liga isto no Inspector

    [Header("Referências")]
    public Transform visualContainer; // Liga o 'filho' vazio aqui

    // [HideInInspector]
    public ulong donoDaMinaClientId; // Definido pelo servidor
    // [HideInInspector]
    public int totalInvested; // Definido pelo servidor
    // [HideInInspector]
    public DebrisSpotMP myDebrisSpot; // Definido pelo servidor

    public override void OnNetworkSpawn()
    {
        // Regista a callback
        level.OnValueChanged += OnLevelChanged;
        // Corre uma vez no início
        OnLevelChanged(0, level.Value);

        // Apenas o SERVIDOR deve gerar moedas
        if (IsServer)
        {
            StartCoroutine(GenerateCoinsRoutine());
        }
    }

    public override void OnNetworkDespawn()
    {
        level.OnValueChanged -= OnLevelChanged;
    }

    /// <summary>
    /// Chamado em todos os clientes quando 'level' muda
    /// </summary>
    private void OnLevelChanged(int previousLevel, int newLevel)
    {
        ApplyLevelVisuals(newLevel);
    }

    /// <summary>
    /// Atualiza o modelo 3D da mina
    /// </summary>
    void ApplyLevelVisuals(int newLevel)
    {
        if (visualContainer == null) return;
        if (newLevel <= 0 || newLevel > levelStats.Length) return;

        MineLevelStats stats = levelStats[newLevel - 1];
        if (stats.visualPrefab == null) return;

        // Limpa o visual antigo
        foreach (Transform child in visualContainer)
        {
            Destroy(child.gameObject);
        }

        // Instancia o visual novo
        Instantiate(stats.visualPrefab, visualContainer.position, visualContainer.rotation, visualContainer);
    }

    /// <summary>
    /// CORRE APENAS NO SERVIDOR!
    /// </summary>
    IEnumerator GenerateCoinsRoutine()
    {
        if (!IsServer) yield break; // Segurança extra

        while (true)
        {
            MineLevelStats stats = levelStats[level.Value - 1];
            yield return new WaitForSeconds(stats.generationInterval);

            // Adiciona dinheiro ao dono da mina
            CurrencySystemMP.Instance.AddMoney(donoDaMinaClientId, stats.coinsPerInterval);
        }
    }

    /// <summary>
    /// Adiciona o clique na própria mina (como fizemos na torre)
    /// </summary>
    private void OnMouseDown()
    {
        // 1. Impede clique se o rato estiver sobre o UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 2. Verifica se o jogador local é o dono
        if (PlayerNetwork.LocalInstance == null) return;

        PlayerID localPlayerId = (PlayerNetwork.LocalInstance.OwnerClientId == 0)
                                 ? PlayerID.JogadorA
                                 : PlayerID.JogadorB;

        if (myDebrisSpot == null)
        {
            Debug.LogError("Mina não sabe a que spot pertence!");
            return;
        }

        if (myDebrisSpot.donoDoSpot != localPlayerId)
        {
            Debug.Log("Esta mina não é sua!");
            return;
        }

        // 3. Abre o painel de Upgrade
        if (MineUpgradeUIMP.Instance != null)
        {
            // Fecha outros painéis
            if (TowerPlacementUIMP.Instance != null)
                TowerPlacementUIMP.Instance.ClosePanel();
            if (TowerUpgradeUIMP.Instance != null)
                TowerUpgradeUIMP.Instance.ClosePanel();

            MineUpgradeUIMP.Instance.OpenPanel(this, myDebrisSpot);
        }
    }

    /// <summary>
    /// Tenta melhorar a mina. SÓ DEVE SER CHAMADO NO SERVIDOR.
    /// </summary>
    public void TryUpgrade(ulong playerClientId)
    {
        if (!IsServer) return;

        // Verifica se o jogador que pede é o dono
        if (playerClientId != donoDaMinaClientId) return;

        if (level.Value >= maxLevel) return;

        int upgradeCost = levelStats[level.Value].upgradeCost; // Custo para o *próximo* nível

        if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCost))
        {
            totalInvested += upgradeCost;
            level.Value++; // Isto sincroniza o nível e o visual para todos
        }
    }

    /// <summary>
    /// Usado pela UI para calcular o valor de venda
    /// </summary>
    public int GetSellValue()
    {
        // O teu script original usava 70%
        return Mathf.RoundToInt(totalInvested * 0.7f);
    }
}
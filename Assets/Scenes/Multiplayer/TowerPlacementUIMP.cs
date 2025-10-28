using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode; // Precisa disto

public class TowerPlacementUIMP : MonoBehaviour
{
    public static TowerPlacementUIMP Instance; // Singleton

    public GameObject panel;
    public Button closeButton;

    // (Pode adicionar os seus botões de torre aqui)
    // public Button buildArcherTowerButton;
    // public TowerBlueprint archerTowerBlueprint; // (ScriptableObject ou classe)
    // public int archerTowerPrefabId; // ID da lista de NetworkPrefabs

    private TowerSpotMP currentSpot;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // --- Exemplo de como ligar um botão ---
        // if (buildArcherTowerButton != null)
        // {
        //     buildArcherTowerButton.onClick.AddListener(() => {
        //         BuildTower(archerTowerBlueprint, archerTowerPrefabId);
        //     });
        // }
    }

    public void OpenPanel(TowerSpotMP spot)
    {
        currentSpot = spot;
        panel.SetActive(true);
        // (Pode atualizar o UI aqui para mostrar se tem dinheiro, etc.)
    }

    public void ClosePanel()
    {
        currentSpot = null;
        panel.SetActive(false);
    }

    // Chamado pelo seu botão de construir
    // (Precisa de uma forma de saber o 'prefabId' e o 'custo')
    public void BuildTower(int towerPrefabId, int cost)
    {
        if (currentSpot == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        // 1. Pede ao servidor para construir a torre
        PlayerNetwork.LocalInstance.RequestBuildTowerServerRpc(
            towerPrefabId,
            cost,
            currentSpot.transform.position,     // Posição onde construir
            currentSpot.NetworkObjectId         // ID do spot a ocupar
        );

        // 2. Fecha o painel
        ClosePanel();
    }
}
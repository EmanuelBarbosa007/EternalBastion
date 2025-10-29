using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TowerPlacementUIMP : MonoBehaviour
{
    public static TowerPlacementUIMP Instance; // Singleton

    public GameObject panel;
    public Button closeButton;

    // --- NOVO: LIGA OS TEUS BOTÕES E DEFINE OS VALORES NO INSPECTOR ---

    [Header("Torre Normal")]
    public Button buildNormalTowerButton;
    // Define o ID da lista de NetworkPrefabs (ex: 0)
    public int normalTowerPrefabId;
    // Define o custo (ex: 100)
    public int normalTowerCost;

    [Header("Torre de Fogo")]
    public Button buildFireTowerButton;
    // Define o ID da lista de NetworkPrefabs (ex: 1)
    public int fireTowerPrefabId;
    // Define o custo (ex: 150)
    public int fireTowerCost;

    [Header("Torre Perfurante (Piercing)")]
    public Button buildPiercingTowerButton;
    // Define o ID da lista de NetworkPrefabs (ex: 2)
    public int piercingTowerPrefabId;
    // Define o custo (ex: 200)
    public int piercingTowerCost;

    // --- Fim das novas variáveis ---

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

        // --- NOVO: Ligar os botões de construção ---
        if (buildNormalTowerButton != null)
        {
            buildNormalTowerButton.onClick.AddListener(() => {
                BuildTower(normalTowerPrefabId, normalTowerCost);
            });
        }

        if (buildFireTowerButton != null)
        {
            buildFireTowerButton.onClick.AddListener(() => {
                BuildTower(fireTowerPrefabId, fireTowerCost);
            });
        }

        if (buildPiercingTowerButton != null)
        {
            buildPiercingTowerButton.onClick.AddListener(() => {
                BuildTower(piercingTowerPrefabId, piercingTowerCost);
            });
        }
        // --- Fim da nova lógica de Start ---
    }

    public void OpenPanel(TowerSpotMP spot)
    {
        currentSpot = spot;
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        currentSpot = null;
        panel.SetActive(false);
    }

    // Chamado pelos seus botões de construir
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
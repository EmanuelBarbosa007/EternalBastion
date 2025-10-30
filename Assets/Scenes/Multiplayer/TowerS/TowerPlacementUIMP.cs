using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TowerPlacementUIMP : MonoBehaviour
{
    public static TowerPlacementUIMP Instance; // Singleton

    public GameObject panel;
    public Button closeButton;

    [Header("Torre Normal")]
    public Button buildNormalTowerButton;
    public int normalTowerPrefabId;
    public int normalTowerCost;

    [Header("Torre de Fogo")]
    public Button buildFireTowerButton;
    public int fireTowerPrefabId;
    public int fireTowerCost;

    [Header("Torre Perfurante (Piercing)")]
    public Button buildPiercingTowerButton;
    public int piercingTowerPrefabId;
    public int piercingTowerCost;

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

    // --- FUNÇÃO MODIFICADA ---
    public void BuildTower(int towerPrefabId, int cost)
    {
        if (currentSpot == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        // Define a posição de spawn com o offset
        Vector3 spawnPos = currentSpot.transform.position + new Vector3(0f, 2f, 0f);


        // 1. Pede ao servidor para construir a torre
        PlayerNetwork.LocalInstance.RequestBuildTowerServerRpc(
            towerPrefabId,
            cost,
            spawnPos, // <<< Passa a nova posição com offset
            currentSpot.NetworkObjectId
        );

        // 2. Fecha o painel
        ClosePanel();
    }
}
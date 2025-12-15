using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class TowerPlacementUIMP : MonoBehaviour
{
    public static TowerPlacementUIMP Instance;

    public GameObject panel;
    public Button closeButton;

    [Header("Configurações de UI")]
    [Tooltip("Tempo em segundos que os botões ficam bloqueados ao abrir o painel (evita cliques acidentais)")]
    public float inputDelay = 0.3f;

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

    [Header("Audio")]
    public AudioClip actionSound; // Som de Construção
    [Range(0f, 1f)] public float soundVolume = 1f;

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
        StartCoroutine(EnableButtonsRoutine());
    }

    IEnumerator EnableButtonsRoutine()
    {
        SetButtonsInteractable(false);
        yield return new WaitForSeconds(inputDelay);
        SetButtonsInteractable(true);
    }

    void SetButtonsInteractable(bool state)
    {
        if (buildNormalTowerButton != null) buildNormalTowerButton.interactable = state;
        if (buildFireTowerButton != null) buildFireTowerButton.interactable = state;
        if (buildPiercingTowerButton != null) buildPiercingTowerButton.interactable = state;
        if (closeButton != null) closeButton.interactable = state;
    }

    public void ClosePanel()
    {
        currentSpot = null;
        panel.SetActive(false);
    }

    public void BuildTower(int towerPrefabId, int cost)
    {
        if (currentSpot == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        // Usar GetMoney com o ID do jogador local
        ulong myClientId = PlayerNetwork.LocalInstance.OwnerClientId;
        int myMoney = CurrencySystemMP.Instance.GetMoney(myClientId);

        if (myMoney < cost)
        {
            Debug.Log("Dinheiro insuficiente!");
            return;
        }

        // Tocar Som (apenas se tiver dinheiro e for construir)
        if (actionSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(actionSound, Camera.main.transform.position, soundVolume);
        }

        Vector3 spawnPos = currentSpot.transform.position + new Vector3(0f, 2f, 0f);

        PlayerNetwork.LocalInstance.RequestBuildTowerServerRpc(
            towerPrefabId,
            cost,
            spawnPos,
            currentSpot.NetworkObjectId
        );

        ClosePanel();
    }
}
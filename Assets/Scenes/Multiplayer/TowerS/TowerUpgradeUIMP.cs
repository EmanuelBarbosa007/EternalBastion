using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections;

public class TowerUpgradeUIMP : MonoBehaviour
{
    public static TowerUpgradeUIMP Instance;

    public GameObject uiPanel;
    public Button upgradeButton;
    public Button sellButton;
    public Button closeButton;

    [Header("Configurações UI")]
    [Tooltip("Tempo de espera para evitar cliques acidentais ao abrir")]
    public float inputDelay = 0.3f;

    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;
    public TextMeshProUGUI towerNameText;

    [Header("Audio")]
    public AudioClip actionSound; // Som de Upgrade
    [Range(0f, 1f)] public float soundVolume = 1f;

    private TowerMP currentTower;
    private TowerSpotMP currentSpot;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeTower);

        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Update()
    {
        if (!uiPanel.activeInHierarchy || currentTower == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == currentTower.gameObject)
                    return;
            }

            ClosePanel();
        }
    }

    public void OpenPanel(TowerMP tower, TowerSpotMP spot)
    {
        currentTower = tower;
        currentSpot = spot;
        uiPanel.SetActive(true);

        UpdateUI_Texts();
        StartCoroutine(EnableButtonsRoutine());
    }

    IEnumerator EnableButtonsRoutine()
    {
        if (upgradeButton != null) upgradeButton.interactable = false;
        if (sellButton != null) sellButton.interactable = false;
        if (closeButton != null) closeButton.interactable = false;

        yield return new WaitForSeconds(inputDelay);

        if (sellButton != null) sellButton.interactable = true;
        if (closeButton != null) closeButton.interactable = true;

        if (currentTower != null && upgradeButton != null)
        {
            if (currentTower.level.Value < 3)
            {
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.interactable = false;
            }
        }
    }

    public void ClosePanel()
    {
        uiPanel.SetActive(false);
        currentTower = null;
        currentSpot = null;
    }

    void UpdateUI_Texts()
    {
        if (currentTower == null) return;

        if (towerNameText != null)
            towerNameText.text = $"{currentTower.towerName} (Level {currentTower.level.Value})";

        int sellAmount = currentTower.totalInvested / 2;
        if (sellValueText != null)
            sellValueText.text = $"Vender\n{sellAmount} Moedas";

        if (upgradeCostText != null)
        {
            if (currentTower.level.Value == 1)
                upgradeCostText.text = $"Melhorar\n{currentTower.upgradeCostLevel2} Moedas";
            else if (currentTower.level.Value == 2)
                upgradeCostText.text = $"Melhorar\n{currentTower.upgradeCostLevel3} Moedas";
            else
                upgradeCostText.text = "NÍVEL MÁXIMO";
        }
    }

    private void UpgradeTower()
    {
        if (currentTower == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        // Determina o custo
        int cost = 0;
        if (currentTower.level.Value == 1) cost = currentTower.upgradeCostLevel2;
        else if (currentTower.level.Value == 2) cost = currentTower.upgradeCostLevel3;

        //  Usar GetMoney com o ID do jogador local ---
        ulong myClientId = PlayerNetwork.LocalInstance.OwnerClientId;
        int myMoney = CurrencySystemMP.Instance.GetMoney(myClientId);

        if (myMoney < cost)
        {
            return; // Não tem dinheiro, sai da função
        }

        // Tocar Som
        if (actionSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(actionSound, Camera.main.transform.position, soundVolume);
        }

        PlayerNetwork.LocalInstance.RequestUpgradeTowerServerRpc(
            currentTower.NetworkObjectId
        );

        ClosePanel();
    }

    private void SellTower()
    {
        if (currentTower == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        PlayerNetwork.LocalInstance.RequestSellTowerServerRpc(
            currentTower.NetworkObjectId,
            currentSpot.NetworkObjectId
        );

        ClosePanel();
    }
}
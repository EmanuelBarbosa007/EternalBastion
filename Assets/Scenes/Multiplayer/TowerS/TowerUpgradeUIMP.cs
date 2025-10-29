using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class TowerUpgradeUIMP : MonoBehaviour
{
    public static TowerUpgradeUIMP Instance; // Singleton

    public GameObject uiPanel;
    public Button upgradeButton;
    public Button sellButton;
    public Button closeButton;


    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;

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

        // Liga os botões às funções de RPC
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeTower);

        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel(TowerMP tower, TowerSpotMP spot)
    {
        currentTower = tower;
        currentSpot = spot;
        uiPanel.SetActive(true);

        // Atualiza o UI com a informação da torre
        UpdateUI();
    }

    public void ClosePanel()
    {
        uiPanel.SetActive(false);
        currentTower = null;
        currentSpot = null;
    }

    // Função para atualizar os botões
    void UpdateUI()
    {
        if (currentTower == null) return;

        // Lógica de Venda
        int sellAmount = currentTower.totalInvested / 2;
        sellValueText.text = $"Vender\n{sellAmount} Moedas";

        // Lógica de Upgrade
        if (currentTower.level.Value == 1)
        {
            upgradeCostText.text = $"Melhorar\n{currentTower.upgradeCostLevel2} Moedas";
            upgradeButton.interactable = true;
        }
        else if (currentTower.level.Value == 2)
        {
            upgradeCostText.text = $"Melhorar\n{currentTower.upgradeCostLevel3} Moedas";
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeCostText.text = "NÍVEL MÁXIMO";
            upgradeButton.interactable = false;
        }

    }


    private void UpgradeTower()
    {
        if (currentTower == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        // Pede ao servidor para fazer o upgrade
        PlayerNetwork.LocalInstance.RequestUpgradeTowerServerRpc(
            currentTower.NetworkObjectId
        );

        ClosePanel();
    }

    private void SellTower()
    {
        if (currentTower == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        // Pede ao servidor para vender a torre
        PlayerNetwork.LocalInstance.RequestSellTowerServerRpc(
            currentTower.NetworkObjectId,
            currentSpot.NetworkObjectId
        );

        ClosePanel();
    }
}
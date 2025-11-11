using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MineUpgradeUIMP : MonoBehaviour
{
    public static MineUpgradeUIMP Instance;

    public GameObject panel;
    public Button upgradeButton;
    public Button sellButton;
    public Button closeButton; // Opcional, mas recomendado

    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;
    public TextMeshProUGUI statsText;
    // public TextMeshProUGUI mineNameText; // Se quiseres, como na torre

    private GoldMineMP currentMine;
    private DebrisSpotMP currentSpot;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        panel.SetActive(false);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        sellButton.onClick.AddListener(OnSellClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    // Lógica para fechar ao clicar fora
    void Update()
    {
        if (!panel.activeInHierarchy || currentMine == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == currentMine.gameObject)
                    return;
            }
            ClosePanel();
        }
    }

    public void OpenPanel(GoldMineMP mine, DebrisSpotMP spot)
    {
        currentMine = mine;
        currentSpot = spot;
        UpdateUI();
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentMine = null;
        currentSpot = null;
    }

    public void UpdateUI()
    {
        if (currentMine == null) return;

        int currentLevel = currentMine.level.Value;
        var currentStats = currentMine.levelStats[currentLevel - 1];

        // if (mineNameText != null) 
        //     mineNameText.text = $"Mina de Ouro (Nível {currentLevel})";

        statsText.text = $"Gera: {currentStats.coinsPerInterval} moedas\na cada {currentStats.generationInterval} seg.";
        sellValueText.text = $"Vender\n+{currentMine.GetSellValue()}";

        if (currentLevel >= currentMine.maxLevel)
        {
            upgradeButton.interactable = false;
            upgradeCostText.text = "NÍVEL MÁX.";
        }
        else
        {
            int upgradeCost = currentMine.levelStats[currentLevel].upgradeCost;
            upgradeCostText.text = $"Melhorar\n{upgradeCost}";

            bool hasMoney = false;
            if (PlayerNetwork.LocalInstance != null)
                hasMoney = CurrencySystemMP.Instance.GetMoney(PlayerNetwork.LocalInstance.OwnerClientId) >= upgradeCost;

            upgradeButton.interactable = hasMoney;
        }
    }

    private void OnUpgradeClicked()
    {
        if (currentMine == null) return;
        PlayerNetwork.LocalInstance.RequestUpgradeMineServerRpc(currentMine.NetworkObjectId);
        ClosePanel();
    }

    private void OnSellClicked()
    {
        if (currentMine == null) return;
        PlayerNetwork.LocalInstance.RequestSellMineServerRpc(currentMine.NetworkObjectId, currentSpot.NetworkObjectId);
        ClosePanel();
    }
}
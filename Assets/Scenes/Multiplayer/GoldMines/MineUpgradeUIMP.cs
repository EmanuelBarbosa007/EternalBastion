using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections; // Necessário para IEnumerator

public class MineUpgradeUIMP : MonoBehaviour
{
    public static MineUpgradeUIMP Instance;

    public GameObject panel;
    public Button upgradeButton;
    public Button sellButton;
    public Button closeButton;

    [Header("Configurações UI")]
    public float inputDelay = 0.3f; // Tempo de segurança

    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;
    public TextMeshProUGUI statsText;

    [Header("Audio")] 
    public AudioClip actionSound; // Som de Upgrade
    [Range(0f, 1f)] public float soundVolume = 1f;

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

        panel.SetActive(true);

        // Atualiza textos
        UpdateUI_Texts();

        // Inicia segurança dos botões
        StartCoroutine(EnableButtonsRoutine());
    }

    // --- CORROTINA DE SEGURANÇA ---
    IEnumerator EnableButtonsRoutine()
    {
        // 1. Bloqueia tudo
        if (upgradeButton != null) upgradeButton.interactable = false;
        if (sellButton != null) sellButton.interactable = false;
        if (closeButton != null) closeButton.interactable = false;

        // 2. Espera
        yield return new WaitForSeconds(inputDelay);

        // 3. Reativa Vender e Fechar
        if (sellButton != null) sellButton.interactable = true;
        if (closeButton != null) closeButton.interactable = true;

        // 4. Lógica Inteligente do botão Upgrade
        if (currentMine != null && upgradeButton != null)
        {
            int currentLevel = currentMine.level.Value;

            // Se já for nível máximo, mantém desligado
            if (currentLevel >= currentMine.maxLevel)
            {
                upgradeButton.interactable = false;
            }
            else
            {
                // Se não for máx, verifica se tem dinheiro
                int upgradeCost = currentMine.levelStats[currentLevel].upgradeCost;
                bool hasMoney = false;

                if (PlayerNetwork.LocalInstance != null)
                    hasMoney = CurrencySystemMP.Instance.GetMoney(PlayerNetwork.LocalInstance.OwnerClientId) >= upgradeCost;

                upgradeButton.interactable = hasMoney;
            }
        }
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentMine = null;
        currentSpot = null;
    }

    // Separei os textos para serem atualizados instantaneamente
    public void UpdateUI_Texts()
    {
        if (currentMine == null) return;

        int currentLevel = currentMine.level.Value;

        // Proteção caso o nível esteja dessincronizado
        int statsIndex = Mathf.Clamp(currentLevel - 1, 0, currentMine.levelStats.Length - 1);
        var currentStats = currentMine.levelStats[statsIndex];

        if (statsText != null)
            statsText.text = $"Generate: {currentStats.coinsPerInterval} coins\neach {currentStats.generationInterval} sec.";

        if (sellValueText != null)
            sellValueText.text = $"Sell\n+{currentMine.GetSellValue()}";

        if (upgradeCostText != null)
        {
            if (currentLevel >= currentMine.maxLevel)
            {
                upgradeCostText.text = "Max Level";
            }
            else
            {
                // Verifica se o array tem o próximo nível
                if (currentLevel < currentMine.levelStats.Length)
                {
                    int upgradeCost = currentMine.levelStats[currentLevel].upgradeCost;
                    upgradeCostText.text = $"Upgrade\n{upgradeCost}";
                }
            }
        }
    }

    private void OnUpgradeClicked()
    {
        if (currentMine == null) return;

        // Verificação local de dinheiro antes de tocar o som
        if (PlayerNetwork.LocalInstance != null)
        {
            int currentLevel = currentMine.level.Value;
            if (currentLevel < currentMine.maxLevel)
            {
                int upgradeCost = currentMine.levelStats[currentLevel].upgradeCost;
                if (CurrencySystemMP.Instance.GetMoney(PlayerNetwork.LocalInstance.OwnerClientId) < upgradeCost)
                    return;
            }
        }

        // NOVO: Tocar Som 
        if (actionSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(actionSound, Camera.main.transform.position, soundVolume);
        }

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
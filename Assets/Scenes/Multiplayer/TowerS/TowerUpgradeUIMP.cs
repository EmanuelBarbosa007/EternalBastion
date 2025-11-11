using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.EventSystems; // Importa o EventSystems

public class TowerUpgradeUIMP : MonoBehaviour
{
    public static TowerUpgradeUIMP Instance; // Singleton

    public GameObject uiPanel;
    public Button upgradeButton;
    public Button sellButton;
    public Button closeButton;

    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;

    // <<< LIGA ISTO NO INSPECTOR >>>
    // (O script que tu colaste na tua mensagem não tinha esta linha)
    public TextMeshProUGUI towerNameText;

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

    // <<< CORREÇÃO: Lógica de fechar ao clicar fora >>>
    private void Update()
    {
        // Se o painel não está ativo ou não há torre selecionada, não faz nada
        if (!uiPanel.activeInHierarchy || currentTower == null)
            return;

        // Se o jogador clicar com o botão esquerdo
        if (Input.GetMouseButtonDown(0))
        {
            // Verifica se o clique foi em cima de um elemento de UI (o painel, um botão, etc.)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Foi num UI, por isso não fecha
                return;
            }

            // NOVO: Raycast para ver se clicámos na torre que está selecionada
            RaycastHit hit;
            // Dispara um raio da câmara para a posição do rato
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Se o raio acertar em alguma coisa
            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Se o objeto em que acertámos é o GameObject da torre atual...
                if (hit.collider.gameObject == currentTower.gameObject)
                {
                    // ...ignora o clique (foi o clique que abriu o painel).
                    return;
                }
            }

            // Se chegou aqui, clicou fora do UI e fora da torre selecionada. Fecha o painel.
            ClosePanel();
        }
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

        // <<< NOVO: Atualiza o texto do nome e nível da torre >>>
        // (O script que tu colaste na tua mensagem não tinha isto)
        if (towerNameText != null)
        {
            towerNameText.text = $"{currentTower.towerName} (Level {currentTower.level.Value})";
        }
        else
        {
            // Aviso caso te tenhas esquecido de ligar no Inspector
            Debug.LogWarning("TowerNameText não está ligado no Inspector do TowerUpgradeUIMP!");
        }


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
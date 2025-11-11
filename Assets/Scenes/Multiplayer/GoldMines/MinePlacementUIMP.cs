using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinePlacementUIMP : MonoBehaviour
{
    public static MinePlacementUIMP Instance; // Singleton

    public GameObject panel;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI costText;

    private DebrisSpotMP currentDebrisSpot;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        panel.SetActive(false);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    public void OpenPanel(DebrisSpotMP spot)
    {
        currentDebrisSpot = spot;
        costText.text = "Construir Mina?\nCusto: " + spot.buildCost;

        // NOVO: Verifica o dinheiro no sistema de MP
        bool hasMoney = false;
        if (PlayerNetwork.LocalInstance != null)
        {
            hasMoney = CurrencySystemMP.Instance.GetMoney(PlayerNetwork.LocalInstance.OwnerClientId) >= spot.buildCost;
        }
        yesButton.interactable = hasMoney;

        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentDebrisSpot = null;
    }

    private void OnYesClicked()
    {
        if (currentDebrisSpot == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        // NOVO: Envia o pedido ao servidor
        PlayerNetwork.LocalInstance.RequestBuildMineServerRpc(
            currentDebrisSpot.goldMinePrefabId,
            currentDebrisSpot.buildCost,
            currentDebrisSpot.transform.position,
            currentDebrisSpot.NetworkObjectId
        );

        ClosePanel();
    }

    private void OnNoClicked()
    {
        ClosePanel();
    }
}
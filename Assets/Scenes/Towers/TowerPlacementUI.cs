using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TowerPlacementUI : MonoBehaviour
{
    public static TowerPlacementUI Instance;

    [Header("UI")]
    public GameObject panel;
    public Button basicTowerButton;
    public Button fireTowerButton;
    public Button piercingTowerButton;
    public Button closeButton; 

    [Header("Prefabs das torres")]
    public GameObject basicTowerPrefab;
    public GameObject fireTowerPrefab;
    public GameObject piercingTowerPrefab;

    [Header("Custos das torres")]
    public int basicTowerCost = 50;
    public int fireTowerCost = 100;
    public int piercingTowerCost = 150;

    private TowerSpot currentSpot;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        basicTowerButton.onClick.AddListener(() => BuildTower(basicTowerPrefab, basicTowerCost));
        fireTowerButton.onClick.AddListener(() => BuildTower(fireTowerPrefab, fireTowerCost));
        piercingTowerButton.onClick.AddListener(() => BuildTower(piercingTowerPrefab, piercingTowerCost));
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel(TowerSpot spot)
    {
        // Se o painel já estiver aberto, ignora selecionar outro slot
        if (panel.activeSelf)
            return;

        currentSpot = spot;
        StartCoroutine(OpenPanelNextFrame());
    }


    private IEnumerator OpenPanelNextFrame()
    {
        yield return null; // espera 1 frame
        panel.SetActive(true);
    }


    public void ClosePanel()
    {
        panel.SetActive(false);
        currentSpot = null;
    }

    void BuildTower(GameObject towerPrefab, int cost)
    {
        if (currentSpot == null) return;

        if (CurrencySystem.Money >= cost)
        {
            CurrencySystem.SpendMoney(cost);
            GameObject newTower = Instantiate(towerPrefab, currentSpot.transform.position, Quaternion.identity);

            // aumenta a escala da torre
            newTower.transform.localScale *= 1.2f;

            currentSpot.isOccupied = true;
        }
        else
        {
            Debug.Log("Moedas insuficientes para comprar esta torre!");
        }

        ClosePanel();
    }
}

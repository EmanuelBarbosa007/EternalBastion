using UnityEngine;
using System.Collections;

public class GoldMine : MonoBehaviour
{
    [Header("Atributos da Mina")]
    public int currentLevel = 1;
    public int maxLevel = 3;
    public MineLevelStats[] levelStats;

    [Header("Referências")]
    public GameObject debrisSpotPrefab; // Prefab dos destroços (para quando for vendida)
    private MineUpgradeUI upgradeUI;

    // Stats Atuais
    private int coinsPerInterval;
    private float generationInterval;
    private int totalCostSpent; // Para calcular o valor de venda

    void Start()
    {
        // Encontra a UI de upgrade
        upgradeUI = FindAnyObjectByType<MineUpgradeUI>();

        // O custo inicial (totalCostSpent) é agora definido via InitializeCost()
        // chamado pelo DebrisSpot quando a mina é construída.

        ApplyLevelStats();
        StartCoroutine(GenerateCoinsRoutine());
    }

    // Método chamado pelo DebrisSpot ao construir
    public void InitializeCost(int initialCost)
    {
        totalCostSpent = initialCost;
    }

    void OnMouseDown()
    {
        if (TowerUpgradeUI.Instance != null && TowerUpgradeUI.Instance.IsPanelActive())
        {
            return;
        }

        if (upgradeUI != null)
        {
            upgradeUI.OpenPanel(this);
        }
    }

    void ApplyLevelStats()
    {
        if (currentLevel > 0 && currentLevel <= levelStats.Length)
        {
            MineLevelStats stats = levelStats[currentLevel - 1];
            coinsPerInterval = stats.coinsPerInterval;
            generationInterval = stats.generationInterval;
        }
    }

    IEnumerator GenerateCoinsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(generationInterval);

            // Usa o método estático
            CurrencySystem.AddMoney(coinsPerInterval);
        }
    }

    public void Upgrade()
    {
        if (currentLevel >= maxLevel) return;

        int upgradeCost = GetNextUpgradeCost();

        // Verifica e gasta usando os métodos estáticos
        if (CurrencySystem.SpendMoney(upgradeCost))
        {
            currentLevel++;
            totalCostSpent += upgradeCost;
            ApplyLevelStats();

            upgradeUI.UpdateUI();
            Debug.Log("Mina melhorada para nível " + currentLevel);
        }
        else
        {
            Debug.Log("Não tem moedas suficientes para o upgrade.");
            // TODO: Mostrar mensagem ao jogador
        }
    }

    public void Sell()
    {
        int sellAmount = Mathf.RoundToInt(totalCostSpent * 0.7f);

        // Usa o método estático
        CurrencySystem.AddMoney(sellAmount);

        if (debrisSpotPrefab != null)
        {
            Instantiate(debrisSpotPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    // --- Métodos Públicos para a UI ---

    public int GetNextUpgradeCost()
    {
        if (currentLevel >= maxLevel) return 0;
        return levelStats[currentLevel].upgradeCost;
    }

    public int GetCurrentProduction()
    {
        return coinsPerInterval;
    }

    public float GetCurrentInterval()
    {
        return generationInterval;
    }

    public int GetSellValue()
    {
        return Mathf.RoundToInt(totalCostSpent * 0.7f);
    }
}

// (A struct MineLevelStats permanece igual)
[System.Serializable]
public class MineLevelStats
{
    [Tooltip("Custo para fazer upgrade PARA este nível")]
    public int upgradeCost;
    public int coinsPerInterval;
    public float generationInterval = 10f;
    public GameObject visualPrefab;
}
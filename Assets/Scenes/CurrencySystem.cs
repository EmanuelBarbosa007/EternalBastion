using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencySystem : MonoBehaviour
{
    public static int Money { get; private set; }

    public int startingMoney = 100;
    public TextMeshProUGUI moneyText;

    void Start()
    {
        Money = startingMoney;
        UpdateUI();
    }

    public static void AddMoney(int amount)
    {
        Money += amount;

        CurrencySystem instance = Object.FindFirstObjectByType<CurrencySystem>();
        if (instance != null)
            instance.UpdateUI();
    }

    public static bool SpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            CurrencySystem instance = Object.FindFirstObjectByType<CurrencySystem>();
            if (instance != null)
                instance.UpdateUI();
            return true;
        }
        return false; // não tem dinheiro suficiente
    }

    void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "Moedas: " + Money;
    }
}

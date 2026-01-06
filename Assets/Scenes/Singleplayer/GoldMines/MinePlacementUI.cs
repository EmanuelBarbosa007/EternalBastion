using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinePlacementUI : MonoBehaviour
{
    public GameObject panel;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI costText;

    [Header("Audio")] 
    public AudioClip actionSound; // Som de Construção 
    [Range(0f, 1f)] public float soundVolume = 1f;

    private DebrisSpot currentDebrisSpot;

    void Start()
    {
        panel.SetActive(false);

        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    public void OpenPanel(DebrisSpot spot)
    {
        currentDebrisSpot = spot;
        costText.text = "Cost: " + spot.buildCost;

        // Verifica o botão usando a variável estática
        yesButton.interactable = (CurrencySystem.Money >= spot.buildCost);

        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentDebrisSpot = null;
    }

    private void OnYesClicked()
    {
        if (currentDebrisSpot != null && CurrencySystem.SpendMoney(currentDebrisSpot.buildCost))
        {
            //  Tocar Som
            if (actionSound != null)
            {
                AudioSource.PlayClipAtPoint(actionSound, Camera.main.transform.position, soundVolume);
            }

            currentDebrisSpot.BuildMine();
        }
        ClosePanel();
    }

    private void OnNoClicked()
    {
        ClosePanel();
    }
}
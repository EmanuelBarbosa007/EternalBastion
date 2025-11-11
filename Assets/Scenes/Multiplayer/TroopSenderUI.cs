using UnityEngine;
using UnityEngine.UI; // Precisa disto para os Botões
using TMPro; // Precisa disto para os Textos

public class TroopSenderUI : MonoBehaviour
{
    [Header("Custos de Spawn")]
    public int custoTropaNormal = 20;
    public int prefabIdTropaNormal = 0;

    public int custoTropaTanque = 35;
    public int prefabIdTropaTanque = 1;

    public int custoCavalo = 25;
    public int prefabIdCavalo = 8;

    // --- NOVO: Custos de Upgrade ---
    [Header("Custos de Upgrade")]
    public int custoUpgradeTropaNormal = 50;
    public int custoUpgradeTropaTanque = 75;
    public int custoUpgradeCavalo = 60;

    // --- NOVO: Referências de UI (Ligar no Inspector) ---
    [Header("UI Tropa Normal")]
    public Button botaoUpgradeNormal;
    public TextMeshProUGUI textoNivelNormal;

    [Header("UI Tropa Tanque")]
    public Button botaoUpgradeTanque;
    public TextMeshProUGUI textoNivelTanque;

    [Header("UI Cavalo")]
    public Button botaoUpgradeCavalo;
    public TextMeshProUGUI textoNivelCavalo;


    // --- NOVO: Update para atualizar UI ---
    void Update()
    {
        // Espera que o jogador local exista na rede
        if (PlayerNetwork.LocalInstance == null)
            return;

        // --- Atualiza UI da Tropa Normal ---
        int nivelNormal = PlayerNetwork.LocalInstance.NivelTropaNormal.Value;
        if (textoNivelNormal != null)
            textoNivelNormal.text = "Nível: " + nivelNormal;

        // Desativa o botão de upgrade se já estiver no nível 2
        if (botaoUpgradeNormal != null)
            botaoUpgradeNormal.interactable = (nivelNormal < 2);

        // --- Atualiza UI da Tropa Tanque ---
        int nivelTanque = PlayerNetwork.LocalInstance.NivelTropaTanque.Value;
        if (textoNivelTanque != null)
            textoNivelTanque.text = "Nível: " + nivelTanque;

        if (botaoUpgradeTanque != null)
            botaoUpgradeTanque.interactable = (nivelTanque < 2);

        // --- Atualiza UI do Cavalo ---
        int nivelCavalo = PlayerNetwork.LocalInstance.NivelCavalo.Value;
        if (textoNivelCavalo != null)
            textoNivelCavalo.text = "Nível: " + nivelCavalo;

        if (botaoUpgradeCavalo != null)
            botaoUpgradeCavalo.interactable = (nivelCavalo < 2);
    }


    // --- Funções de Comprar (iguais às tuas) ---

    public void OnClick_ComprarTropaNormal()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestSpawnTroopServerRpc(
                prefabIdTropaNormal,
                custoTropaNormal
            );
        }
    }

    public void OnClick_ComprarTropaTanque()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestSpawnTroopServerRpc(
                prefabIdTropaTanque,
                custoTropaTanque
            );
        }
    }

    public void OnClick_ComprarCavalo()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestSpawnTroopServerRpc(
                prefabIdCavalo,
                custoCavalo
            );
        }
    }

    // --- NOVO: Funções de Melhorar ---

    public void OnClick_MelhorarTropaNormal()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestUpgradeTroopServerRpc(
                prefabIdTropaNormal,
                custoUpgradeTropaNormal
            );
        }
    }

    public void OnClick_MelhorarTropaTanque()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestUpgradeTroopServerRpc(
                prefabIdTropaTanque,
                custoUpgradeTropaTanque
            );
        }
    }

    public void OnClick_MelhorarCavalo()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestUpgradeTroopServerRpc(
                prefabIdCavalo,
                custoUpgradeCavalo
            );
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class TowerSpotMP : NetworkBehaviour
{
    // NOVO: Sincroniza o estado de ocupado do Server para todos os Clientes
    public NetworkVariable<bool> isOccupied = new NetworkVariable<bool>(false);

    // NOVO: Sincroniza qual torre (pelo seu ID de rede) est� neste spot
    public NetworkVariable<ulong> currentTowerNetworkId = new NetworkVariable<ulong>(0);

    // NOVO: Defina isto no Inspector para cada spot
    // (ex: Spots do lado esquerdo s�o JogadorA, do lado direito s�o JogadorB)
    public PlayerID donoDoSpot;

    // Guarda a torre em cache para o UI n�o ter de a procurar
    private TowerMP cachedTower;

    /// <summary>
    /// Esta � a fun��o principal. � chamada no CLIENTE que clica.
    /// </summary>
    private void OnMouseDown()
    {
        // 1. Impede clique se o rato estiver sobre o UI
        // (Igual ao seu script original)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 2. NOVO: Verifica se o jogador local (quem est� a jogar neste PC)
        // � o dono deste spot.
        if (PlayerNetwork.LocalInstance == null)
        {
            Debug.LogError("N�o consigo encontrar o PlayerNetwork.LocalInstance!");
            return;
        }

        // Descobre se este jogador � o JogadorA ou JogadorB
        PlayerID localPlayerId = (PlayerNetwork.LocalInstance.OwnerClientId == 0)
                                 ? PlayerID.JogadorA
                                 : PlayerID.JogadorB;

        if (donoDoSpot != localPlayerId)
        {
            Debug.Log("Este spot n�o � seu!");
            return; // Sai da fun��o
        }

        // 3. NOVO: L�gica de abrir painel,
        // mas agora chama os pain�is da vers�o MP

        if (isOccupied.Value)
        {
            // --- SPOT OCUPADO: ABRE O PAINEL DE UPGRADE ---

            // Procura a torre que est� neste spot
            if (cachedTower == null && currentTowerNetworkId.Value != 0)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(currentTowerNetworkId.Value, out NetworkObject towerNetworkObject))
                {
                    cachedTower = towerNetworkObject.GetComponent<TowerMP>();
                }
            }

            // (Aviso: Voc� precisar� criar o TowerUpgradeUIMP.cs)
            if (cachedTower != null && TowerUpgradeUIMP.Instance != null)
            {
                // Fecha o painel de constru��o se estiver aberto
                if (TowerPlacementUIMP.Instance != null)
                    TowerPlacementUIMP.Instance.ClosePanel();

                // TowerUpgradeUIMP.Instance.OpenPanel(cachedTower, this);
                Debug.Log("Abrir painel de Upgrade para a torre: " + cachedTower.name);
            }
        }
        else
        {
            // --- SPOT VAZIO: ABRE O PAINEL DE CONSTRU��O ---

            // (Aviso: Voc� precisar� criar o TowerPlacementUIMP.cs)
            if (TowerPlacementUIMP.Instance != null)
            {
                // Fecha o painel de upgrade se estiver aberto
                if (TowerUpgradeUIMP.Instance != null)
                    TowerUpgradeUIMP.Instance.ClosePanel();

                // Passa 'this' (este pr�prio script) para o painel de UI
                // O UI precisa de saber o NetworkObjectId deste spot
                // para o enviar no ServerRpc de constru��o
                // TowerPlacementUIMP.Instance.OpenPanel(this);
                Debug.Log("Abrir painel de Constru��o para este spot.");
            }
        }
    }

    /// <summary>
    /// Esta fun��o � chamada pelo SERVIDOR (no PlayerNetwork.cs)
    /// depois de uma torre ser constru�da, para atualizar o estado.
    /// </summary>
    public void SetOcupado(TowerMP torre)
    {
        if (!IsServer) return;

        isOccupied.Value = true;
        currentTowerNetworkId.Value = torre.NetworkObjectId;
    }

    /// <summary>
    /// Esta fun��o � chamada pelo SERVIDOR (num ServerRpc)
    /// depois de uma torre ser vendida.
    /// </summary>
    public void SetVazio()
    {
        if (!IsServer) return;

        isOccupied.Value = false;
        currentTowerNetworkId.Value = 0;
        cachedTower = null;
    }
}
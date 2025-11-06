using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalInstance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

    }


    [ServerRpc]
    public void RequestSpawnTroopServerRpc(int tropaPrefabID, int custo)
    {
        // Chama o cérebro central, passando o ID de quem pediu
        GameServerLogic.Instance.TrySpawnTroop(OwnerClientId, tropaPrefabID, custo);
    }

    // Construção de Torre

    [ServerRpc]
    public void RequestBuildTowerServerRpc(int torrePrefabID, int custo, Vector3 posicao, ulong spotNetworkId)
    {
        // Chama o cérebro central
        GameServerLogic.Instance.TryBuildTower(OwnerClientId, torrePrefabID, custo, posicao, spotNetworkId);
    }

    //Upgrade de Torre

    [ServerRpc]
    public void RequestUpgradeTowerServerRpc(ulong towerNetworkId)
    {
        // Chama o cérebro central
        GameServerLogic.Instance.TryUpgradeTower(OwnerClientId, towerNetworkId);
    }

    // RPC de Venda de Torre 

    [ServerRpc]
    public void RequestSellTowerServerRpc(ulong towerNetworkId, ulong spotNetworkId)
    {
        // Chama o cérebro central
        GameServerLogic.Instance.TrySellTower(OwnerClientId, towerNetworkId, spotNetworkId);
    }
}
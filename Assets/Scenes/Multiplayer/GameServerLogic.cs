using UnityEngine;
using Unity.Netcode;

public class GameServerLogic : NetworkBehaviour
{
    public static GameServerLogic Instance;

    // --- REFERÊNCIAS PARA AMBOS OS LADOS (Ligar no Inspector) ---
    [Header("Jogador A (Host)")]
    public Transform spawnPointA;
    public WaypointPathMP caminhoA;
    public BaseHealthMP baseA;

    [Header("Jogador B (IA)")]
    public Transform spawnPointB;
    public WaypointPathMP caminhoB;
    public BaseHealthMP baseB;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    public bool TrySpawnTroop(ulong clientId, int tropaPrefabID, int custo)
    {
        if (!IsServer) return false;
        if (CurrencySystemMP.Instance.SpendMoney(clientId, custo))
        {
            Transform spawnPoint = (clientId == 0) ? spawnPointA : spawnPointB;
            WaypointPathMP caminho = (clientId == 0) ? caminhoA : caminhoB;
            BaseHealthMP baseInimiga = (clientId == 0) ? baseB : baseA;

            GameObject prefabTropa = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[tropaPrefabID].Prefab;
            GameObject go = Instantiate(prefabTropa, spawnPoint.position, spawnPoint.rotation);

            EnemyMP tropa = go.GetComponent<EnemyMP>();
            tropa.Setup(clientId, caminho.points, baseInimiga);

            go.GetComponent<NetworkObject>().Spawn();
            return true;
        }
        return false;
    }

    public bool TryBuildTower(ulong clientId, int torrePrefabID, int custo, Vector3 posicao, ulong spotNetworkId)
    {
        if (!IsServer) return false;

        if (CurrencySystemMP.Instance.SpendMoney(clientId, custo))
        {
            GameObject prefabTorre = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[torrePrefabID].Prefab;
            GameObject go = Instantiate(prefabTorre, posicao, Quaternion.identity);

            TowerMP torre = go.GetComponent<TowerMP>();
            torre.donoDaTorreClientId = clientId;
            torre.totalInvested = custo;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
            {
                TowerSpotMP spot = spotNetworkObject.GetComponent<TowerSpotMP>();
                spot.SetOcupado(torre);
                torre.myTowerSpot = spot;
            }

            go.GetComponent<NetworkObject>().Spawn();
            return true;
        }
        return false;
    }

    public bool TryUpgradeTower(ulong clientId, ulong towerNetworkId)
    {
        if (!IsServer) return false;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(towerNetworkId, out NetworkObject towerNetworkObject))
        {
            Debug.LogError($"GameServerLogic: Não foi possível encontrar a torre {towerNetworkId} para upgrade.");
            return false;
        }
        TowerMP torre = towerNetworkObject.GetComponent<TowerMP>();
        if (torre == null)
        {
            Debug.LogError($"GameServerLogic: O objeto {towerNetworkId} não tem um componente TowerMP.");
            return false;
        }

        torre.TryUpgrade(clientId); 
        return true; 
    }

    public bool TrySellTower(ulong clientId, ulong towerNetworkId, ulong spotNetworkId)
    {
        if (!IsServer) return false;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(towerNetworkId, out NetworkObject towerNetworkObject))
            return false;

        TowerMP torre = towerNetworkObject.GetComponent<TowerMP>();
        if (torre == null) return false;

        if (torre.donoDaTorreClientId != clientId)
            return false;

        int sellAmount = torre.totalInvested / 2;
        CurrencySystemMP.Instance.AddMoney(clientId, sellAmount);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
        {
            spotNetworkObject.GetComponent<TowerSpotMP>().SetVazio();
        }

        towerNetworkObject.Despawn();
        return true;
    }
}
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalInstance { get; private set; }

    private Transform meuPontoSpawn;
    private WaypointPathMP meuCaminho;
    private BaseHealthMP baseInimiga;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        if (IsServer)
        {
            if (OwnerClientId == 0) // Host (Jogador A)
            {
                meuPontoSpawn = GameObject.Find("SpawnPoint_A").transform;
                meuCaminho = GameObject.Find("Caminho_A_para_B").GetComponent<WaypointPathMP>();
                baseInimiga = GameManagerMP.Instance.baseJogadorB;
            }
            else // Cliente (Jogador B)
            {
                meuPontoSpawn = GameObject.Find("SpawnPoint_B").transform;
                meuCaminho = GameObject.Find("Caminho_B_para_A").GetComponent<WaypointPathMP>();
                baseInimiga = GameManagerMP.Instance.baseJogadorA;
            }
        }
    }

    // --- RPC de Enviar Tropas ---

    [ServerRpc]
    public void RequestSpawnTroopServerRpc(int tropaPrefabID, int custo)
    {
        if (!IsServer) return;

        if (CurrencySystemMP.Instance.SpendMoney(OwnerClientId, custo))
        {
            // (Precisa ter os seus prefabs de tropas na lista de NetworkPrefabs)
            GameObject prefabTropa = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[tropaPrefabID].Prefab;
            GameObject go = Instantiate(prefabTropa, meuPontoSpawn.position, meuPontoSpawn.rotation);

            EnemyMP tropa = go.GetComponent<EnemyMP>();
            tropa.Setup(OwnerClientId, meuCaminho.points, baseInimiga);

            go.GetComponent<NetworkObject>().Spawn();
        }
    }

    // --- RPCs de Construção de Torre ---

    [ServerRpc]
    public void RequestBuildTowerServerRpc(int torrePrefabID, int custo, Vector3 posicao, ulong spotNetworkId)
    {
        if (!IsServer) return;

        // 1. Tenta gastar o dinheiro
        if (CurrencySystemMP.Instance.SpendMoney(OwnerClientId, custo))
        {
            // 2. Encontra o Prefab da Torre
            GameObject prefabTorre = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[torrePrefabID].Prefab;

            // 3. Instancia a Torre no Servidor
            GameObject go = Instantiate(prefabTorre, posicao, Quaternion.identity);

            // 4. Configura a Torre
            TowerMP torre = go.GetComponent<TowerMP>();
            torre.donoDaTorreClientId = OwnerClientId;
            torre.totalInvested = custo; // Define o investimento inicial

            // 5. Encontra o Spot e atualiza-o
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
            {
                TowerSpotMP spot = spotNetworkObject.GetComponent<TowerSpotMP>();
                spot.SetOcupado(torre);
                torre.myTowerSpot = spot; // Diz à torre qual é o seu spot
            }

            // 6. Spawna a Torre na Rede
            go.GetComponent<NetworkObject>().Spawn();
        }
    }

    // --- NOVO: RPC de Upgrade de Torre ---

    [ServerRpc]
    public void RequestUpgradeTowerServerRpc(ulong towerNetworkId)
    {
        if (!IsServer) return;

        // 1. Encontra a Torre
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(towerNetworkId, out NetworkObject towerNetworkObject))
        {
            Debug.LogError($"Servidor: Não foi possível encontrar a torre com ID {towerNetworkId}");
            return;
        }

        TowerMP torre = towerNetworkObject.GetComponent<TowerMP>();
        if (torre == null) return;

        // 2. Manda a torre tentar fazer o upgrade.
        // O 'OwnerClientId' é o ID do jogador que enviou este RPC.
        torre.TryUpgrade(OwnerClientId);
    }

    // --- NOVO: RPC de Venda de Torre ---

    [ServerRpc]
    public void RequestSellTowerServerRpc(ulong towerNetworkId, ulong spotNetworkId)
    {
        if (!IsServer) return;

        // 1. Encontra a Torre
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(towerNetworkId, out NetworkObject towerNetworkObject))
        {
            Debug.LogError($"Servidor: Não foi possível encontrar a torre {towerNetworkId} para vender.");
            return;
        }

        TowerMP torre = towerNetworkObject.GetComponent<TowerMP>();
        if (torre == null) return;

        // 2. Verifica se o jogador que pediu é o dono
        if (torre.donoDaTorreClientId != OwnerClientId)
        {
            Debug.LogWarning("Batota? Jogador tentou vender torre que não é sua.");
            return;
        }

        // 3. Devolve o dinheiro (baseado em Tower.cs)
        int sellAmount = torre.totalInvested / 2;
        CurrencySystemMP.Instance.AddMoney(OwnerClientId, sellAmount);

        // 4. Liberta o Spot
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
        {
            TowerSpotMP spot = spotNetworkObject.GetComponent<TowerSpotMP>();
            spot.SetVazio(); // Atualiza a NetworkVariable do spot
        }

        // 5. Destrói a torre na rede
        towerNetworkObject.Despawn();
    }
}
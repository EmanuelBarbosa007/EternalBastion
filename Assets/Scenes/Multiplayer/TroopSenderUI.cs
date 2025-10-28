using UnityEngine;

public class TroopSenderUI : MonoBehaviour
{
    // Custo e ID do Prefab (o ID é a ordem
    // na lista de Prefabs do NetworkManager)
    public int custoTropaNormal = 50;
    public int prefabIdTropaNormal = 0; // TEM DE CONFIGURAR ISTO

    public int custoTropaTanque = 100;
    public int prefabIdTropaTanque = 1; // TEM DE CONFIGURAR ISTO

    // Liga este botão no Inspector
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

    // Liga este botão no Inspector
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
}
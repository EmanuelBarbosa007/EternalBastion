using UnityEngine;

public class TroopSenderUI : MonoBehaviour
{
    public int custoTropaNormal = 20;
    public int prefabIdTropaNormal = 0;

    public int custoTropaTanque = 35;
    public int prefabIdTropaTanque = 1;

    public int custoCavalo = 25;
    public int prefabIdCavalo = 8;

    // Ligar botões no Inspector
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
}
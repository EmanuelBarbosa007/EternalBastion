using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpot : MonoBehaviour
{
    [HideInInspector] public bool isOccupied = false;

    private void OnMouseDown()
    {
        // impede clique se o rato estiver sobre o UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (isOccupied)
        {
            Debug.Log("Spot já ocupado!");
            return;
        }

        if (TowerPlacementUI.Instance != null)
        {
            TowerPlacementUI.Instance.OpenPanel(this);
        }
    }
}

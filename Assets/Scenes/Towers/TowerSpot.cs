using UnityEngine;

public class TowerSpot : MonoBehaviour
{
    public GameObject towerPrefab;
    private bool isOccupied = false;

    private void OnMouseDown()
    {
        if (!isOccupied && CurrencySystem.SpendMoney(50)) 
        {
            GameObject newTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);
            newTower.transform.localScale = newTower.transform.localScale * 1.5f;
            isOccupied = true;

        }
        else
        {
            Debug.Log("Spot já ocupado ou moedas insuficientes!");
        }
    }
}

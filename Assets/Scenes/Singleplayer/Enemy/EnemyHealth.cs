using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public int reward = 20; 

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // dá moedas ao jogador
        CurrencySystem.AddMoney(reward);

        // Avisa o spawner que este inimigo morreu
        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;


        // Tenta encontrar o script do boss neste objeto
        TrojanHorseBoss boss = GetComponent<TrojanHorseBoss>(); 

        // 2. Se for o boss chama a função de spawn
        if (boss != null)
        {
            boss.SpawnTroops(); 
        }


        // 3. Destrói o inimigo
        Destroy(gameObject);
    }
}

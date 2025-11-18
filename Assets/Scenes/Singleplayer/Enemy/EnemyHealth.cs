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
        // 1. Lógica comum (Moedas e Contador)
        CurrencySystem.AddMoney(reward);

        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        // 2. Verifica se é o Cavalo de Troia
        TrojanHorseBoss boss = GetComponent<TrojanHorseBoss>();

        if (boss != null)
        {
            // Se for o boss, mandamos ELE tratar da morte (spawnar tropas)
            boss.StartDeathSequence();

            // IMPORTANTE: O return faz com que a função pare aqui!
            // Assim não executamos o Destroy abaixo, deixando o cavalo "vivo" 
            // o tempo suficiente para spawnar os soldados.
            return;
        }

        // 3. Se NÃO for boss, destrói normalmente
        Destroy(gameObject);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;
    public int reward = 20;

    [Header("UI")]
    public Slider healthBar; // O slot para arrastares a barra no Unity

    void Start()
    {
        currentHealth = maxHealth;

        // Configura a barra de vida inicial
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Atualiza a barra visualmente
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        CurrencySystem.AddMoney(reward);

        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        TrojanHorseBoss boss = GetComponent<TrojanHorseBoss>();

        if (boss != null)
        {
            boss.StartDeathSequence();
            return;
        }

        Destroy(gameObject);
    }
}
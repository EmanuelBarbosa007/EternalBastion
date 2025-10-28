using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class EnemyHealthMP : NetworkBehaviour
{
    public int maxHealth = 10;
    public int moneyOnDeath = 5;

    public Slider healthBar; // (Opcional) UI da barra de vida

    // Sincroniza a vida do Server para os Clientes
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Só o server define a vida inicial
            currentHealth.Value = maxHealth;
        }

        // Todos os clientes (incluindo o server)
        // atualizam o UI quando a vida muda
        currentHealth.OnValueChanged += OnHealthChanged;
        // Atualiza o UI pela primeira vez
        OnHealthChanged(0, currentHealth.Value);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        // Esta função corre em TODOS os clientes
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = newValue;
        }
    }

    // Chamado pela BulletMP (que só corre no Server)
    public void TakeDamage(int amount, ulong killerClientId)
    {
        if (!IsServer) return; // Segurança extra
        if (currentHealth.Value <= 0) return; // Já está morto

        currentHealth.Value -= amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value <= 0)
        {
            // Dá o dinheiro ao jogador que deu o tiro final
            CurrencySystemMP.Instance.AddMoney(killerClientId, moneyOnDeath);

            // Não precisamos mais disto:
            // EnemySpawner.EnemiesAlive--; 

            // Destrói o inimigo em todos os clientes
            NetworkObject.Despawn();
        }
    }
}
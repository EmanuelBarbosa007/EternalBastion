using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class EnemyHealthMP : NetworkBehaviour
{
    [Header("Stats Base (Nível 1)")]
    public int baseHealth = 10; // RENOMEADO
    public int moneyOnDeath = 5;

    [Header("Multiplicadores Nível 2")]
    public float healthMultiplierLvl2 = 1.5f; // Bónus de 50%

    public Slider healthBar;

    // --- MODIFICADO: Sincroniza a vida MÁXIMA e ATUAL ---
    private NetworkVariable<int> currentMaxHealth = new NetworkVariable<int>();
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();


    // --- NOVO: Função chamada pelo EnemyMP.Setup ---
    public void SetNivelServer(int nivel)
    {
        if (!IsServer) return; // Só o servidor decide isto

        int calculatedMaxHealth;
        if (nivel >= 2)
        {
            // Calcula a vida de Nível 2
            calculatedMaxHealth = (int)(baseHealth * healthMultiplierLvl2);
        }
        else
        {
            // Vida de Nível 1
            calculatedMaxHealth = baseHealth;
        }

        // Define os valores sincronizados
        // Isto é feito ANTES do OnNetworkSpawn nos clientes
        currentMaxHealth.Value = calculatedMaxHealth;
        currentHealth.Value = calculatedMaxHealth; // Nasce com vida cheia
    }


    public override void OnNetworkSpawn()
    {
        // --- MODIFICADO ---
        // A lógica de definir a vida inicial foi movida para SetNivelServer,
        // que é chamado pelo GameServerLogic ANTES dos clientes receberem o spawn.

        // Todos os clientes (incluindo o server)
        // atualizam o UI quando a vida muda
        currentHealth.OnValueChanged += OnHealthChanged;

        // --- NOVO: Atualiza o UI se o MaxHealth mudar ---
        currentMaxHealth.OnValueChanged += (prev, next) => OnHealthChanged(0, currentHealth.Value);

        // Atualiza o UI pela primeira vez com os valores corretos (Nv 1 ou Nv 2)
        OnHealthChanged(0, currentHealth.Value);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        // Esta função corre em TODOS os clientes
        if (healthBar != null)
        {
            // --- MODIFICADO: Usa a MaxHealth sincronizada ---
            healthBar.maxValue = currentMaxHealth.Value;
            healthBar.value = newValue;
        }
    }

    // Chamado pela BulletMP (que só corre no Server)
    public void TakeDamage(int amount, ulong killerClientId)
    {
        if (!IsServer) return; // Segurança extra
        if (currentHealth.Value <= 0) return; // Já está morto

        currentHealth.Value -= amount;

        // --- MODIFICADO: Usa a MaxHealth sincronizada ---
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, currentMaxHealth.Value);

        if (currentHealth.Value <= 0)
        {
            // Dá o dinheiro ao jogador que deu o tiro final
            CurrencySystemMP.Instance.AddMoney(killerClientId, moneyOnDeath);

            // Destrói o inimigo em todos os clientes
            NetworkObject.Despawn();
        }
    }
}
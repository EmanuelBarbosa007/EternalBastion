using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic; // Para a lista de inimigos j� atingidos

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(Rigidbody))] // Rigidbody para OnTriggerEnter
public class PiercingBulletMP : NetworkBehaviour
{
    // NetworkVariable para sincronizar a dire��o pode ser �til se a dire��o mudar,
    // mas para um tiro reto, definir uma vez no spawn pode ser suficiente.
    // Vamos tentar sem NetworkVariable primeiro.
    private Vector3 moveDirection;

    public float speed = 20f;
    public int damage = 40; // Dano base
    public float lifetime = 5f; // Tempo de vida da bala

    private float lifeTimer = 0f;

    [HideInInspector]
    public ulong ownerClientId;

    // Lista para guardar os inimigos que esta bala j� atingiu
    private List<EnemyHealthMP> enemiesHit = new List<EnemyHealthMP>();

    // M�todo para a torre definir a dire��o inicial (chamado no Server ap�s Spawn)
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        // Opcional: Rodar a bala para olhar na dire��o do movimento
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }


    void Start()
    {
        // Garante que o Rigidbody � Kinematic e usa triggers
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Movimento ser� controlado por script, n�o pela f�sica
            // Garante que o Collider associado (ex: SphereCollider, CapsuleCollider) � um Trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
            else
            {
                Debug.LogError("PiercingBulletMP precisa de um Collider para detectar colis�es (OnTriggerEnter)!", this);
            }
        }
        else
        {
            Debug.LogError("PiercingBulletMP precisa de um Rigidbody!", this);
        }
    }

    void Update()
    {
        // --- S� O SERVER MOVIMENTA E CONTROLA O TEMPO DE VIDA ---
        if (!IsServer) return;

        // Movimento em linha reta
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Controlo do tempo de vida
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            NetworkObject.Despawn(); // Destr�i na rede
        }
    }

    // --- DETE��O DE COLIS�O (S� NO SERVER) ---
    private void OnTriggerEnter(Collider other)
    {
        // S� o servidor processa colis�es e dano
        if (!IsServer) return;

        // Tenta obter o componente de vida do inimigo
        EnemyHealthMP enemyHealth = other.GetComponent<EnemyHealthMP>();

        // Se for um inimigo E esta bala ainda n�o o atingiu
        if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
        {
            // Aplica dano
            enemyHealth.TakeDamage(damage, ownerClientId);

            // Adiciona o inimigo � lista de atingidos por esta bala
            enemiesHit.Add(enemyHealth);

            // NOTA: A bala continua o seu percurso! N�o � destru�da aqui.
        }
        // Se colidir com outra coisa (ex: cen�rio), pode querer destruir a bala
        // else if (other.gameObject.CompareTag("Obstaculo")) // Exemplo
        // {
        //     NetworkObject.Despawn();
        // }
    }
}
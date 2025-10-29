using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic; // Para a lista de inimigos já atingidos

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(Rigidbody))] // Rigidbody para OnTriggerEnter
public class PiercingBulletMP : NetworkBehaviour
{
    // NetworkVariable para sincronizar a direção pode ser útil se a direção mudar,
    // mas para um tiro reto, definir uma vez no spawn pode ser suficiente.
    // Vamos tentar sem NetworkVariable primeiro.
    private Vector3 moveDirection;

    public float speed = 20f;
    public int damage = 40; // Dano base
    public float lifetime = 5f; // Tempo de vida da bala

    private float lifeTimer = 0f;

    [HideInInspector]
    public ulong ownerClientId;

    // Lista para guardar os inimigos que esta bala já atingiu
    private List<EnemyHealthMP> enemiesHit = new List<EnemyHealthMP>();

    // Método para a torre definir a direção inicial (chamado no Server após Spawn)
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        // Opcional: Rodar a bala para olhar na direção do movimento
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }


    void Start()
    {
        // Garante que o Rigidbody é Kinematic e usa triggers
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Movimento será controlado por script, não pela física
            // Garante que o Collider associado (ex: SphereCollider, CapsuleCollider) é um Trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
            else
            {
                Debug.LogError("PiercingBulletMP precisa de um Collider para detectar colisões (OnTriggerEnter)!", this);
            }
        }
        else
        {
            Debug.LogError("PiercingBulletMP precisa de um Rigidbody!", this);
        }
    }

    void Update()
    {
        // --- SÓ O SERVER MOVIMENTA E CONTROLA O TEMPO DE VIDA ---
        if (!IsServer) return;

        // Movimento em linha reta
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Controlo do tempo de vida
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            NetworkObject.Despawn(); // Destrói na rede
        }
    }

    // --- DETEÇÃO DE COLISÃO (SÓ NO SERVER) ---
    private void OnTriggerEnter(Collider other)
    {
        // Só o servidor processa colisões e dano
        if (!IsServer) return;

        // Tenta obter o componente de vida do inimigo
        EnemyHealthMP enemyHealth = other.GetComponent<EnemyHealthMP>();

        // Se for um inimigo E esta bala ainda não o atingiu
        if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
        {
            // Aplica dano
            enemyHealth.TakeDamage(damage, ownerClientId);

            // Adiciona o inimigo à lista de atingidos por esta bala
            enemiesHit.Add(enemyHealth);

            // NOTA: A bala continua o seu percurso! Não é destruída aqui.
        }
        // Se colidir com outra coisa (ex: cenário), pode querer destruir a bala
        // else if (other.gameObject.CompareTag("Obstaculo")) // Exemplo
        // {
        //     NetworkObject.Despawn();
        // }
    }
}
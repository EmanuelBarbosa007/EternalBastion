using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic; // Para a lista de inimigos atingidos

// Requer NetworkObject para existir na rede e NetworkTransform para sincronizar posi��o
[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public class FireballMP : NetworkBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float explosionRadius = 3f;
    public int damage = 50; // Dano base, ser� modificado pela torre N�vel 3

    public GameObject impactEffectPrefab; // Prefab do efeito visual de explos�o

    [HideInInspector]
    public ulong ownerClientId; // ID do jogador que disparou

    // Lista para garantir que cada inimigo s� leva dano uma vez pela explos�o
    private List<EnemyHealthMP> hitEnemies = new List<EnemyHealthMP>();

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        // --- S� O SERVER MOVIMENTA E VERIFICA COLIS�O ---
        if (!IsServer)
        {
            // Os clientes recebem a posi��o pelo NetworkTransform
            return;
        }

        // Se o alvo morreu ou desapareceu enquanto a bola voava
        if (target == null)
        {
            NetworkObject.Despawn(); // Destr�i na rede
            return;
        }

        // L�gica de movimento (igual � original)
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Verifica se alcan�ou ou ultrapassou o alvo neste frame
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget(); // Chama a fun��o de impacto (s� no server)
            return; // Sai do Update ap�s atingir o alvo
        }

        // Move a bola de fogo
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        // Opcional: fazer a bola olhar para o alvo (NetworkTransform pode sincronizar rota��o)
        // transform.LookAt(target);
    }

    // --- Chamada APENAS NO SERVER quando atinge o alvo ---
    void HitTarget()
    {
        // 1. Aplica dano em �rea
        Explode();

        // 2. Manda os clientes spawnarem o efeito visual
        SpawnImpactEffectClientRpc(transform.position);

        // 3. Destr�i a bola de fogo na rede
        NetworkObject.Despawn();
    }

    // --- Aplica dano em �rea (S� NO SERVER) ---
    void Explode()
    {
        hitEnemies.Clear(); // Limpa a lista para esta explos�o

        // Encontra todos os colliders dentro do raio de explos�o
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            // Tenta encontrar o componente de vida do inimigo (vers�o MP)
            EnemyHealthMP enemyHealth = col.GetComponent<EnemyHealthMP>();

            // Se encontrou um inimigo E ainda n�o o atingimos nesta explos�o
            if (enemyHealth != null && !hitEnemies.Contains(enemyHealth))
            {
                // Aplica dano, passando o ID do dono da torre/bala
                enemyHealth.TakeDamage(damage, ownerClientId);
                hitEnemies.Add(enemyHealth); // Adiciona � lista para n�o atingir de novo
            }
        }
    }

    // --- ClientRpc para mostrar o efeito visual ---
    [ClientRpc]
    private void SpawnImpactEffectClientRpc(Vector3 position)
    {
        // Este c�digo corre em TODOS os clientes (e no host/server)
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // Destr�i o efeito visual ap�s 2 segundos (localmente)
        }
    }

    // (Opcional) Desenhar o raio no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
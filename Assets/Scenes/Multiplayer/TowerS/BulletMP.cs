using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components; // Precisa disto para o NetworkTransform

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public class BulletMP : NetworkBehaviour
{
    private Transform target;
    public float speed = 10f;
    public int damage = 1;

    // NOVO: ID do jogador que disparou esta bala
    // (A TowerMP vai definir isto)
    [HideInInspector]
    public ulong ownerClientId;

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        // --- SÓ O SERVER MOVIMENTA A BALA ---
        if (!IsServer)
        {
            // Os clientes recebem a posição pelo NetworkTransform
            return;
        }

        // O resto da lógica é igual à sua
        if (target == null)
        {
            // O alvo morreu ou desapareceu
            NetworkObject.Despawn(); // Destrói na rede
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        // Esta função só corre no Server

        // Procura o script de vida do inimigo (a versão MP)
        EnemyHealthMP e = target.GetComponent<EnemyHealthMP>();
        if (e != null)
        {
            // Passa o dano E quem o causou (para o dinheiro)
            e.TakeDamage(damage, ownerClientId);
        }

        NetworkObject.Despawn(); // Destrói a bala na rede
    }
}
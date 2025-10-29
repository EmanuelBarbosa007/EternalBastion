using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Netcode.Components;

// NetworkTransform lida com a sincroniza��o de posi��o/rota��o
[RequireComponent(typeof(NetworkTransform))]
public class EnemyMP : NetworkBehaviour
{
    public float speed = 3f;
    public float rotateSpeed = 10f;

    // Estas vari�veis s�o configuradas PELO SERVER
    private ulong donoDaTropa;
    private Transform[] meuCaminho;
    private BaseHealthMP baseAlvo;

    private int waypointIndex = 0;
    private float startY;
    private Transform target;

    // Chamado pelo Server no PlayerNetwork.cs
    public void Setup(ulong dono, Transform[] caminho, BaseHealthMP alvo)
    {
        donoDaTropa = dono;
        meuCaminho = caminho;
        baseAlvo = alvo;

        if (meuCaminho == null || meuCaminho.Length == 0)
        {
            Debug.LogError("Tropa spawnada sem caminho!");
            return;
        }

        target = meuCaminho[0];
        startY = transform.position.y;
    }

    void Update()
    {
        // --- S� O SERVER MOVIMENTA OS INIMIGOS ---
        if (!IsServer)
        {
            // Os clientes recebem a posi��o via NetworkTransform
            return;
        }

        // --- L�gica de movimento (igual � sua, mas s� no server) ---

        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
        }

        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        if (HasReachedTarget())
        {
            GetNextWaypoint();
        }
    }

    // Fun��o auxiliar (s� corre no server)
    bool HasReachedTarget()
    {
        Vector3 posXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetXZ = new Vector3(target.position.x, 0, target.position.z);
        return Vector3.Distance(posXZ, targetXZ) < 0.2f;
    }

    // Fun��o auxiliar (s� corre no server)
    void GetNextWaypoint()
    {
        waypointIndex++;

        if (waypointIndex >= meuCaminho.Length)
        {
            // Chegou � base
            if (baseAlvo != null)
                baseAlvo.TakeDamage(1);

            // Despawna o inimigo da rede
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
            else
            {
                Destroy(gameObject); // Fallback
            }
            return;
        }

        target = meuCaminho[waypointIndex];
    }
}
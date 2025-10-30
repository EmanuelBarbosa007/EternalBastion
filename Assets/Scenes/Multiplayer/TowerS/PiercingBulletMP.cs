using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(Rigidbody))]
public class PiercingBulletMP : NetworkBehaviour
{
    private Vector3 moveDirection;

    public float speed = 20f;
    public int damage = 3; // Dano base
    public float lifetime = 10f; // Tempo de vida da bala

    private float lifeTimer = 0f;

    [HideInInspector]
    public ulong ownerClientId;

    private List<EnemyHealthMP> enemiesHit = new List<EnemyHealthMP>();

    public void SetDirection(Vector3 direction)
    {

        // Ignora a componente Y.
        direction.y = 0f;

        moveDirection = direction.normalized;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }


    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
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
        // Apenas o servidor move a bala
        if (!IsServer) return;

        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Controlo de tempo de vida
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            NetworkObject.Despawn(); // Destrói na rede
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // Apenas o servidor deteta colisões
        if (!IsServer) return;

        EnemyHealthMP enemyHealth = other.GetComponent<EnemyHealthMP>();

        if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
        {
            enemyHealth.TakeDamage(damage, ownerClientId);
            enemiesHit.Add(enemyHealth);
        }
    }
}
using UnityEngine;
using Unity.Netcode;

public class PiercingTowerMP : TowerMP
{
    protected override void Start()
    {
        base.Start();
        if (IsOwner) // Ou sem a verifica��o, se o UI for buscar o nome diretamente
        {
            towerName = "Piercing Tower";
        }
    }

    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            PiercingBulletMP pb = bulletPrefab.GetComponent<PiercingBulletMP>();
            if (pb != null)
            {
                baseBulletDamage = pb.damage;
                baseBulletSpeed = pb.speed;
            }
            else
            {
                Debug.LogError($"O prefab '{bulletPrefab.name}' atribu�do a {gameObject.name} n�o tem o script PiercingBulletMP!", this);
            }
        }
        else
        {
            Debug.LogError($"A torre {gameObject.name} n�o tem prefab de bala atribu�do!", this);
        }
    }

    protected override void Shoot()
    {
        // S� corre no Server
        if (bulletPrefab == null || firePoint == null || target == null) return;

        // 1. Instancia o prefab da PiercingBulletMP
        // Usa a rota��o da torre (firePoint) para a dire��o inicial
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Obt�m e Spawna o NetworkObject
        NetworkObject bulletNO = bulletGO.GetComponent<NetworkObject>();
        if (bulletNO == null)
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' n�o tem NetworkObject!", this);
            Destroy(bulletGO);
            return;
        }
        bulletNO.Spawn();

        // 3. Obt�m o script PiercingBulletMP
        PiercingBulletMP bullet = bulletGO.GetComponent<PiercingBulletMP>();
        if (bullet != null)
        {
            // 4. Define o dono e stats
            bullet.ownerClientId = this.donoDaTorreClientId;

            int currentDamage = baseBulletDamage;
            float currentSpeed = baseBulletSpeed;

            if (level.Value == 3)
            {
                currentDamage = (int)(baseBulletDamage * 1.5f);
                currentSpeed = baseBulletSpeed * 1.5f;
            }

            // Define stats na inst�ncia da bala
            bullet.damage = currentDamage;
            bullet.speed = currentSpeed;

            // 5. Define a dire��o inicial (IMPORTANTE para bala perfurante)
            // A bala vai seguir em frente a partir do firePoint na dire��o do alvo inicial
            if (target != null)
            {
                Vector3 direction = (target.position - firePoint.position).normalized;
                bullet.SetDirection(direction); // Usa um m�todo para definir a dire��o
            }
            else
            {
                // Fallback: Dispara na dire��o para onde a torre est� a apontar
                bullet.SetDirection(firePoint.forward);
            }
        }
        else
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' n�o tem o script PiercingBulletMP!", this);
            bulletNO.Despawn();
        }
    }
}
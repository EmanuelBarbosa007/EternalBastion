using UnityEngine;

public class PiercingTower : Tower
{
    // Override Start para definir o nome e guardar stats
    protected override void Start()
    {
        base.Start(); // Chama o Start() da Tower (que chama StoreBaseBulletStats)
        towerName = "Piercing Tower";
    }

    // Override para guardar os stats da PiercingBullet
    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            PiercingBullet pb = bulletPrefab.GetComponent<PiercingBullet>();
            if (pb != null)
            {
                baseBulletDamage = pb.damage;
                baseBulletSpeed = pb.speed;
            }
        }
    }

    // Override Shoot para aplicar stats � PiercingBullet
    protected override void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        PiercingBullet pb = go.GetComponent<PiercingBullet>();
        if (pb != null)
        {
            // Aplica melhorias de N�vel 3
            if (level == 3)
            {
                pb.damage = (int)(baseBulletDamage * 1.5f); // +50% Dano
                pb.speed = baseBulletSpeed * 1.5f;       // +50% Velocidade
            }

            pb.Seek(target);
        }
    }
}
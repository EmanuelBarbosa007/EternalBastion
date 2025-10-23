using UnityEngine;

public class FireTower : Tower
{
    // Override Start para definir o nome e guardar stats
    protected override void Start()
    {
        base.Start(); // Chama o Start() da Tower (que chama StoreBaseBulletStats)
        towerName = "Fire Tower";
    }

    // Override para guardar os stats da Fireball
    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            Fireball fb = bulletPrefab.GetComponent<Fireball>();
            if (fb != null)
            {
                baseBulletDamage = fb.damage;
                baseBulletSpeed = fb.speed;
            }
        }
    }

    // Override Shoot para aplicar stats à Fireball
    protected override void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Fireball fb = go.GetComponent<Fireball>();
        if (fb != null)
        {
            // Aplica melhorias de Nível 3
            if (level == 3)
            {
                fb.damage = (int)(baseBulletDamage * 1.5f); // +50% Dano
                fb.speed = baseBulletSpeed * 1.5f;       // +50% Velocidade
            }

            fb.Seek(target);
        }
    }
}
using UnityEngine;

public class FireTower : Tower
{
    protected override void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Fireball fb = go.GetComponent<Fireball>();
        if (fb != null)
            fb.Seek(target);
    }
}

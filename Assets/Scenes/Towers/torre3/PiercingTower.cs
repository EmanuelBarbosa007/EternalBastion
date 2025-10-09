using UnityEngine;

public class PiercingTower : Tower
{
    protected override void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        PiercingBullet pb = go.GetComponent<PiercingBullet>();
        if (pb != null)
        {
            pb.Seek(target); 
        }
    }
}

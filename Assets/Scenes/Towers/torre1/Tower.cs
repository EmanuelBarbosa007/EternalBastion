using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Stats")]
    public float range = 5f;
    public float fireRate = 1f;
    public float rotationSpeed = 10f;

    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Parts")]
    public Transform partToRotate;

    // proteções para que subclasses possam aceder
    protected Transform target;
    protected float fireCountdown = 0f;

    protected virtual void Update()
    {
        UpdateTarget();

        if (target != null)
        {
            RotateToTarget();
        }

        if (target == null) return;

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    // procura o inimigo mais perto dentro do range
    protected virtual void UpdateTarget()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        Enemy nearest = null;

        foreach (Enemy e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < shortestDistance && d <= range)
            {
                shortestDistance = d;
                nearest = e;
            }
        }

        if (nearest != null)
            target = nearest.transform;
        else
            target = null;
    }

    protected virtual void RotateToTarget()
    {
        if (partToRotate == null || target == null) return;

        Vector3 dir = target.position - partToRotate.position;
        dir.y = 0f; // impede rotação vertical

        Quaternion lookRotation = Quaternion.LookRotation(dir);
        lookRotation *= Quaternion.Euler(0f, 180f, 0f); 
        partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }


    protected virtual void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Seek(target);
    }

    // (opcional) desenhar o alcance no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

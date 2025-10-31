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


    [Header("Upgrade Stats")]
    public string towerName = "Archer Tower"; // Nome para o UI
    public int level = 1;

    [Tooltip("Custo para comprar esta torre (Nível 1)")]
    public int costLevel1 = 100;
    public int upgradeCostLevel2 = 75;
    public int upgradeCostLevel3 = 150;

    [HideInInspector] public int totalInvested; // Total gasto (compra + melhorias)
    [HideInInspector] public TowerSpot myTowerSpot; // Referência ao spot onde está

    // Stats Base (para calcular melhorias)
    protected float baseRange;
    protected int baseBulletDamage;
    protected float baseBulletSpeed;



    protected Transform target;
    protected float fireCountdown = 0f;


    protected virtual void Start()
    {
        // Guarda os stats base 
        baseRange = range;
        StoreBaseBulletStats();

        if (totalInvested == 0)
        {
            totalInvested = costLevel1;
        }
    }


    protected virtual void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            Bullet b = bulletPrefab.GetComponent<Bullet>();
            if (b != null)
            {
                baseBulletDamage = b.damage;
                baseBulletSpeed = b.speed;
            }
        }
    }


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
        dir.y = 0f;
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
        {
            // Aplica melhorias de Nível 3
            if (level == 3)
            {
                bullet.damage = (int)(baseBulletDamage * 1.5f); // +50% Dano
                bullet.speed = baseBulletSpeed * 1.5f;       // +50% Velocidade
            }
            // (Se for Nível 1 ou 2, usa os stats padrão do prefab)

            bullet.Seek(target);
        }
    }


    public virtual void UpgradeTower()
    {
        if (level == 1) // Tentar ir para Nível 2
        {
            if (CurrencySystem.SpendMoney(upgradeCostLevel2))
            {
                totalInvested += upgradeCostLevel2;
                level = 2;

                // Aplicar melhoria Nível 2: +50% Alcance
                range = baseRange * 1.5f;

                Debug.Log("Torre melhorada para Nível 2!");
            }
        }
        else if (level == 2) // Tentar ir para Nível 3
        {
            if (CurrencySystem.SpendMoney(upgradeCostLevel3))
            {
                totalInvested += upgradeCostLevel3;
                level = 3;

                // Melhorias de Nível 3 (Dano e Velocidade) são aplicadas no 'Shoot()'

                Debug.Log("Torre melhorada para Nível 3!");
            }
        }
    }

 
    // Vende a torre por 50% do valor total investido

    public virtual void SellTower()
    {
        int sellAmount = totalInvested / 2;
        CurrencySystem.AddMoney(sellAmount);

        // Liberta o TowerSpot
        if (myTowerSpot != null)
        {
            myTowerSpot.isOccupied = false;
            myTowerSpot.currentTower = null;
        }

        // Destrói a torre
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
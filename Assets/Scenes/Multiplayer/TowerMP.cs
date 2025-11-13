using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class TowerMP : NetworkBehaviour
{
    [Header("Stats")]
    public NetworkVariable<float> range = new NetworkVariable<float>(5f);
    public NetworkVariable<int> level = new NetworkVariable<int>(1);

    public float fireRate = 1f;
    public float rotationSpeed = 10f;

    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Parts")]
    public Transform partToRotate;

    [Header("Tower Models (visuals)")]
    public GameObject level1Model;
    public GameObject level2Model;
    public GameObject level3Model;

    private GameObject currentModelInstance;

    [Header("Upgrade Stats")]
    public string towerName = "Archer Tower";
    public int costLevel1 = 100;
    public int upgradeCostLevel2 = 75;
    public int upgradeCostLevel3 = 150;

    [HideInInspector] public int totalInvested;
    [HideInInspector] public TowerSpotMP myTowerSpot;
    [HideInInspector] public ulong donoDaTorreClientId;

    protected float baseRange;
    protected int baseBulletDamage;
    protected float baseBulletSpeed;

    protected Transform target;
    protected float fireCountdown = 0f;

    protected virtual void Start()
    {
        baseRange = range.Value;
        StoreBaseBulletStats();

        if (totalInvested == 0)
            totalInvested = costLevel1;

        // Apenas o servidor instancia o modelo base
        if (IsServer)
            SpawnModelForLevel(level.Value);
    }

    protected virtual void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            BulletMP b = bulletPrefab.GetComponent<BulletMP>();
            if (b != null)
            {
                baseBulletDamage = b.damage;
                baseBulletSpeed = b.speed;
            }
        }
    }

    protected virtual void Update()
    {
        if (!IsServer) return;

        UpdateTarget();

        if (target != null)
            RotateToTarget();

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
        EnemyMP[] enemies = Object.FindObjectsByType<EnemyMP>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        EnemyMP nearest = null;

        foreach (EnemyMP e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < shortestDistance && d <= range.Value)
            {
                shortestDistance = d;
                nearest = e;
            }
        }

        target = nearest != null ? nearest.transform : null;
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
        bulletGO.GetComponent<NetworkObject>().Spawn();

        BulletMP bullet = bulletGO.GetComponent<BulletMP>();

        if (bullet != null)
        {
            bullet.ownerClientId = donoDaTorreClientId;

            if (level.Value == 3)
            {
                bullet.damage = (int)(baseBulletDamage * 1.5f);
                bullet.speed = baseBulletSpeed * 1.5f;
            }

            bullet.Seek(target);
        }
    }

    /// <summary>
    /// Método chamado pelo servidor quando um jogador tenta melhorar a torre.
    /// </summary>
    public void TryUpgrade(ulong playerClientId)
    {
        if (!IsServer) return;

        if (playerClientId != donoDaTorreClientId)
        {
            Debug.LogWarning("Jogador tentou melhorar torre que não é sua.");
            return;
        }

        if (level.Value == 1)
        {
            if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCostLevel2))
            {
                totalInvested += upgradeCostLevel2;
                level.Value = 2;
                range.Value = baseRange * 1.5f;

                // Atualiza modelo visual em todos os clientes
                UpdateTowerModelClientRpc(level.Value);
            }
        }
        else if (level.Value == 2)
        {
            if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCostLevel3))
            {
                totalInvested += upgradeCostLevel3;
                level.Value = 3;

                UpdateTowerModelClientRpc(level.Value);
            }
        }
    }

    [ClientRpc]
    private void UpdateTowerModelClientRpc(int newLevel)
    {
        // Garante que o modelo muda em todos os clientes
        SpawnModelForLevel(newLevel);
    }

    private void SpawnModelForLevel(int lvl)
    {
        if (currentModelInstance != null)
            Destroy(currentModelInstance);

        GameObject modelToSpawn = null;
        if (lvl == 1) modelToSpawn = level1Model;
        else if (lvl == 2) modelToSpawn = level2Model;
        else if (lvl == 3) modelToSpawn = level3Model;

        if (modelToSpawn == null)
        {
            Debug.LogWarning($"TowerMP: Modelo para o nível {lvl} não atribuído!");
            return;
        }

        currentModelInstance = Instantiate(modelToSpawn, transform);
        currentModelInstance.transform.localPosition = Vector3.zero;
        currentModelInstance.transform.localRotation = Quaternion.identity;

        // Atualiza referências
        partToRotate = currentModelInstance.transform.Find("PartToRotate");
        firePoint = currentModelInstance.transform.Find("FirePoint");

        if (firePoint == null)
        {
            Debug.LogWarning($"Modelo de torre (nível {lvl}) não contém FirePoint!");
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (PlayerNetwork.LocalInstance == null)
        {
            Debug.LogError("Não consigo encontrar o PlayerNetwork.LocalInstance!");
            return;
        }

        PlayerID localPlayerId = (PlayerNetwork.LocalInstance.OwnerClientId == 0)
                                 ? PlayerID.JogadorA
                                 : PlayerID.JogadorB;

        if (myTowerSpot == null)
        {
            Debug.LogError("A Torre " + gameObject.name + " não tem referência ao seu TowerSpotMP!");
            return;
        }

        if (myTowerSpot.donoDoSpot != localPlayerId)
        {
            Debug.Log("Este spot não é seu!");
            return;
        }

        if (TowerUpgradeUIMP.Instance != null)
        {
            if (TowerPlacementUIMP.Instance != null)
                TowerPlacementUIMP.Instance.ClosePanel();

            TowerUpgradeUIMP.Instance.OpenPanel(this, myTowerSpot);
        }
        else
        {
            Debug.LogError("FALHA CRÍTICA: TowerUpgradeUIMP.Instance está NULO!");
        }
    }
}

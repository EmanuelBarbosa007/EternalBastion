using UnityEngine;
using Unity.Netcode;

// NOTA: A sua torre base (e as outras, como FireTower)
// devem herdar de TowerMP em vez de Tower
public class TowerMP : NetworkBehaviour
{
    [Header("Stats")]
    // Sincroniza o alcance e o nível para todos os jogadores
    public NetworkVariable<float> range = new NetworkVariable<float>(5f);
    public NetworkVariable<int> level = new NetworkVariable<int>(1);

    public float fireRate = 1f;
    public float rotationSpeed = 10f;

    [Header("Prefabs")]
    public GameObject bulletPrefab; // IMPORTANTE: O seu prefab da bala (Bullet)
                                    // também tem de ter um NetworkObject
    public Transform firePoint;

    [Header("Parts")]
    public Transform partToRotate;

    [Header("Upgrade Stats")]
    public string towerName = "Archer Tower";
    public int costLevel1 = 100;
    public int upgradeCostLevel2 = 75;
    public int upgradeCostLevel3 = 150;

    [HideInInspector] public int totalInvested;
    [HideInInspector] public TowerSpotMP myTowerSpot;

    // NOVO: ID do jogador que é dono da torre
    [HideInInspector] public ulong donoDaTorreClientId;

    protected float baseRange;
    protected int baseBulletDamage;
    protected float baseBulletSpeed;

    protected Transform target;
    protected float fireCountdown = 0f;


    protected virtual void Start()
    {
        baseRange = range.Value; // Lê o valor inicial da NetworkVariable
        StoreBaseBulletStats();

        if (totalInvested == 0)
            totalInvested = costLevel1;
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
        // --- IMPORTANTE ---
        // Apenas o SERVER (Host) pode procurar alvos e disparar
        if (!IsServer) return;

        // O resto da lógica é igual à sua
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
        // (Lógica de encontrar inimigos é igual)
        // NOTA: Deve procurar por "EnemyMP" em vez de "Enemy"
        EnemyMP[] enemies = Object.FindObjectsByType<EnemyMP>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        EnemyMP nearest = null;

        foreach (EnemyMP e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < shortestDistance && d <= range.Value) // Usa range.Value
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
        // (Lógica igual)
        if (partToRotate == null || target == null) return;
        Vector3 dir = target.position - partToRotate.position;
        dir.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        lookRotation *= Quaternion.Euler(0f, 180f, 0f);
        partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    protected virtual void Shoot()
    {
        // Esta função SÓ corre no Server
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Spawnar a bala na rede para todos os clientes a verem
        bulletGO.GetComponent<NetworkObject>().Spawn();

        BulletMP bullet = bulletGO.GetComponent<BulletMP>();

        if (bullet != null)
        {
            // Diz à bala quem é o seu dono
            bullet.ownerClientId = this.donoDaTorreClientId;
            // --- FIM DA LINHA NOVA ---

            if (level.Value == 3)
            {
                bullet.damage = (int)(baseBulletDamage * 1.5f);
                bullet.speed = baseBulletSpeed * 1.5f;
            }
            bullet.Seek(target);
        }
    }


    // (Adicione este método DENTRO da classe TowerMP.cs)

    /// <summary>
    /// Tenta melhorar a torre. SÓ DEVE SER CHAMADO NO SERVIDOR.
    /// O próprio método verifica o nível, gasta o dinheiro e aplica os stats.
    /// </summary>
    public void TryUpgrade(ulong playerClientId)
    {
        // Verificação de segurança (embora já deva estar no server)
        if (!IsServer) return;

        // Verifica se o jogador que está a pedir é o dono
        if (playerClientId != donoDaTorreClientId)
        {
            Debug.LogWarning("Jogador tentou melhorar torre que não é sua.");
            return;
        }

        // Lógica de upgrade (baseada no seu Tower.cs)
        if (level.Value == 1)
        {
            // Tenta gastar o dinheiro do jogador que pediu
            if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCostLevel2))
            {
                totalInvested += upgradeCostLevel2;
                level.Value = 2; // Sincroniza o nível para todos

                // AQUI ESTÁ A CORREÇÃO:
                // Como estamos DENTRO da classe TowerMP, podemos aceder a 'baseRange'.
                range.Value = baseRange * 1.5f; // Sincroniza o alcance para todos
            }
        }
        else if (level.Value == 2)
        {
            if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCostLevel3))
            {
                totalInvested += upgradeCostLevel3;
                level.Value = 3; // Sincroniza o nível para todos

                // (O dano/velocidade extra é aplicado no método 'Shoot()',
                // que já está correto)
            }
        }
        // Se já for nível 3, não faz nada.
    }
    // Os métodos de Upgrade/Sell serão chamados
    // pelo seu UI de upgrade (TowerUpgradeUIMP)
    // O UI é que deve enviar o ServerRpc para o PlayerNetwork
}
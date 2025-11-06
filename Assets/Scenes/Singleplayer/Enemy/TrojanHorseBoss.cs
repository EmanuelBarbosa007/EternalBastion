using UnityEngine;
using UnityEngine.AI;

public class TrojanHorseBoss : MonoBehaviour
{
    [Header("Navegação")]
    public float speed = 2f;

    private Transform baseTarget;
    private NavMeshAgent agent;
    private static BaseHealth baseHealth;

    [Header("Atributos do Boss")]
    public int damageToBase = 10;
    public int enemiesToSpawn = 5;
    public GameObject[] enemyPrefabs;

    // Variável para garantir que as tropas só spawnam uma vez
    private bool troopsSpawned = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent não encontrado no Boss!", this);
            return;
        }

        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null)
        {
            baseTarget = baseObject.transform;
            agent.speed = speed;
            agent.SetDestination(baseTarget.position);
        }
        else
        {
            Debug.LogError("Não foi possível encontrar a Base! Verifica se o teu objeto 'Base' tem a Tag 'Base'.", this);
        }

        if (baseHealth == null)
            baseHealth = Object.FindFirstObjectByType<BaseHealth>();
    }

    void Update()
    {
        if (agent != null && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    ReachBase();
                }
            }
        }
    }

    void ReachBase()
    {
        // Causa dano à base
        if (baseHealth != null)
            baseHealth.TakeDamage(damageToBase);
        else
            Debug.LogWarning("Não foi possível encontrar BaseHealth para dar dano.");

        // Decrementa a contagem de inimigos (o boss vai ser destruído)
        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        // Chama a função de spawn
        SpawnTroops(); 

        // Destroi o objeto do boss
        Destroy(gameObject);
    }

    public void SpawnTroops()
    {
        if (troopsSpawned) return;
        troopsSpawned = true;

        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            Debug.Log("Cavalo de Troia foi destruído! A libertar inimigos...");

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject prefabToSpawn = enemyPrefabs[randomIndex];

                Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

                Instantiate(prefabToSpawn, spawnPosition, transform.rotation);

                // Adiciona o novo inimigo à contagem
                EnemySpawner.EnemiesAlive++;
            }
        }
        else
        {
            Debug.LogWarning("O Boss Cavalo de Troia não tem prefabs de inimigos para 'spawnar'!", this);
        }
    }
}
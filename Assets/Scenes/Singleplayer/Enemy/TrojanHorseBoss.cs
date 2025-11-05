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
    [Tooltip("Dano que o boss causa à base ao chegar.")]
    public int damageToBase = 10; // mais dano à torre

    [Tooltip("Número de inimigos a 'spawnar' ao chegar à base.")]
    public int enemiesToSpawn = 5;

    [Tooltip("Array de prefabs de inimigos (Normal, Tanque, Cavalo) para 'spawnar' aleatoriamente.")]
    public GameObject[] enemyPrefabs; // Arraste aqui os seus 3 prefabs de inimigos


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
                    ReachBase(); // Chegou à base
                }
            }
        }
    }

    void ReachBase()
    {
        // 1. Causa dano à base
        if (baseHealth != null)
            baseHealth.TakeDamage(damageToBase);
        else
            Debug.LogWarning("Não foi possível encontrar BaseHealth para dar dano.");


        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;



        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            Debug.Log("Cavalo de Troia chegou à base! A libertar inimigos...");

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                // Escolhe um prefab aleatório da lista
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject prefabToSpawn = enemyPrefabs[randomIndex];

                // Posição de spawn
                Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

                // Cria o novo inimigo
                Instantiate(prefabToSpawn, spawnPosition, transform.rotation);

                //  Adiciona o novo inimigo à contagem de inimigos vivos
                EnemySpawner.EnemiesAlive++;
            }
        }
        else
        {
            Debug.LogWarning("O Boss Cavalo de Troia não tem prefabs de inimigos para 'spawnar'!", this);
        }

        // 4. Destroi o objeto do boss
        Destroy(gameObject);
    }
}
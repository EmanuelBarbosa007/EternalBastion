using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Necessário para o IEnumerator

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

    [Header("Configuração do Spawn")]
    public float timeBetweenSpawns = 0.5f; // Tempo entre cada tropa

    // --- AQUI ESTAVA A VARIÁVEL EM FALTA ---
    private bool isDead = false;

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
            Debug.LogError("Erro: Objeto 'Base' não encontrado.", this);
        }

        if (baseHealth == null)
            baseHealth = Object.FindFirstObjectByType<BaseHealth>();
    }

    void Update()
    {
        // Se estiver morto, não faz nada no Update
        if (isDead) return;

        if (agent != null && !agent.pathPending && agent.isActiveAndEnabled)
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
        if (isDead) return;

        // Causa dano à base
        if (baseHealth != null)
            baseHealth.TakeDamage(damageToBase);

        // Decrementa a contagem de inimigos (porque o cavalo vai morrer agora)
        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        // Inicia a morte e o spawn
        StartDeathSequence();
    }

    // Esta função é chamada pelo script de Vida (EnemyHealth) ou pelo ReachBase
    public void StartDeathSequence()
    {
        if (isDead) return; // Garante que só corre uma vez
        isDead = true;

        StartCoroutine(SpawnTroopsRoutine());
    }

    IEnumerator SpawnTroopsRoutine()
    {
        Debug.Log("Cavalo destruído. A libertar tropas com intervalo...");

        // 1. DESATIVAR O CAVALO (Esconder visualmente)
        if (agent != null) agent.enabled = false;

        // Tenta apanhar o Renderer no próprio objeto ou nos filhos (caso o modelo 3D esteja dentro)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders) c.enabled = false;

        // 2. SPAWNAR TROPAS COM TIMER
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                int randomIndex = Random.Range(0, enemyPrefabs.Length);

                // Pequeno ajuste na posição para não ficarem uns em cima dos outros
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f));
                Vector3 spawnPos = transform.position + offset;

                Instantiate(enemyPrefabs[randomIndex], spawnPos, transform.rotation);

                // Adiciona à contagem global do jogo
                EnemySpawner.EnemiesAlive++;

                // O TIMER: Espera X segundos antes do próximo
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        // 3. DESTRUIR O OBJETO FINALMENTE
        Destroy(gameObject);
    }
}
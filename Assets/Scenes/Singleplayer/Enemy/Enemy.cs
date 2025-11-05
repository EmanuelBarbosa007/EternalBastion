using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;

    [Header("AI Pathfinding")]
    [Tooltip("De quantos em quantos segundos o inimigo recalcula o caminho? ")]
    public float pathUpdateRate = 1.0f;

    private Transform baseTarget;
    private NavMeshAgent agent;
    private float startY;
    private static BaseHealth baseHealth;

    void Start()
    {
        startY = transform.position.y;

        // Obtem o componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent não encontrado no inimigo!", this);
            return;
        }

        // Encontra a base pela Tag "Base" 
        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null)
        {
            baseTarget = baseObject.transform;
        }
        else
        {
            Debug.LogError("Não foi possível encontrar a Base! Verifica se o teu objeto 'Base' tem a Tag 'Base'.", this);
            return;
        }

        // Cache da vida da base 
        if (baseHealth == null)
            baseHealth = Object.FindFirstObjectByType<BaseHealth>();

        // Define a velocidade do agente
        agent.speed = speed;

        // Aplica os custos de área (para evitar zonas perigosas)
        ApplyAreaCosts();

        // Inicia a rotina de atualização de caminho
        StartCoroutine(UpdatePath());
    }

    private void ApplyAreaCosts()
    {
        // Aplica manualmente os custos de área — isto força o agente a respeitar as zonas perigosas
        SetCost("Walkable", 1f);
        SetCost("DangerLevel1", 5f);
        SetCost("DangerLevel2", 15f);
        SetCost("DangerLevel3", 40f);
    }

    private void SetCost(string areaName, float cost)
    {
        int index = NavMesh.GetAreaFromName(areaName);
        if (index != -1)
        {
            agent.SetAreaCost(index, cost);
        }
        else
        {
            Debug.LogWarning($"Área '{areaName}' não encontrada no NavMesh. Verifica em Navigation > Areas.");
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            if (baseTarget != null && agent.isOnNavMesh && agent.enabled)
            {
                agent.SetDestination(baseTarget.position);
            }
            yield return new WaitForSeconds(pathUpdateRate);
        }
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
        StopAllCoroutines();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (baseHealth != null)
            baseHealth.TakeDamage(1);
        else
            Debug.LogWarning("Não foi possível encontrar BaseHealth para dar dano.");

        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        Destroy(gameObject);
    }
}

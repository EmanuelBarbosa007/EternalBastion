using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;


    private Transform baseTarget;  
    private NavMeshAgent agent;    //componente de navega��o
    private float startY;          // manter a altura


    // 3. Manter a refer�ncia est�tica para a vida da base
    private static BaseHealth baseHealth;

    void Start()
    {

        startY = transform.position.y;

        // Obtem o componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent n�o encontrado no inimigo!", this);
            return;
        }

        // Encontra a base pela Tag "Base" 
        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null)
        {
            baseTarget = baseObject.transform;


            agent.speed = speed;
            agent.SetDestination(baseTarget.position);
        }
        else
        {
            Debug.LogError("N�o foi poss�vel encontrar a Base! Verifica se o teu objeto 'Base' tem a Tag 'Base'.", this);
        }

        // Cache da vida da base 
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
                    ReachBase(); // Chegou � base
                }
            }
        }
    }

    void ReachBase()
    {
        if (baseHealth != null)
            baseHealth.TakeDamage(1); // tira 1 de vida da base
        else
            Debug.LogWarning("N�o foi poss�vel encontrar BaseHealth para dar dano.");


        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        // Destroi o inimigo
        Destroy(gameObject);
    }

}
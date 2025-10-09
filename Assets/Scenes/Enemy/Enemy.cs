using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;

    private Transform target;
    private int waypointIndex = 0;
    private float startY;

    private static BaseHealth baseHealth;

    void Start()
    {
        target = WaypointPath.points[0];
        startY = transform.position.y;

        // Cache da base (evita chamar Find a cada inimigo)
        if (baseHealth == null)
            baseHealth = Object.FindFirstObjectByType<BaseHealth>();
    }

    void Update()
    {
        if (target == null) return;

        // Direção até ao próximo waypoint (ignora eixo Y)
        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        // Mantém sempre a mesma altura
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        if (HasReachedTarget())
        {
            GetNextWaypoint();
        }
    }

    bool HasReachedTarget()
    {
        Vector3 posXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetXZ = new Vector3(target.position.x, 0, target.position.z);
        return Vector3.Distance(posXZ, targetXZ) < 0.2f; 
    }

    void GetNextWaypoint()
    {
        waypointIndex++;

        // Se chegou ao fim (a base)
        if (waypointIndex >= WaypointPath.points.Length)
        {
            if (baseHealth != null)
                baseHealth.TakeDamage(1); // tira 1 de vida da base

            Destroy(gameObject); // inimigo desaparece
            return;
        }

        target = WaypointPath.points[waypointIndex];
    }
}

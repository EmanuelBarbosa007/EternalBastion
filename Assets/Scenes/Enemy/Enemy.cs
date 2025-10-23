using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    public float rotateSpeed = 10f;

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

        // Dire��o at� ao pr�ximo waypoint (ignora eixo Y)
        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        
        if (dir != Vector3.zero)
        {
            // Calcula a rota��o necess�ria para olhar na dire��o do movimento
            Quaternion lookRotation = Quaternion.LookRotation(dir.normalized);

            // Suaviza a rota��o para n�o ser instant�nea
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
        }

        // Move o inimigo
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        // Mant�m sempre a mesma altura
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


            // Avisa o spawner que este inimigo chegou � base
            if (EnemySpawner.EnemiesAlive > 0)
                EnemySpawner.EnemiesAlive--;
            // --- FIM DA LINHA ---

            Destroy(gameObject); // inimigo desaparece
            return;
        }

        target = WaypointPath.points[waypointIndex];
    }
}
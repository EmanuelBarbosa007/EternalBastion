using UnityEngine;

public class PiercingBullet : MonoBehaviour
{
    private Vector3 direction;
    public float speed = 20f;
    public int damage = 40;
    public float lifetime = 10f;

    private float lifeTimer = 0f;

    public void Seek(Transform target)
    {
        direction = (target.position - transform.position).normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth e = other.GetComponent<EnemyHealth>();
        if (e != null)
        {
            e.TakeDamage(damage);
        }
    }
}

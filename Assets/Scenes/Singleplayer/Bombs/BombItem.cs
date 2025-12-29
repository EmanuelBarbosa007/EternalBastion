using UnityEngine;

public class BombItem : MonoBehaviour
{
    public int damage = 5;        // Dano pedido
    public float explosionRadius = 3f; // Raio da explosão
    public GameObject explosionVFX; 

    [Header("Audio")]
    public AudioClip explosionSound; 

    // Quando algo entra no Trigger da bomba
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto é um inimigo 
        if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        //  Criar efeito visual se existir
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        // 2. Detetar todos os coliders dentro do raio
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hitCollider in hitColliders)
        {
            // Tenta encontrar o script EnemyHealth
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // 3. Destruir a bomba
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
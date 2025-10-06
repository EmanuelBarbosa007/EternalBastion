using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float timeBetweenWaves = 20f;
    public int enemiesPerWave = 5;
    public float spawnInterval = 1f;

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (true)
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                yield return new WaitForSeconds(spawnInterval);
            }
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
}

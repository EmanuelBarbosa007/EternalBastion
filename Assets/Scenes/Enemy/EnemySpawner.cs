using System.Collections;
using UnityEngine;
/*
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
} */
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject firstEnemyPrefab;
    public GameObject lastEnemyPrefab;

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
                GameObject prefab;

                // primeiro da wave
                if (i == 0 && firstEnemyPrefab != null)
                    prefab = firstEnemyPrefab;
                // último da wave
                else if (i == enemiesPerWave - 1 && lastEnemyPrefab != null)
                    prefab = lastEnemyPrefab;
                else
                    prefab = enemyPrefab;

                Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
}

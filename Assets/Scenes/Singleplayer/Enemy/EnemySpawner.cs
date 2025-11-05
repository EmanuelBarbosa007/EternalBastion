using System.Collections;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnState { SPAWNING, WAITING, COUNTDOWN }
    private SpawnState state = SpawnState.COUNTDOWN;

    [Header("Prefabs dos Inimigos")]
    public GameObject enemyPrefab; // Inimigo normal
    public GameObject firstEnemyPrefab; // Inimigo Tanque
    public GameObject lastEnemyPrefab; // Inimigo Cavalo
    public GameObject bossPrefab; // cavalo de troia

    [Header("Referências")]
    [Tooltip("Pontos de spawn (2 ou mais)")]
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI countdownText;

    [Header("Stats Iniciais da Wave")]
    public float timeBetweenWaves = 20f;
    public int enemiesPerWave = 5;
    public float spawnInterval = 1f;

    [Header("Escala de Dificuldade (por wave)")]
    [Tooltip("Quantos inimigos NORMAIS a mais por wave")]
    public int enemiesPerWaveIncrease = 2;
    [Tooltip("Em que wave começam a aparecer Tanques")]
    public int waveToStartTanks = 3;
    [Tooltip("Em que wave começam a aparecer Cavalos")]
    public int waveToStartHorses = 5;

    [Space]
    [Tooltip("Quanto mais rápido fica o spawn de inimigos (ex: 0.05)")]
    public float spawnIntervalDecrease = 0.05f;
    [Tooltip("O tempo mínimo de spawn (para não ser 0)")]
    public float minSpawnInterval = 0.2f;

    [Space]
    [Tooltip("Quanto tempo a menos de espera entre waves (ex: 0.5)")]
    public float timeBetweenWavesDecrease = 0.5f;
    [Tooltip("O tempo mínimo de espera")]
    public float minTimeBetweenWaves = 5f;

    [Header("Stats do Boss")] 
    [Tooltip("A cada quantas waves o boss deve 'spawnar' (ex: 5 = Wave 5, 10, 15...)")] 
    public int bossWaveFrequency = 5; 
    [Tooltip("Quantos bosses 'spawnam' nessa wave.")] 
    public int bossCount = 1; 

    private int waveNumber = 1;
    private float countdown;

    public static int EnemiesAlive = 0;

    private void Start()
    {
        EnemiesAlive = 0;
        countdown = timeBetweenWaves;
        state = SpawnState.COUNTDOWN;
        UpdateWaveUI();
    }

    private void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (EnemiesAlive <= 0)
            {
                StartWaveCountdown();
            }
            return;
        }

        if (state == SpawnState.COUNTDOWN)
        {
            countdown -= Time.deltaTime;
            countdown = Mathf.Max(0, countdown);
            countdownText.text = $"Próxima Wave em: {Mathf.CeilToInt(countdown)}s";

            if (countdown <= 0f)
            {
                state = SpawnState.SPAWNING;
                countdownText.text = "Ataque!";
                StartCoroutine(SpawnWave());
            }
        }
    }

    void StartWaveCountdown()
    {
        state = SpawnState.COUNTDOWN;

        enemiesPerWave += enemiesPerWaveIncrease;
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease);
        timeBetweenWaves = Mathf.Max(minTimeBetweenWaves, timeBetweenWaves - timeBetweenWavesDecrease);

        countdown = timeBetweenWaves;

        waveNumber++;
        UpdateWaveUI();
        Debug.Log("Wave completa! A preparar a Wave " + waveNumber);
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
            waveText.text = "Wave " + waveNumber;
    }

    IEnumerator SpawnWave()
    {
        EnemiesAlive = 0;

        // Spawna os TANQUES
        if (waveNumber >= waveToStartTanks && firstEnemyPrefab != null)
        {
            int tankCount = waveNumber / waveToStartTanks;
            for (int i = 0; i < tankCount; i++)
            {
                SpawnEnemy(firstEnemyPrefab);
                yield return new WaitForSeconds(spawnInterval * 2);
            }
        }

        // Spawna os inimigos NORMAIS
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        // Spawna os CAVALOS
        if (waveNumber >= waveToStartHorses && lastEnemyPrefab != null)
        {
            int horseCount = waveNumber / waveToStartHorses;
            for (int i = 0; i < horseCount; i++)
            {
                SpawnEnemy(lastEnemyPrefab);
                yield return new WaitForSeconds(spawnInterval * 0.5f);
            }
        }

        // Verifica se é uma "Boss Wave" 
        if (waveNumber % bossWaveFrequency == 0 && bossPrefab != null) 
        {
            Debug.LogWarning("WAVE " + waveNumber + " É UMA BOSS WAVE! A 'spawnar' Cavalo de Troia...");

            // Espera um pouco mais para o boss entrar
            yield return new WaitForSeconds(spawnInterval * 3); 

            // Spawna a quantidade de bosses definida
            for (int i = 0; i < bossCount; i++) // <-- NOVO
            {
                SpawnEnemy(bossPrefab); // <-- NOVO
                yield return new WaitForSeconds(spawnInterval * 2); // Intervalo entre bosses (se for mais que 1)
            }
        }

        state = SpawnState.WAITING;
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Nenhum ponto de spawn atribuído!");
            return;
        }

        // Escolhe aleatoriamente um ponto de spawn
        Transform chosenSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(prefab, chosenSpawn.position, chosenSpawn.rotation);
        EnemiesAlive++;
    }
}
using System.Collections;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    // Enum para sabermos o que o spawner está a fazer
    public enum SpawnState { SPAWNING, WAITING, COUNTDOWN }
    private SpawnState state = SpawnState.COUNTDOWN;

    [Header("Prefabs dos Inimigos")]
    public GameObject enemyPrefab; // Inimigo normal
    public GameObject firstEnemyPrefab; // Inimigo Tanque
    public GameObject lastEnemyPrefab; // Inimigo Cavalo

    [Header("Referências")]
    public Transform spawnPoint;
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


    
    private int waveNumber = 1;
    private float countdown;

    
    public static int EnemiesAlive = 0;

    private void Start()
    {
        EnemiesAlive = 0;
        countdown = timeBetweenWaves; // Começa a primeira contagem
        state = SpawnState.COUNTDOWN;
        UpdateWaveUI();
    }

    private void Update()
    {
        // espera que o jogador mate os inimigos
        if (state == SpawnState.WAITING)
        {
            if (EnemiesAlive <= 0)
            {
                // Começa a contagem para a próxima wave
                StartWaveCountdown();
            }
            return; // Continua a esperar
        }

        //
        if (state == SpawnState.COUNTDOWN)
        {
            countdown -= Time.deltaTime;
            countdown = Mathf.Max(0, countdown); // Impede que vá a negativo
            countdownText.text = $"Próxima Wave em: {Mathf.CeilToInt(countdown)}s";

            if (countdown <= 0f)
            {
                // Acaba a contagem e começa a spawnar
                state = SpawnState.SPAWNING;
                countdownText.text = "Ataque!";
                StartCoroutine(SpawnWave());
            }
        }
    }

    void StartWaveCountdown()
    {
        state = SpawnState.COUNTDOWN;

        // dificuldade progressiva
        enemiesPerWave += enemiesPerWaveIncrease;
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease);
        timeBetweenWaves = Mathf.Max(minTimeBetweenWaves, timeBetweenWaves - timeBetweenWavesDecrease);

        countdown = timeBetweenWaves; // Reinicia a contagem

        waveNumber++; // Incrementa a wave
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
        EnemiesAlive = 0; // Zera o contador no início da wave

        //Spawna os TANQUES
        if (waveNumber >= waveToStartTanks && firstEnemyPrefab != null)
        {
            int tankCount = waveNumber / waveToStartTanks;
            for (int i = 0; i < tankCount; i++)
            {
                SpawnEnemy(firstEnemyPrefab);
                yield return new WaitForSeconds(spawnInterval * 2); // Tanques saem mais devagar
            }
        }

        //Spawna os inimigos NORMAIS
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        //Spawna os CAVALOS
        if (waveNumber >= waveToStartHorses && lastEnemyPrefab != null)
        {
            int horseCount = waveNumber / waveToStartHorses;
            for (int i = 0; i < horseCount; i++)
            {
                SpawnEnemy(lastEnemyPrefab);
                yield return new WaitForSeconds(spawnInterval * 0.5f); // Cavalos saem mais rápido
            }
        }

        state = SpawnState.WAITING; 
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoint == null) return;

        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        EnemiesAlive++; // Adiciona ao contador
    }
}
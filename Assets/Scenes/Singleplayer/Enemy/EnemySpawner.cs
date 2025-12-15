using System.Collections;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnState { SPAWNING, WAITING, COUNTDOWN }

    [Header("Prefabs")]
    public GameObject baseEnemyPrefab;
    public GameObject horsePrefab;
    public GameObject bossPrefab;

    [Header("Assets para Decorators")]
    public Material tankMaterial;

    // --- SONS DE MORTE (Para os Decorators) ---
    [Header("Sons de Morte (Inimigos)")]
    public AudioClip normalDeathSound;
    public AudioClip tankDeathSound;

    [Header("Referências UI e Spawn")]
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI countdownText;

    [Header("Sistema de Áudio das Waves")]
    public AudioSource audioSource;
    public AudioClip startGameAudio; // "Inimigos a caminho"
    public AudioClip firstWaveAudio; // "Primeira Onda"
    public AudioClip bossWaveAudio;  // "Boss a chegar"

    [Header("Stats das Waves")]
    public float timeBetweenWaves = 20f;
    public int enemiesPerWave = 5;
    public float spawnInterval = 1f;

    [Header("Dificuldade")]
    public int enemiesPerWaveIncrease = 2;
    public int waveToStartTanks = 3;
    public int waveToStartHorses = 5;
    public float spawnIntervalDecrease = 0.05f;
    public float minSpawnInterval = 0.2f;
    public float timeBetweenWavesDecrease = 0.5f;
    public float minTimeBetweenWaves = 5f;

    [Header("Boss")]
    public int bossWaveFrequency = 5;
    public int bossCount = 1;

    private int waveNumber = 1;
    private float countdown;
    private SpawnState state = SpawnState.COUNTDOWN;
    public static int EnemiesAlive = 0;

    private enum EnemyType { Normal, Tank, Horse }

    private void Start()
    {
        EnemiesAlive = 0;
        countdown = timeBetweenWaves;
        state = SpawnState.COUNTDOWN;
        UpdateWaveUI();

        if (audioSource != null)
        {
            if (startGameAudio != null)
                audioSource.PlayOneShot(startGameAudio);

        }
    }

    private void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (EnemiesAlive <= 0) StartWaveCountdown();
            return;
        }

        if (state == SpawnState.COUNTDOWN)
        {
            countdown -= Time.deltaTime;
            countdown = Mathf.Max(0, countdown);
            if (countdownText) countdownText.text = $"Próxima Wave: {Mathf.CeilToInt(countdown)}s";

            if (countdown <= 0f)
            {
                state = SpawnState.SPAWNING;
                if (countdownText) countdownText.text = "ATAQUE!";


                // Toca o áudio da Primeira Onda no momento exato em que o ataque começa
                if (waveNumber == 1 && audioSource != null && firstWaveAudio != null)
                {
                    audioSource.PlayOneShot(firstWaveAudio);
                }

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

        waveNumber++; // Incrementa a onda

        UpdateWaveUI();

        // Lógica de Áudio do Boss
        if (waveNumber % bossWaveFrequency == 0)
        {
            if (audioSource != null && bossWaveAudio != null)
            {
                audioSource.PlayOneShot(bossWaveAudio);
            }
        }
    }

    void UpdateWaveUI()
    {
        if (waveText) waveText.text = "Wave " + waveNumber;
    }

    IEnumerator SpawnWave()
    {
        EnemiesAlive = 0;

        // 1. Spawna TANQUES
        if (waveNumber >= waveToStartTanks)
        {
            int tankCount = waveNumber / waveToStartTanks;
            for (int i = 0; i < tankCount; i++)
            {
                SpawnEnemy(EnemyType.Tank);
                yield return new WaitForSeconds(spawnInterval * 2);
            }
        }

        // 2. Spawna INIMIGOS NORMAIS
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy(EnemyType.Normal);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 3. Spawna CAVALOS
        if (waveNumber >= waveToStartHorses)
        {
            int horseCount = waveNumber / waveToStartHorses;
            for (int i = 0; i < horseCount; i++)
            {
                SpawnEnemy(EnemyType.Horse);
                yield return new WaitForSeconds(spawnInterval * 0.5f);
            }
        }

        // 4. Boss Logic
        if (waveNumber % bossWaveFrequency == 0 && bossPrefab != null)
        {
            yield return new WaitForSeconds(spawnInterval * 3);
            for (int i = 0; i < bossCount; i++)
            {
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Instantiate(bossPrefab, sp.position, sp.rotation);
                EnemiesAlive++;
                yield return new WaitForSeconds(spawnInterval * 2);
            }
        }

        state = SpawnState.WAITING;
    }

    void SpawnEnemy(EnemyType type)
    {
        if (spawnPoints.Length == 0) return;
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject objectToSpawn = null;

        if (type == EnemyType.Horse)
        {
            if (horsePrefab != null)
            {
                Instantiate(horsePrefab, sp.position, sp.rotation);
                EnemiesAlive++;
            }
            return;
        }
        else
        {
            objectToSpawn = Instantiate(baseEnemyPrefab, sp.position, sp.rotation);
        }

        IEnemyDecorator decorator = null;

        switch (type)
        {
            case EnemyType.Normal:
                decorator = new NormalEnemyDecorator(normalDeathSound);
                break;
            case EnemyType.Tank:
                decorator = new TankEnemyDecorator(tankMaterial, tankDeathSound);
                break;
        }

        if (decorator != null && objectToSpawn != null)
        {
            decorator.Decorate(objectToSpawn);
        }

        EnemiesAlive++;
    }
}
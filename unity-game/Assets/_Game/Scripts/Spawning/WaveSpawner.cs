using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject playerTarget;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Configuration")]
    [SerializeField] private int[] enemiesPerWave = { 2, 3, 4 };

    [Min(0f)]
    [SerializeField] private float spawnInterval = 0.75f;

    [Min(0f)]
    [SerializeField] private float interWaveDelay = 2f;

    private readonly Queue<Transform> pendingSpawnPoints = new();
    private readonly List<GameObject> activeEnemies = new();

    private PlayerHealth playerHealth;

    private int currentWaveIndex;
    private int currentSpawnPointIndex;

    private float spawnTimer;
    private float interWaveTimer;

    private bool waitingForNextWave;
    private bool runCompleted;

    public int CurrentWaveNumber => currentWaveIndex + 1;

    public int PendingEnemyCount => pendingSpawnPoints.Count;

    public int ActiveEnemyCount => activeEnemies.Count;

    private void Awake()
    {
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

        spawnInterval = Mathf.Max(0f, spawnInterval);
        interWaveDelay = Mathf.Max(0f, interWaveDelay);
    }

    private void Start()
    {
        BeginCurrentWave();
    }

    private void Update()
    {
        if (runCompleted)
        {
            return;
        }

        if (playerHealth.CurrentHealth <= 0f)
        {
            StopForPlayerDeath();
            return;
        }

        RemoveDestroyedEnemies();

        if (pendingSpawnPoints.Count > 0)
        {
            UpdateEnemySpawning();
            return;
        }

        if (activeEnemies.Count > 0)
        {
            return;
        }

        UpdateWaveProgression();
    }

    private bool ValidateConfiguration()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError(
                "WaveSpawner requires an enemy prefab.",
                this);

            return false;
        }

        if (enemyPrefab.GetComponent<EnemyController>() == null)
        {
            Debug.LogError(
                "WaveSpawner enemy prefab requires an EnemyController component.",
                this);

            return false;
        }

        if (playerTarget == null)
        {
            Debug.LogError(
                "WaveSpawner requires a player target.",
                this);

            return false;
        }

        playerHealth = playerTarget.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError(
                "WaveSpawner player target requires a PlayerHealth component.",
                this);

            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "WaveSpawner requires at least one spawn point.",
                this);

            return false;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError(
                    $"WaveSpawner spawn point at index {i} is missing.",
                    this);

                return false;
            }
        }

        if (enemiesPerWave == null || enemiesPerWave.Length == 0)
        {
            Debug.LogError(
                "WaveSpawner requires at least one configured wave.",
                this);

            return false;
        }

        for (int i = 0; i < enemiesPerWave.Length; i++)
        {
            if (enemiesPerWave[i] <= 0)
            {
                Debug.LogError(
                    $"WaveSpawner wave {i + 1} must contain at least one enemy.",
                    this);

                return false;
            }
        }

        if (spawnInterval < 0f)
        {
            Debug.LogWarning(
                "WaveSpawner spawn interval was negative and will be clamped to zero.",
                this);
        }

        if (interWaveDelay < 0f)
        {
            Debug.LogWarning(
                "WaveSpawner inter-wave delay was negative and will be clamped to zero.",
                this);
        }

        return true;
    }

    private void BeginCurrentWave()
    {
        int enemyCount = enemiesPerWave[currentWaveIndex];

        for (int i = 0; i < enemyCount; i++)
        {
            Transform spawnPoint =
                spawnPoints[currentSpawnPointIndex % spawnPoints.Length];

            pendingSpawnPoints.Enqueue(spawnPoint);
            currentSpawnPointIndex++;
        }

        spawnTimer = 0f;

        // 每波第一只敌人立即生成。
        SpawnNextEnemy();
    }

    private void UpdateEnemySpawning()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
        {
            return;
        }

        SpawnNextEnemy();
        spawnTimer = 0f;
    }

    private void SpawnNextEnemy()
    {
        if (pendingSpawnPoints.Count == 0)
        {
            return;
        }

        Transform spawnPoint = pendingSpawnPoints.Dequeue();

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity);

        EnemyController enemyController =
            enemy.GetComponent<EnemyController>();

        enemyController.Initialize(playerTarget.transform);
        activeEnemies.Add(enemy);
    }

    private void RemoveDestroyedEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    private void UpdateWaveProgression()
    {
        if (currentWaveIndex >= enemiesPerWave.Length - 1)
        {
            CompleteRun();
            return;
        }

        if (!waitingForNextWave)
        {
            waitingForNextWave = true;
            interWaveTimer = 0f;
        }

        interWaveTimer += Time.deltaTime;

        if (interWaveTimer < interWaveDelay)
        {
            return;
        }

        currentWaveIndex++;
        waitingForNextWave = false;
        BeginCurrentWave();
    }

    private void CompleteRun()
    {
        runCompleted = true;
        Debug.Log("All waves completed!", this);
        enabled = false;
    }

    private void StopForPlayerDeath()
    {
        Debug.Log("Player is dead. Stopping enemy spawning.", this);

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            GameObject enemy = activeEnemies[i];

            if (enemy == null)
            {
                continue;
            }

            EnemyController enemyController =
                enemy.GetComponent<EnemyController>();

            if (enemyController != null)
            {
                enemyController.enabled = false;
            }
        }

        enabled = false;
    }
}
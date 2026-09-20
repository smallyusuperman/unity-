using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] private WaveConfig[] waveConfigs;

    [Header("Scene References")]
    [SerializeField] private GameObject playerTarget;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Configuration")]
    [Min(0f)]
    [SerializeField] private float interWaveDelay = 2f;

    [SerializeField] private ScenePathfindingGrid pathfindingGrid;

    private readonly Queue<Transform> pendingSpawnPoints = new();
    private readonly List<GameObject> activeEnemies = new();

    private PlayerHealth playerHealth;

    private int currentWaveIndex;
    private int currentSpawnPointIndex;

    private float spawnTimer;
    private float currentSpawnInterval;
    private float interWaveTimer;

    private bool waitingForNextWave;
    private bool runCompleted;

    /// <summary>当前波次序号，从 1 开始。</summary>
    public int CurrentWaveNumber => currentWaveIndex + 1;

    /// <summary>本波剩余待生成的敌人数量。</summary>
    public int PendingEnemyCount => pendingSpawnPoints.Count;

    /// <summary>当前记为存活的敌人数量；已销毁的对象会在下一次 Update 中被剔除。</summary>
    public int ActiveEnemyCount => activeEnemies.Count;

    private void Awake()
    {
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }
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
        if (waveConfigs == null || waveConfigs.Length == 0)
        {
            Debug.LogError(
            "WaveSpawner requires at least one wave config.",
            this);

            return false;
        }
        for (int i = 0; i < waveConfigs.Length; i++)
        {
            if (waveConfigs[i] == null)
            {
                Debug.LogError(
                    $"WaveSpawner wave config at index {i} is missing.",
                    this);

                return false;
            }
        }

        for (int i = 0; i < waveConfigs.Length; i++)
        {
            if (waveConfigs[i].enemyPrefab == null)
            {
                Debug.LogError(
                    "WaveSpawner requires an enemy prefab.",
                    this);

                return false;
            }
        }

        for (int i = 0; i < waveConfigs.Length; i++)
        {
            if (waveConfigs[i].enemyPrefab.GetComponent<EnemyController>() == null)
            {
                Debug.LogError(
                    "WaveSpawner enemy prefab requires an EnemyController component.",
                    this);

                return false;
            }
        }

        if (playerTarget == null)
        {
            Debug.LogError(
                "WaveSpawner requires a player target.",
                this);

            return false;
        }

        if (pathfindingGrid == null)
        {
            Debug.LogError(
                "WaveSpawner requires a ScenePathfindingGrid.",
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

        for (int i = 0; i < waveConfigs.Length; i++)
        {
            if (waveConfigs[i].enemyCount <= 0)
            {
                Debug.LogError(
                    $"WaveSpawner wave {i + 1} must contain at least one enemy.",
                    this);

                return false;
            }
        }
        for (int i = 0; i < waveConfigs.Length; i++)
        {
            if (waveConfigs[i].spawnInterval < 0f)
            {
                Debug.LogWarning(
                    "WaveSpawner spawn interval was negative and will be clamped to zero.",
                    this);
            }
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
        WaveConfig currentWaveConfig = waveConfigs[currentWaveIndex];
        int enemyCount = currentWaveConfig.enemyCount;
        currentSpawnInterval = Mathf.Max(0f, currentWaveConfig.spawnInterval);

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

        if (spawnTimer < currentSpawnInterval)
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
            waveConfigs[currentWaveIndex].enemyPrefab,
            spawnPoint.position,
            Quaternion.identity);

        EnemyController enemyController =
            enemy.GetComponent<EnemyController>();

        enemyController.Initialize(playerTarget.transform, pathfindingGrid);
        activeEnemies.Add(enemy);
    }

    // activeEnemies 保存的是 GameObject 引用。敌人死亡时 EnemyHealth 会 Destroy 自身，
    // 此时 Unity 重载的 == 使该引用与 null 相等，因此这里能识别出已销毁的敌人。
    // 倒序遍历是为了在 RemoveAt 之后不跳过后续元素。
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
        if (currentWaveIndex >= waveConfigs.Length - 1)
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

using UnityEngine;

public class TestEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject EnemyPrefab;

    [SerializeField] private GameObject SpawnPoint;

    [SerializeField] private float spawnTimer;

    [SerializeField] private int spawnCount;

    [SerializeField] private Transform newTarget;
    [SerializeField] private ScenePathfindingGrid newPathfindingGrid;

    private float timer;

    private void FixedUpdate()
    {
        timer -= Time.deltaTime;
    }

    private void Update()
    {
        if(timer < 0 && spawnCount > 0)
        {
            GameObject enemy = Instantiate(EnemyPrefab, SpawnPoint.transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyController>().Initialize(newTarget, newPathfindingGrid);
            timer = spawnTimer;
            spawnCount--;
        }
    }
}

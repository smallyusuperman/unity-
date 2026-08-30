using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Experiments/Wave Config")]
public class WaveConfig : ScriptableObject
{
    public int enemyCount;

    public GameObject enemyPrefab;

    public float spawnInterval;
}

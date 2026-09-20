using UnityEngine;

/// <summary>
/// 单个波次的共享配置资产：敌人数量、预制体与生成间隔。
/// 只保存设计期参数；运行期进度（已生成数量、计时）由 WaveSpawner 持有，不写回本资产。
/// </summary>
[CreateAssetMenu(fileName = "WaveConfig", menuName = "Experiments/Wave Config")]
public class WaveConfig : ScriptableObject
{
    public int enemyCount;

    public GameObject enemyPrefab;

    public float spawnInterval;
}

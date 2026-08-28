using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Min(0f)]
    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private EnemyStatsConfig enemyData;
    private float currentHealth;

    public float CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;

    private void Awake()
    {
        // 未绑定配置时沿用组件原有数值，保留旧 Prefab 的运行方式。
        if (enemyData != null)
        {
            maxHealth = enemyData.maxHealth;
        }
        maxHealth = Mathf.Max(0f, maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        damage = Mathf.Max(0f, damage);
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        Debug.Log($"Enemy Health: {currentHealth}/{maxHealth}");
        if (currentHealth == 0f)
        {
            Destroy(gameObject);
            Debug.Log("Enemy destroyed");
        }
    }
}

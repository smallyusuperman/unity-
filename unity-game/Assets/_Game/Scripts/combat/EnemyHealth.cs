using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Min(0f)]
    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private EnemyStatsConfig enemyData;
    private float currentHealth;

    /// <summary>当前血量；仅由 TakeDamage 修改，外部只读。</summary>
    public float CurrentHealth => currentHealth;

    /// <summary>血量上限；Awake 时若绑定了 EnemyStatsConfig，会用配置值覆盖 Inspector 值。</summary>
    public float MaxHealth => maxHealth;

    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        // 未绑定配置时沿用组件原有数值，保留旧 Prefab 的运行方式。
        if (enemyData != null)
        {
            maxHealth = enemyData.maxHealth;
        }
        maxHealth = Mathf.Max(0f, maxHealth);
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 唯一受伤入口（IDamageable 实现）。负伤害归零，结果钳制在 [0, maxHealth]。
    /// 血量归零时切换到 Dead 并销毁自身，因此调用方不得在调用后继续持有该对象。
    /// </summary>
    public void TakeDamage(float damage)
    {
        damage = Mathf.Max(0f, damage);
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        Debug.Log($"Enemy Health: {currentHealth}/{maxHealth}");
        if (currentHealth == 0f)
        {
            enemyController.ChangeState(EnemyState.Dead);
            Destroy(gameObject);
            Debug.Log("Enemy destroyed");
        }
    }
}

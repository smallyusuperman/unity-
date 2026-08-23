using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // maxHealth 是可配置上限；currentHealth 是每次运行时独立变化的状态。
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        // 所有伤害来源都经过同一入口：拒绝负伤害，并把生命值限制在合法范围。
        damage = Mathf.Max(0f, damage);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"Player Health: {currentHealth}/{maxHealth}");
    }
}

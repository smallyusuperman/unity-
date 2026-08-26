using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // maxHealth 是可配置上限；currentHealth 是每次运行时独立变化的状态。
    [Min(0f)][SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (currentHealth <= 0f)
        {
            currentHealth = 1f;
            TakeDamage(2f); // 确保在初始生命值为零时触发死亡逻辑
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f)
        {
            Debug.Log("Player is already dead. No further damage can be taken.");
            return;
        }
        // 所有伤害来源都经过同一入口：拒绝负伤害，并把生命值限制在合法范围。
        damage = Mathf.Max(0f, damage);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"Player Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Debug.Log("Player is dead");
            // 死亡后停止玩家主动控制；场景重载会恢复初始状态。
            PlayerController playerController = GetComponent<PlayerController>();
            PlayerAttack playerAttack = GetComponent<PlayerAttack>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            if (playerAttack != null){
                playerAttack.enabled = false;
            }
        }
    }
}

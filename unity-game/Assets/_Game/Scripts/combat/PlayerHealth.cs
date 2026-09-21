using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    /// <summary>
    /// 血量变化通知。事件不携带数据，订阅方自行读取 CurrentHealth / MaxHealth。
    /// 注意 Awake 初始化后也会发布一次，订阅方需容忍"早于自身 Start"的通知。
    /// </summary>
    public event EventHandler HealthChanged;

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

        HealthChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 唯一受伤入口（IDamageable 实现）。已经死亡时直接返回，不做任何结算。
    /// 负伤害归零，结果钳制在 [0, maxHealth]；只有伤害大于 0 才发布 HealthChanged。
    /// 血量归零时停用移动、近战与投射物组件，等待 R 键重载场景恢复。
    /// </summary>
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

        if (damage > 0f)
        {
            HealthChanged?.Invoke(this, EventArgs.Empty);
        }

        if (currentHealth <= 0f)
        {
            Debug.Log("Player is dead");
            // 死亡后停止玩家主动控制；场景重载会恢复初始状态。
            PlayerController playerController = GetComponent<PlayerController>();
            PlayerAttack playerAttack = GetComponent<PlayerAttack>();
            PlayerShoot playerShoot = GetComponent<PlayerShoot>();
            SlowSkill slowSkill = GetComponent<SlowSkill>();
            if (slowSkill != null)
            {                
                slowSkill.enabled = false;
            }
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            if (playerAttack != null)
            {
                playerAttack.enabled = false;
            }
            if (playerShoot != null)
            {
                playerShoot.enabled = false;
            }
        }
    }
}

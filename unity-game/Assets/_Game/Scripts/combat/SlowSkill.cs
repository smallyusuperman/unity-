using UnityEngine;
using UnityEngine.InputSystem;

public class SlowSkill : MonoBehaviour
{
    private float damage;
    private float attackRange;
    private float attackInterval;
    [SerializeField] private PlayerSkillConfig SlowSkillConfig;
    private float attackTimer;

    [SerializeField] private float slowDuration;
    [SerializeField] private float speedMultiper;

    private void Awake()
    {
        if (ValidateConfiguration())
        {
            damage = SlowSkillConfig.AttackDamage;
            attackRange = SlowSkillConfig.AttackRange;
            attackInterval = SlowSkillConfig.AttackInterval;
            attackTimer = 0f;
        }
        else
        {
            enabled = false;
        }
    }

    private bool ValidateConfiguration()
    {
        if (SlowSkillConfig == null)
        {
            Debug.LogError("SlowSkillConfig is not assigned.");
            return false;
        }

        if (!IsFinite(SlowSkillConfig.AttackDamage) || SlowSkillConfig.AttackDamage < 0
            || !IsFinite(SlowSkillConfig.AttackRange) || SlowSkillConfig.AttackRange <= 0
            || !IsFinite(SlowSkillConfig.AttackInterval) || SlowSkillConfig.AttackInterval <= 0
            || !IsFinite(slowDuration) || slowDuration <= 0
            || !IsFinite(speedMultiper) || speedMultiper <= 0 || speedMultiper > 1)
        {
            Debug.LogError("Invalid configuration values in SlowSkillConfig.");
            return false;
        }

        return true;
    }

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (Time.timeScale > 0f && attackTimer <= 0f && keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            Slow();
        }
    }

    private void FixedUpdate()
    {
        attackTimer = Mathf.Max(0f, attackTimer - Time.fixedDeltaTime);
    }

    private void Slow()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            EnemyHealth enemyHealth = hitColliders[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.CurrentHealth > 0f)
            {
                enemyHealth.TakeDamage(damage);
                if (enemyHealth.CurrentHealth > 0f && enemyHealth.TryGetComponent<SlowEffect>(out var effect))
                {
                    effect.SetSlowEffect(speedMultiper, slowDuration);
                }
            }
        }
        attackTimer = attackInterval;
    }
}

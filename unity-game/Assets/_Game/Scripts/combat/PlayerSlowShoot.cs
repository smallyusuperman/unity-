using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlowShoot : MonoBehaviour
{
    private float damage;
    private float attackRange;
    private float attackInterval;
    [SerializeField] private float maxDistance;
    [SerializeField] private float boltSpeed;
    [SerializeField] private float speedMultiper;
    [SerializeField] private float slowDuration;

    [SerializeField] private PlayerSkillConfig BoltConfig;
    

    private float attackTimer;
    private Vector3 direction;

    [SerializeField] private GameObject SlowBoltPrefab;

    private void Awake()
    {
        if (ValidateConfiguration())
        {
            damage = BoltConfig.AttackDamage;
            attackRange = BoltConfig.AttackRange;
            attackInterval = BoltConfig.AttackInterval;
            attackTimer = 0f;
        }
        else
        {
            enabled = false;
        }
    }

    private bool ValidateConfiguration()
    {
        if (BoltConfig == null)
        {
            Debug.LogError("BoltConfig is null");
            return false;
        }

        if (maxDistance <= 0)
        {
            Debug.LogError("maxDistance is less than or equal to 0");
            return false;
        }

        if (speedMultiper <= 0 || speedMultiper > 1)
        {
            Debug.LogError("speedMultiper is less than or equal to 0 or greater than 1");
            return false;
        }

        if (slowDuration <= 0)
        {
            Debug.LogError("slowDuration is less than or equal to 0");
            return false;            
        }

        if (BoltConfig.AttackDamage <= 0)
        {
            Debug.LogError("damage is less than or equal to 0");
            return false;
        }
        if (BoltConfig.AttackInterval <= 0)
        {
            Debug.LogError("attackInterval is less than or equal to 0");
            return false;
        }
        if (BoltConfig.AttackRange <= 0)
        {
            Debug.LogError("attackRange is less than or equal to 0");
            return false;
        }
        return true;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (attackTimer <= 0f && keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        attackTimer -= Time.fixedDeltaTime;
    }


    private void Shoot()
    {
        GameObject nearestEnemy = FindNearestEnemyInRange();
        if (nearestEnemy == null)
        {
            // 范围内没有存活敌人时不消耗冷却：空放不施加惩罚，按键可立即重试。
            return;
        }
        direction = (nearestEnemy.transform.position - transform.position).normalized;
        attackTimer = attackInterval;
        GameObject bolt = Instantiate(SlowBoltPrefab, transform.position, Quaternion.identity);
        bolt.GetComponent<SlowArrowAction>().Initialize(direction, boltSpeed, damage, maxDistance, speedMultiper, slowDuration);

    }

    // 在 attackRange 内挑选最近的"存活"敌人（CurrentHealth > 0，排除已死但尚未销毁的敌人）。
    // 距离相等时使用 <= 比较，因此会取后遇到的那一个；每次 Vector3.Distance 都会开平方，整体为 O(n) 扫描。
    // 注意 attackRange 是索敌半径，投射物实际飞行距离由 maxDistance 控制。
    private GameObject FindNearestEnemyInRange()
    {
        GameObject nearestEnemyInRange = null;
        float nearestDistance = float.PositiveInfinity; 
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            EnemyHealth enemyHealth = hitColliders[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.CurrentHealth > 0f)
            {
                if (nearestEnemyInRange == null)
                {
                    nearestEnemyInRange = hitColliders[i].gameObject;
                    nearestDistance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                }
                else
                {
                    float distanceToEnemy = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                    if (distanceToEnemy <= nearestDistance)
                    {
                        nearestEnemyInRange = hitColliders[i].gameObject;
                        nearestDistance = distanceToEnemy;
                    }
                }
            }
        }
        return nearestEnemyInRange;
    }

}

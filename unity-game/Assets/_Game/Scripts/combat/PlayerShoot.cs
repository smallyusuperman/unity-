using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    private float damage;
    private float attackRange;
    private float attackInterval;
    [SerializeField] private float maxDistance;
    [SerializeField] private float boltSpeed;

    [SerializeField] private PlayerSkillConfig BoltConfig;
    

    private float attackTimer;
    private Vector3 direction;

    [SerializeField] private GameObject BoltPrefab;

    private void Awake()
    {
        if (ValidateConfiguration())
        {
            damage = BoltConfig.AttackDamage;
            attackRange = BoltConfig.AttackRange;
            attackInterval = BoltConfig.AttackInterval;
            attackTimer = 0f;
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
        if (attackTimer <= 0f && keyboard != null && keyboard.qKey.wasPressedThisFrame)
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
            return;
        }
        direction = (nearestEnemy.transform.position - transform.position).normalized;
        attackTimer = attackInterval;
        GameObject bolt = Instantiate(BoltPrefab, transform.position, Quaternion.identity);
        bolt.GetComponent<ArrowAction>().Initialize(direction, boltSpeed, damage, maxDistance);

    }

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

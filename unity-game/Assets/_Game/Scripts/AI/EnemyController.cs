using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 场景实例可由 Inspector 配置目标；动态实例由 WaveSpawner 调用 Initialize 注入目标。
    [SerializeField] private Transform target;

    //[Min(0f)][SerializeField] private float moveSpeed = 2f;

    [SerializeField] private EnemyStatsConfig enemyData;

    private Rigidbody2D rb;

    private EnemyState currentState;
    public EnemyState CurrentState => currentState;

    private Vector2 direction;
    private Vector2 newPosition;

    private float IdleTimer = 0f;
    private float cooldownTimer;
    private float maxIdleTime;
    private float attackCooldown;

    private float chaseToIdleDistance;
    private float idleToChaseDistance;
    private float attackRange;

    private float idleMovespeed;
    private float chaseMovespeed;

    private void Awake()
    {   if (ValidateConfiguration()){
            rb = GetComponent<Rigidbody2D>();
            // if(enemyData != null)
            // {
            // moveSpeed = Mathf.Max(0f, enemyData.moveSpeed);
            // }

            chaseToIdleDistance = enemyData.ChaseToIdleDistance;
            idleToChaseDistance = enemyData.IdleToChaseDistance;
            attackRange = enemyData.AttackRange;

            idleMovespeed = enemyData.IdleMovespeed;
            chaseMovespeed = enemyData.ChaseMovespeed;

            maxIdleTime = enemyData.IdleTime;
            attackCooldown = enemyData.AttackCooldown;

        }
        else
        {
            Debug.LogError("EnemyController: Missing EnemyStatsConfig. Please assign it in the Inspector.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if ((target.position - transform.position).magnitude >= idleToChaseDistance)
        {
            direction = Random.insideUnitCircle.normalized;
            currentState = EnemyState.Idle;
        }
        else
        {
            currentState = EnemyState.Chase;
        }
    }

    public void Initialize(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogError(
                "EnemyController cannot initialize without a target Transform.",
                this);

            enabled = false;
            return;
        }

        target = newTarget;
        enabled = true;
    }

    private void FixedUpdate()
    {
        // 归一化方向，确保追踪速度不随目标距离或斜向分量变化。
        //Vector2 direction = (target.position - transform.position).normalized;
        //Vector2 newPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        //rb.MovePosition(newPosition);
        switch (currentState)
        {
            case EnemyState.Idle:
                if (IdleTimer >= maxIdleTime)
                {
                    IdleTimer = 0f;
                    direction = Random.insideUnitCircle.normalized;
                }
                IdleTimer += Time.fixedDeltaTime;
                newPosition = rb.position + direction * idleMovespeed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);

                cooldownTimer -= Time.fixedDeltaTime;

                if ((target.position - transform.position).magnitude < idleToChaseDistance)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
            case EnemyState.Chase:
                // 处理追踪状态
                direction = (target.position - transform.position).normalized;
                newPosition = rb.position + direction * chaseMovespeed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);

                cooldownTimer -= Time.fixedDeltaTime;

                if ((target.position - transform.position).magnitude >= chaseToIdleDistance)
                {
                    direction = Random.insideUnitCircle.normalized;
                    IdleTimer = 0f;
                    ChangeState(EnemyState.Idle);
                }

                if ((target.position - transform.position).magnitude < attackRange && cooldownTimer < 0f)
                {
                    ChangeState(EnemyState.Attack);
                }
                break;
            case EnemyState.Attack:
                // 处理攻击状态
                break;
            case EnemyState.Dead:
                // 处理死亡状态
                break;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if(currentState == EnemyState.Dead)
        {
            Debug.LogWarning("EnemyController: Attempted to change state from Dead. No state change will occur.", this);
            return;
        }
        currentState = newState;
    }

    public void ResetCooldown()
    {
        cooldownTimer = attackCooldown;
    }

    private bool ValidateConfiguration()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyController: Missing EnemyStatsConfig. Please assign it in the Inspector.", this);
            return false;
        }

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError("EnemyController: Missing Rigidbody2D component. Please ensure it is attached to the GameObject.", this);
            return false;
        }

        if (enemyData.ChaseToIdleDistance <= 0f || enemyData.IdleToChaseDistance <= 0f || enemyData.AttackRange <= 0f)
        {
            Debug.LogError("EnemyController: Invalid distance or range values in EnemyStatsConfig. Please ensure they are positive.", this);
            return false;
        }

        if (enemyData.AttackRange >= enemyData.ChaseToIdleDistance)
        {
            Debug.LogError("EnemyController: Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.AttackRange >= enemyData.IdleToChaseDistance)
        {
            Debug.LogError("EnemyController: Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.ChaseToIdleDistance <= enemyData.IdleToChaseDistance)
        {
            Debug.LogError("EnemyController: Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.IdleMovespeed >= enemyData.ChaseMovespeed)
        {
            Debug.LogError("EnemyController: Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.IdleMovespeed <= 0f || enemyData.ChaseMovespeed <= 0f)
        {
            Debug.LogError("EnemyController: Invalid speed values in EnemyStatsConfig. Please ensure IdleMovespeed and ChaseMovespeed are positive.", this);
            return false;
        }

        if (enemyData.IdleTime < 0f || enemyData.AttackCooldown < 0f)
        {
            Debug.LogError("EnemyController: Invalid time values in EnemyStatsConfig. Please ensure IdleTime and AttackCooldown are non-negative.", this);
            return false;
        }

        return true;
    }
}

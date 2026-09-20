using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{

    [SerializeField] bool debugMode = true;
    // 场景实例可由 Inspector 配置目标；动态实例由 WaveSpawner 调用 Initialize 注入目标。
    [SerializeField] private Transform target;

    [SerializeField] private EnemyStatsConfig enemyData;

    private Rigidbody2D rb;

    private EnemyState previousState;
    /// <summary>上一次 ChangeState 之前的状态，供调试显示与转移日志使用。</summary>
    public EnemyState PreviousState => previousState;
    private EnemyState currentState;
    /// <summary>当前状态；只允许由 ChangeState 修改。</summary>
    public EnemyState CurrentState => currentState;

    private Vector2 direction;
    private Vector2 newPosition;

    private float IdleTimer = 0f;
    // 攻击冷却：只在 Idle 与 Chase 分支递减；Attack 结算后由 ResetCooldown 重新装填。
    private float cooldownTimer;
    private float maxIdleTime;
    private float attackCooldown;

    [Min(0.1f)]
    [SerializeField] private float repathInterval = 0.5f;

    private float repathTimer;
    private Vector2Int lastTargetCell;
    private bool hasTargetCell;

    private float chaseToIdleDistance;
    private float idleToChaseDistance;
    private float attackRange;

    private float idleMovespeed;
    private float chaseMovespeed;

    /// <summary>一条 A* 路径及当前消费到的索引；通过 PathChanged 事件按引用传出。</summary>
    public class Waypoint
    {
        public List<Vector2> Path;
        public int CurrentIndex;
    }
    private Waypoint waypoint;
    public Waypoint WaypointReadOnly => waypoint;

    private ScenePathfindingGrid pathfindingGrid;

    public event EventHandler<Waypoint> PathChanged;

    private void Awake()
    {
        if (ValidateConfiguration())
        {
            rb = GetComponent<Rigidbody2D>();

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
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Missing EnemyStatsConfig. Please assign it in the Inspector.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if ((target.position - transform.position).magnitude >= idleToChaseDistance)
        {
            direction = Random.insideUnitCircle.normalized;
            currentState = EnemyState.Idle;
            if (debugMode)
                Debug.Log($"{name}{GetInstanceID()} | {Time.frameCount} | Starting in Idle state.", this);
        }
        else
        {
            if (debugMode)
                Debug.Log($"{name}{GetInstanceID()} | {Time.frameCount} | Starting in Chase state.", this);
            currentState = EnemyState.Chase;
        }

        waypoint = new Waypoint
        {
            Path = new List<Vector2>(),
            CurrentIndex = 0
        };
        if (currentState == EnemyState.Chase)
        {
            RefreshPath();
        }
    }

    private void RefreshPath()
    {
        lastTargetCell = pathfindingGrid.WorldToCell(target.position);
        hasTargetCell = true;

        if (pathfindingGrid.TryGetWaypoints(
                rb.position,
                target.position,
                out List<Vector2> newPath))
        {
            waypoint.Path = newPath;
        }
        else
        {
            waypoint.Path.Clear();
        }

        waypoint.CurrentIndex = 0;
        repathTimer = repathInterval;

        NotifyPathChanged();
    }

    /// <summary>
    /// 由 WaveSpawner 在 Instantiate 之后立即调用，注入两个运行期依赖。
    /// 必须在 Start 之前完成：Start 会读取 target 位置、必要时调用 RefreshPath，
    /// 而 pathfindingGrid 是纯私有字段，无法通过 Inspector 赋值。
    /// </summary>
    public void Initialize(
        Transform newTarget,
        ScenePathfindingGrid newPathfindingGrid)
    {
        if (newTarget == null)
        {
            Debug.LogError(
                "EnemyController cannot initialize without a target Transform.",
                this);

            enabled = false;
            return;
        }

        if (newPathfindingGrid == null)
        {
            Debug.LogError(
                "EnemyController cannot initialize without a ScenePathfindingGrid.",
                this);

            enabled = false;
            return;
        }

        target = newTarget;
        pathfindingGrid = newPathfindingGrid;
        enabled = true;
    }

    private void FixedUpdate()
    {
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
                repathTimer -= Time.fixedDeltaTime;

                bool hasUsablePath =
                    waypoint != null
                    && waypoint.Path != null
                    && waypoint.Path.Count > 0
                    && waypoint.CurrentIndex >= 0
                    && waypoint.CurrentIndex < waypoint.Path.Count;

                Vector2Int currentTargetCell =
                    pathfindingGrid.WorldToCell(target.position);

                bool targetCellChanged =
                    !hasTargetCell || currentTargetCell != lastTargetCell;

                bool retryMissingPath =
                    !hasUsablePath && repathTimer <= 0f;

                if (targetCellChanged || retryMissingPath)
                {
                    RefreshPath();

                    hasUsablePath =
                        waypoint.Path != null
                        && waypoint.Path.Count > 0
                        && waypoint.CurrentIndex >= 0
                        && waypoint.CurrentIndex < waypoint.Path.Count;
                }

                if (hasUsablePath)
                {
                    Vector2 waypointPosition =
                        waypoint.Path[waypoint.CurrentIndex];

                    direction =
                        (waypointPosition - rb.position).normalized;

                    newPosition =
                        rb.position
                        + direction
                        * chaseMovespeed
                        * Time.fixedDeltaTime;

                    rb.MovePosition(newPosition);

                    if (Vector2.Distance(
                            rb.position,
                            waypointPosition) < 0.1f)
                    {
                        if (waypoint.CurrentIndex < waypoint.Path.Count - 1)
                        {
                            waypoint.CurrentIndex++;
                            NotifyPathChanged();
                        }
                    }
                }

                cooldownTimer -= Time.fixedDeltaTime;

                if ((target.position - transform.position).magnitude
                    >= chaseToIdleDistance)
                {
                    direction = Random.insideUnitCircle.normalized;
                    IdleTimer = 0f;
                    ChangeState(EnemyState.Idle);
                }

                if ((target.position - transform.position).magnitude
                    < attackRange
                    && cooldownTimer < 0f)
                {
                    ChangeState(EnemyState.Attack);
                }

                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Dead:
                break;
        }
    }

    /// <summary>
    /// 切换敌人状态，三条不变量：
    /// 1. 目标状态与当前状态相同则直接返回；
    /// 2. Dead 是终态，从 Dead 出发的切换会被拒绝并记录警告；
    /// 3. 进入 Chase 会自动重算路径，其它状态只发布一次 PathChanged 通知。
    /// </summary>
    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        if (currentState == EnemyState.Dead)
        {
            Debug.LogWarning($"{name}{GetInstanceID()} | {Time.frameCount} | Attempted to change state from Dead. No state change will occur.", this);
            return;
        }

        previousState = currentState;
        currentState = newState;

        if (currentState == EnemyState.Chase)
        {
            RefreshPath();
        }
        else
        {
            NotifyPathChanged();
        }
        if (debugMode)
        {
            Debug.Log($"{name}{GetInstanceID()} | {Time.frameCount} | {previousState} -> {currentState}.", this);
        }
    }

    private void NotifyPathChanged()
    {
        PathChanged?.Invoke(this, WaypointReadOnly);
    }

    /// <summary>攻击结算完成后重新装填冷却；当前调用方为 EnemyAttack。</summary>
    public void ResetCooldown()
    {
        cooldownTimer = attackCooldown;
    }

    private bool ValidateConfiguration()
    {
        if (enemyData == null)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Missing EnemyStatsConfig. Please assign it in the Inspector.", this);
            return false;
        }

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Missing Rigidbody2D component. Please ensure it is attached to the GameObject.", this);
            return false;
        }

        if (enemyData.ChaseToIdleDistance <= 0f || enemyData.IdleToChaseDistance <= 0f || enemyData.AttackRange <= 0f)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Invalid distance or range values in EnemyStatsConfig. Please ensure they are positive.", this);
            return false;
        }

        if (enemyData.AttackRange >= enemyData.ChaseToIdleDistance)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.AttackRange >= enemyData.IdleToChaseDistance)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.ChaseToIdleDistance <= enemyData.IdleToChaseDistance)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.IdleMovespeed >= enemyData.ChaseMovespeed)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Invalid speed or range configuration in EnemyStatsConfig. Please ensure IdleMovespeed < ChaseMovespeed and AttackRange < ChaseToIdleDistance.", this);
            return false;
        }

        if (enemyData.IdleMovespeed <= 0f || enemyData.ChaseMovespeed <= 0f)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Invalid speed values in EnemyStatsConfig. Please ensure IdleMovespeed and ChaseMovespeed are positive.", this);
            return false;
        }

        if (enemyData.IdleTime < 0f || enemyData.AttackCooldown < 0f)
        {
            Debug.LogError($"{name}{GetInstanceID()} | {Time.frameCount} | Invalid time values in EnemyStatsConfig. Please ensure IdleTime and AttackCooldown are non-negative.", this);
            return false;
        }

        return true;
    }
}

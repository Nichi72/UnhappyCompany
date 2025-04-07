using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyAIController는 적의 기본 AI 동작을 정의하는 추상 클래스입니다.
/// </summary>
public abstract class EnemyAIController : MonoBehaviour, IDamageable, IDamager
{
    [SerializeField] protected IState currentState; // 현재 활성화된 상태
    protected UtilityCalculator utilityCalculator; // 유틸리티 계산기
    public NavMeshAgent agent; // NavMeshAgent 컴포넌트 참조
    public Transform player; // 플레이어의 Transform 참조
    
    [Header("AI Settings")]
    [SerializeField] protected BaseEnemyAIData enemyData;
    
    public virtual BaseEnemyAIData EnemyData => enemyData;
    
    // 공통 프로퍼티
    public float PatrolRadius => EnemyData.patrolRadius;
    public float ChaseRadius => EnemyData.chaseRadius;
    public float AttackRadius => EnemyData.attackRadius;

    [Header("AI GizmoRange Settings")]
    public Color patrolGizmoRangeColor = Color.green;
    public Color chaseGizmoRangeColor = Color.yellow;
    public Color attackGizmoRangeColor = Color.red;

    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public UtilityCalculator UtilityCalculator { get => utilityCalculator; set => utilityCalculator = value; }
    private int _hp = 0;
    public int hp { get => _hp; set => _hp = value; }

    [Header("Debug Settings")]
    public bool enableDebugUI = true;
    public GameObject debugUIPrefab;  // Inspector에서 디버그 UI 프리팹 할당
    [SerializeField] protected EnemyStateDebugUI debugUI;
    public EnemyBudgetFlag budgetFlag;

    protected virtual void Start()
    {
        InitializeAI();
    }

    protected virtual void InitializeAI()
    {
        utilityCalculator = new UtilityCalculator();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        TimeManager.instance.OnTimeOfDayChanged += HandleTimeOfDayChanged;
        CurrentTimeOfDay = TimeManager.instance.CurrentTimeOfDay;

        if (enableDebugUI)
        {
            InitializeDebugUI();
        }
    }

    protected virtual void InitializeDebugUI()
    {
        if (debugUIPrefab != null)
        {
            var debugUIObject = Instantiate(debugUIPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            debugUI = debugUIObject.GetComponent<EnemyStateDebugUI>();
            if (debugUI != null)
            {
                debugUI.Initialize(transform, patrolGizmoRangeColor, chaseGizmoRangeColor, attackGizmoRangeColor);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (TimeManager.instance != null)
        {
            TimeManager.instance.OnTimeOfDayChanged -= HandleTimeOfDayChanged;
        }

        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.activeEnemies.Remove(gameObject);
        }

        // 디버그 UI 정리
        if (debugUI != null)
        {
            Destroy(debugUI.gameObject);
        }
    }

    protected virtual void HandleTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        CurrentTimeOfDay = newTimeOfDay;
    }

    protected virtual void HandleExecute()
    {
        TimeOfDay currentTimeOfDay = TimeManager.instance.CurrentTimeOfDay;
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Morning:
                currentState?.ExecuteMorning();
                break;
            case TimeOfDay.Afternoon:
                currentState?.ExecuteAfternoon();
                break;
        }
    }

    protected virtual void Update()
    {
        HandleExecute();

        // 디버그 UI 업데이트
        if (debugUI != null)
        {
            debugUI.UpdateState(currentState.GetType().Name.Replace("State", ""));
        }
    }

    protected virtual void FixedUpdate()
    {
        TimeOfDay currentTimeOfDay = TimeManager.instance.CurrentTimeOfDay;
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Morning:
                currentState?.ExecuteFixedMorning();
                break;
            case TimeOfDay.Afternoon:
                currentState?.ExecuteFixedAfternoon();
                break;
        }
    }

    public virtual void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        MyUtility.UtilityGizmos.DrawCircle(transform.position, PatrolRadius, patrolGizmoRangeColor);
        MyUtility.UtilityGizmos.DrawCircle(transform.position, ChaseRadius, chaseGizmoRangeColor);
        MyUtility.UtilityGizmos.DrawCircle(transform.position, AttackRadius, attackGizmoRangeColor);
    }

    public virtual void TakeDamage(int damage, DamageType damageType)
    {
        hp -= damage;
        if(hp <= 0)
        {
            EnemyManager.instance.activeEnemies.Remove(gameObject);
            Debug.Log($"{gameObject.name} 사망");
            Destroy(gameObject);
        }
    }

    public virtual void DealDamage(int damage, IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Physical);
    }

    public virtual void AttackCenter()
    {
        Debug.Log($"{gameObject.name} 센터 공격");
    }
}

/// <summary>
/// EnemyAIController<T>는 EnemyAIController를 상속받아 특정 타입의 AIData를 사용하는 컨트롤러입니다.
/// </summary>
public abstract class EnemyAIController<T> : EnemyAIController where T : BaseEnemyAIData
{
    [SerializeField] protected new T enemyData;
    
    public override BaseEnemyAIData EnemyData => enemyData;
}
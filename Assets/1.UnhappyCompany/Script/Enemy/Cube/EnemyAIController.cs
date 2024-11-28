using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyAIController는 적의 현재 상태를 관리하고 상태 간 전환을 담당합니다.
/// NavMeshAgent를 사용하여 이동을 제어하고 플레이어를 추적합니다.
/// </summary>
public class EnemyAIController : MonoBehaviour
{
    private IState currentState; // 현재 활성화된 상태
    private UtilityCalculator utilityCalculator; // 유틸리티 계산기
    public NavMeshAgent agent; // NavMeshAgent 컴포넌트 참조
    public Transform player; // 플레이어의 Transform 참조

    // 추가된 변수: 행동 반경 설정
    [Header("AI Behavior Settings")]
    public float patrolRadius = 10f; // 순찰 반경
    public float chaseRadius = 15f;  // 추적 반경
    public float attackRadius = 2f;  // 공격 반경

    /// <summary>
    /// 초기화 메서드. 시작 시 순찰 상태로 전환됩니다.
    /// </summary>
    void Start()
    {
        utilityCalculator = new UtilityCalculator(); // 유틸리티 계산기 인스턴스 생성
        agent = GetComponent<NavMeshAgent>(); // NavMeshAgent 컴포넌트 가져오기
        player = GameObject.FindGameObjectWithTag("Player").transform; // "Player" 태그를 가진 오브젝트의 Transform 가져오기
        ChangeState(new PatrolState(this, utilityCalculator)); // 초기 상태를 PatrolState로 설정
    }

    /// <summary>
    /// 매 프레임마다 현재 상태의 Execute 메서드를 호출합니다.
    /// </summary>
    void Update()
    {
        if (currentState != null)
        {
            currentState.Execute(); // 현재 상태의 Execute 메서드 호출
        }
    }

    /// <summary>
    /// 상태 전환을 처리하는 메서드. 현재 상태를 종료하고 새로운 상태로 전환합니다.
    /// </summary>
    /// <param name="newState">전환할 새로운 상태</param>
    public void ChangeState(IState newState)
    {
        if(currentState != null)
        {
            currentState.Exit(); // 현재 상태의 Exit 메서드 호출
        }
        currentState = newState; // 새로운 상태로 변경
        currentState.Enter(); // 새로운 상태의 Enter 메서드 호출
    }

    /// <summary>
    /// Gizmos를 그려 AI의 행동 범위를 시각화합니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 순찰 반경 - 녹색
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // 추적 반경 - 노란색
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        // 공격 반경 - 빨간색
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
} 
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AttackState는 적이 플레이어를 공격하는 상태를 나타냅니다.
/// 공격 쿨타임과 시전시간을 관리하며, 공격 범위를 벗어나면 상태를 전환합니다.
/// </summary>
public class CubeAttackState : IState
{
    private Rigidbody rigidbody;
    private EnemyAICube controller;
    private UtilityCalculator utilityCalculator;
    private float lastAttackTime;
    private float attackCastingStartTime; // 공격 시전 시작 시간
    private bool isAttackCasting; // 공격 시전 중인지 여부


    public CubeAttackState(EnemyAICube controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        this.rigidbody = controller.GetComponent<Rigidbody>();
    }

    public void Enter()
    {
        // DisableAgentSafely();
        lastAttackTime = Time.time;
        isAttackCasting = false;
    }

    public void Exit()
    {
        // EnableAgentSafely();
        isAttackCasting = false;
    }

    public void ExecuteMorning()
    {
        // 빈 메서드
    }

    public void ExecuteAfternoon() 
    {
        // 빈 메서드
    }
    

    public void ExecuteFixedMorning()
    {
        ExecuteFixedUpdate();
        
    }

    public void ExecuteFixedAfternoon()
    {
        ExecuteFixedUpdate();
    }

    private void ExecuteFixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        // 플레이어를 향해 회전
       
        // 공격 범위 벗어남
        if (controller.AttackRadius  <= distanceToPlayer)
        {
            rigidbody.isKinematic = true;
            // EnableAgentSafely();
            controller.ChangeState(new CubePatrolState(controller, utilityCalculator , controller.pathCalculator));
            return;
        }

        if (!isAttackCasting && Time.time - lastAttackTime >= controller.AttackCooldown)
        {
            isAttackCasting = true;
            attackCastingStartTime = Time.time;
            ChangeMaterialColor(Color.red);
            // 시전하면서 플레이어 쳐다보기
            Vector3 directionToPlayer = (controller.player.position - controller.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);

            Debug.Log("공격 시전 시작!");
        }

        if (isAttackCasting && Time.time - attackCastingStartTime >= controller.AttackCastingTime)
        {
            ResetMaterialColor();
            Vector3 directionToPlayer = (controller.player.position - controller.transform.position).normalized;
            directionToPlayer += Vector3.up * 0.2f;
            directionToPlayer.Normalize();
            Debug.DrawRay(controller.transform.position, directionToPlayer * controller.RushForce, Color.red, 1f);

            if (rigidbody != null)
            {
                rigidbody.isKinematic = false;
                rigidbody.AddForce(directionToPlayer * controller.RushForce, ForceMode.Impulse);
                var random = Random.value < 0.5f ? -0.4f : 0.4f;
                rigidbody.AddTorque((Vector3.up + Vector3.right * random) * controller.TorqueForce * 10f, ForceMode.Impulse);
            }

            lastAttackTime = Time.time;
            isAttackCasting = false;
            Debug.Log("공격 실행!");
        }
    }
    // private void DisableAgentSafely()
    // {
    //     if (controller.agent != null && controller.agent.enabled)
    //     {
    //         controller.agent.isStopped = true; // NavMeshAgent의 이동을 중지
    //         controller.agent.enabled = false;  // NavMeshAgent 비활성화
    //         Debug.Log("NavMeshAgent가 안전하게 비활성화되었습니다.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("NavMeshAgent가 이미 비활성화되어 있거나 존재하지 않습니다.");
    //     }
    // }
    // private void EnableAgentSafely()
    // {
    //     if (controller.agent != null && !controller.agent.enabled)
    //     {
    //         controller.agent.enabled = true;   // NavMeshAgent 활성화
    //         controller.agent.isStopped = false; // NavMeshAgent의 이동 재개
    //         Debug.Log("NavMeshAgent가 안전하게 활성화되었습니다.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("NavMeshAgent가 이미 활성화되어 있거나 존재하지 않습니다.");
    //     }
    // }
    private Material originalMaterial;
    private Color originalColor;

    private void ChangeMaterialColor(Color newColor)
    {
        if (controller.GetComponent<Renderer>() != null)
        {
            Material material = controller.GetComponent<Renderer>().material;
            if (originalMaterial == null)
            {
                originalMaterial = material;
                originalColor = material.color;
            }
            material.color = newColor;
            Debug.Log("메테리얼 색상이 변경되었습니다.");
        }
        else
        {
            Debug.LogWarning("Renderer 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void ResetMaterialColor()
    {
        if (originalMaterial != null && controller.GetComponent<Renderer>() != null)
        {
            controller.GetComponent<Renderer>().material.color = originalColor;
            Debug.Log("메테리얼 색상이 초기화되었습니다.");
        }
        else
        {
            Debug.LogWarning("원본 메테리얼이 없거나 Renderer 컴포넌트를 찾을 수 없습니다.");
        }
    }
} 
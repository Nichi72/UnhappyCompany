using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class RampageChargeState : IState
{
    private RampageAIController controller;
    private Rigidbody rb;
    
    private float waitBeforeCharge = 1f; // 돌진 전 대기 시간
    private float chargeSpeed;

    private Coroutine chargeCoroutine;


    public RampageChargeState(RampageAIController controller,string beforeState)
    {
        Debug.Log($"{beforeState} 상태에서 돌진 시작");
        this.controller = controller;
        this.rb = controller.GetComponent<Rigidbody>();
        chargeSpeed = controller.enemyData.rushSpeed;
        
    }
    public void Enter()
    {
        Debug.Log("Rampage: Charge 상태 시작");
        controller.agent.enabled = false;
        rb.isKinematic = true;

        // TODO: 돌진 준비 애니메이션 재생
        AudioManager.instance.PlayOneShot(FMODEvents.instance.TEST, controller.transform);
        chargeCoroutine = controller.StartCoroutine(ChargeCoroutine());
    }

    public void ExecuteMorning()
    {
        // 오브젝트의 회전값의 x와 z축을 0으로 고정
        Vector3 eulerRotation = controller.transform.rotation.eulerAngles;
        eulerRotation.x = 0;
        eulerRotation.z = 0;
        controller.transform.rotation = Quaternion.Euler(eulerRotation);
    }

    public void ExecuteAfternoon()
    {
    }

    public void Exit()
    {
        Debug.Log("Rampage: Charge 상태 종료");
        // TODO: 돌진 종료 시 애니메이션, 사운드 정리 등
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }


    IEnumerator ChargeCoroutine()
    {
        Vector3 initialDirection = Vector3.zero;
        Vector3 playerDirection = Vector3.zero;
        bool isPlayerInRange = true;
        float elapsedTime = 0f;

        // 돌진하기 전 회전해서 플레이어 방향으로 회전
        while(true)
        {
            // 3초가 지나면 루프를 빠져나옴
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitBeforeCharge)
            {
                Debug.Log("3초가 지나 루프를 빠져나갑니다.");
                controller.onceReduceHP = true;
                break;
            }

            initialDirection = (controller.player.position - controller.transform.position).normalized;
            initialDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(initialDirection);
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, Time.deltaTime * 5f);
            Debug.Log("플레이어를 향해 회전중 direction " + initialDirection);
            yield return null;
        }
        // 돌진
        Vector3 playerPosition = GameManager.instance.currentPlayer.transform.position;
        rb.isKinematic = true;
        controller.agent.enabled = true;
        Debug.Log("playerPosition: " + playerPosition);
        while(true)
        {
            // NavMeshAgent를 사용하여 플레이어를 따라감
            controller.agent.SetDestination(playerPosition);

            float distance = Vector3.Distance(controller.transform.position, playerPosition);
            Debug.Log($"Vector3.Distance(controller.transform.position, playerPosition): {distance} <= {controller.enemyData.attackRadius}" );
            if (distance <= controller.enemyData.attackRadius)
            {
                if(isPlayerInRange)
                {
                    isPlayerInRange = false;
                    playerDirection = (playerPosition - controller.transform.position).normalized;
                    Debug.Log($"남은 경로가 하나 남았습니다. playerDirection {playerDirection}");
                    yield return null;
                }
                
                // 플레이어 위치에 도달했는지 확인
                if (controller.agent.remainingDistance <= controller.agent.stoppingDistance)
                {
                    Debug.Log("플레이어 위치에 도달했습니다.");
                    yield return null;
                    break;
                }
            }
            yield return null;
        }

        rb.isKinematic = false;
        controller.agent.enabled = false;

        while(true)
        {
            if(controller.isCollided)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                
                Debug.Log("돌진 충돌로 종료");
                break;
            }
            Debug.Log("플레이어 위치에 도달 해당 방향으로 돌진중");
            rb.linearVelocity = playerDirection * chargeSpeed;
            yield return null;
        }

        Debug.Log("돌진 충돌로 종료후 처리");
        // controller.ChangeState(new RampageIdleState(controller,"ChargeState"));
        if(controller.chargeCount > 0)
        {
            controller.ChangeState(new RampagePanelOpenState(controller,1,"ChargeState"));
        }
        else
        {
            controller.chargeCount = controller.enemyData.maxChargeCount;
            controller.ChangeState(new  RampageIdleState(controller,"ChargeState(연속 돌진)"));
        }
        
        controller.StopCoroutine(chargeCoroutine);
    }

    // 원을 그리는 헬퍼 메서드
    private void DrawCircle(Vector3 center, float radius, Color color, int segments = 20)
    {
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0f, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0f, Mathf.Sin(angle2) * radius);

            Debug.DrawLine(point1, point2, color, 0.1f);
        }
    }

    public bool CheckPlayerInPatrolRange()
    {
        if (controller.player == null) return false;

        Vector3 toPlayer = controller.player.position - controller.transform.position;
        float distance = toPlayer.magnitude;

        return distance <= controller.enemyData.patrolRadius;
    }
}

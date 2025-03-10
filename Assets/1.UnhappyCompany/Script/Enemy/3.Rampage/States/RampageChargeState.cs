using UnityEngine;

public class RampageChargeState : IState
{
    private RampageAIController controller;
    private Rigidbody rb;
    
    private float chargeStartTime;
    private float waitBeforeCharge = 1f; // 돌진 전 대기 시간
    private bool isWaiting = true;
    private float chargeSpeed;

    public RampageChargeState(RampageAIController controller)
    {
        this.controller = controller;
        this.rb = controller.GetComponent<Rigidbody>();
        chargeSpeed = controller.enemyData.rushSpeed;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Charge 상태 시작");
        chargeStartTime = Time.time;
        isWaiting = true;
        controller.agent.enabled = false;
        // TODO: 돌진 준비 애니메이션 재생
        AudioManager.instance.PlayOneShot(FMODEvents.instance.TEST, controller.transform);
    }

    public void ExecuteMorning()
    {
        UpdateCharge();
    }

    public void ExecuteAfternoon()
    {
        UpdateCharge();
    }

    public void Exit()
    {
        Debug.Log("Rampage: Charge 상태 종료");
        // TODO: 돌진 종료 시 애니메이션, 사운드 정리 등
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void UpdateCharge()
    {
        if (controller.isCollided) return;

        // 대기 시간이 지나면 돌진 시작
        if (isWaiting && Time.time - chargeStartTime >= waitBeforeCharge)
        {
            isWaiting = false;
            rb.isKinematic = false;
            controller.agent.enabled = false;
            // 플레이어 방향으로 힘을 가하여 돌진
            Vector3 direction = (controller.player.position - controller.transform.position).normalized;
            
            // direction.y = 0;
            // 플레이어 방향으로 회전
            controller.transform.rotation = Quaternion.LookRotation(direction);
            
            // 디버그 시각화
            Debug.DrawRay(controller.transform.position, direction * 5, Color.red, 2f);
            // Debug.Break();
            
            rb.linearVelocity = direction * chargeSpeed;

            // TODO: 돌진 애니메이션 재생
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
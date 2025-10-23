using System.Collections;
using UnityEngine;

public class RSPHoldingState : IState
{
    public float jumpSpeed = 10f;
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;
    private float holdingDuration = 5f; // 홀딩 지속 시간
    private float holdingStartTime;
    private Player player;
    private RSPSystem rspSystem;
    private bool isMoveToJumpsquare = false;
    private bool isHolding = false;
    private bool isProcessing = false;
    
    // 슬롯머신 UI 위치 - Scene에 추가 필요
    private Transform slotMachineUIPosition;

    private Vector3 jumpsquarePosition = new Vector3(0,-2.1099999f,0.889999986f);

    public RSPHoldingState(EnemyAIRSP controller, UtilityCalculator calculator, Player player, RSPSystem rspSystem)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        this.player = player;
        this.rspSystem = rspSystem;
        
        // Scene에서 슬롯머신과 SlotMachineUIPosition 참조 가져오기
        SetupSlotMachineComponents();
    }
    
    /// <summary>
    /// 슬롯머신 컴포넌트 설정
    /// </summary>
    private void SetupSlotMachineComponents()
    {
        slotMachineUIPosition = controller.slotMachineUIPosition;
        
        if (slotMachineUIPosition == null)
        {
            Debug.LogWarning("RSP: 슬롯머신 UI 위치 참조가 없습니다! 기본 위치가 사용됩니다.");
        }
    }

    public void Enter()
    {
        Debug.Log("RSP: 홀딩 상태 시작");
        holdingStartTime = Time.time;
        RSPHolding();
        isHolding = true;
        player.firstPersonController.LookAtWithQuaternion();
        player.firstPersonController._input.FreezePlayerInput(true);
    
    }

    public void ExecuteMorning()
    {
        // Debug.Log("RSP: 오전 홀딩 중");
        // CheckHoldingCompletion();
    }

    public void ExecuteAfternoon()
    {
        // Debug.Log("RSP: 오후 홀딩 중");
        // CheckHoldingCompletion();
    }

    private void RSPHolding()
    {
        isHolding = true;
        controller.StartCoroutine(MoveToJumpsquare());
    }

    public void Exit()
    {
        Debug.Log("RSP: 홀딩 상태 종료");
        controller.rspSystem.rspUI.GameEndRSPAnimation();
        
        player.firstPersonController._input.FreezePlayerInput(false);
    }

    bool isflag = false;
    public void ExecuteFixedMorning()
    {
        if(isMoveToJumpsquare == false)
        {
            return;
        }

        if(isflag == false)
        {
            isflag = true;
        }
    }

    public void ExecuteFixedAfternoon()
    {
        if(isMoveToJumpsquare == false)
        {
            return;
        }

        if(isflag == false)
        {
            isflag = true;
            
        }
    }

    
    IEnumerator MoveToJumpsquare()
    {
        Debug.Log("RSP: 점프스퀘어로 점프");
        EnableCompoenet(false);
        controller.PlayAnimation(controller.HoldingAnimationName, 0f);

        Vector3 jumpsquarePosition = player.OffsetLists[0].position;
        while (Vector3.Distance(controller.transform.position, jumpsquarePosition) > 0.1f)
        {
            // 플레이어를 바라보면서 이동
            Vector3 direction = (jumpsquarePosition - controller.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f);

            controller.transform.position = Vector3.MoveTowards(controller.transform.position, jumpsquarePosition, Time.deltaTime * jumpSpeed);
            yield return null;
        }
        controller.transform.parent = player.OffsetLists[0];
        controller.transform.localPosition = new Vector3(0,0,0);
        controller.transform.localRotation = Quaternion.Euler(0,0,0);

        Debug.Log("RSP: 점프스퀘어에 도착");

        yield return new WaitForSeconds(1f);
        
        // compulsoryPlayStack이 0이 될 때까지 계속 진행
        while (controller.GetCompulsoryPlayStack() > 0)
        {
            Debug.Log($"RSP: 남은 스택 수: {controller.GetCompulsoryPlayStack()}");
            
            // 게임 진행 중 플래그 초기화
            gameInProgress = true;
            
            // RSP+메달 게임 시작
            rspSystem.StartRSPWithMedalGame(OnRSPMedalGameComplete);
            
            // 게임이 완료될 때까지 대기
            yield return new WaitUntil(() => !gameInProgress);
            
            // 짧은 대기 시간 추가
            yield return new WaitForSeconds(1f);
        }
        
        Debug.Log("RSP: 모든 스택 클리어 완료!");

        // 1. 부모 해제 전에 지면 위치 찾기
        Vector3 targetPosition = controller.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(controller.transform.position + Vector3.up * 2f, Vector3.down, out hit, 10f, LayerMask.GetMask("Ground")))
        {
            targetPosition = hit.point;
            Debug.Log($"RSP: 지면 발견 위치 = {targetPosition}");
        }
        else
        {
            Debug.LogWarning("RSP: 지면을 찾지 못했습니다. 플레이어 주변 위치로 이동합니다.");
            targetPosition = player.transform.position + player.transform.forward * 2f;
        }
        
        // 2. 부모 해제 및 위치 설정
        controller.transform.parent = null;
        controller.transform.position = targetPosition;
        
        // 3. 애니메이터 먼저 활성화
        controller.animator.enabled = true;
        
        // 4. NavMeshAgent와 Collider 활성화
        EnableCompoenet(true);
        
        // 5. NavMeshAgent를 NavMesh 위로 강제 이동 (Warp)
        if (controller.agent.enabled)
        {
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                controller.agent.Warp(navHit.position);
                Debug.Log($"RSP: NavMesh 위치로 Warp 완료 = {navHit.position}");
            }
            else
            {
                Debug.LogError("RSP: NavMesh를 찾을 수 없습니다!");
            }
        }
        
        // 6. 지면 확인 대기 (최대 2초)
        float waitStartTime = Time.time;
        yield return new WaitUntil(() => controller.isGround || Time.time - waitStartTime > 2f);
        
        if (controller.isGround)
        {
            Debug.Log("RSP: 지면에 정상적으로 도착했습니다.");
        }
        else
        {
            Debug.LogWarning("RSP: 지면 도착 대기 시간 초과. 강제로 상태 전환합니다.");
        }
        
        // 7. 순찰 상태로 전환
        controller.ChangeState(new RSPPatrolState(controller));
    }
    
    /// <summary>
    /// 슬롯머신 게임이 진행 중인지 확인
    /// </summary>
    private bool IsSlotMachineGameInProgress()
    {
        // 간단한 플래그 변수로 대체 (OnGameComplete 이벤트에서 설정)
        return gameInProgress;
    }
    
    // 게임 진행 중 여부를 트래킹하는 플래그
    public bool gameInProgress = true;
    
    private void EnableCompoenet(bool isEnable)
    {
        controller.agent.enabled = isEnable;
        controller.GetComponent<Collider>().enabled = isEnable;
    }

    // 메달 게임이 끝나면 처리할 콜백
    private void OnRSPMedalGameComplete(RSPResult rspResult, int medalResult)
    {
        Debug.Log($"RSP+메달 게임 최종 결과: RSP={rspResult}, 메달 번호={medalResult}");
        
        // RSP 결과에 따른 처리
        if (rspResult == RSPResult.Win)
        {
            Debug.Log($"승리하고 메달 번호 {medalResult}을(를) 획득했습니다!");
            // 스택 감소 (승리 시)
            controller.DecrementStack();
        }
        else
        {
            Debug.Log($"가위바위보에서 {rspResult}했습니다. 다음 기회에!");
        }
        
        // 게임 진행 상태 업데이트
        gameInProgress = false;
    }
}

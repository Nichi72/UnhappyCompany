using System.Collections;
using UnityEngine;
using UnhappyCompany;

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
    
    // 슬롯머신 컴포넌트 참조
    private RSPSlotMachine rspSlotMachine;
    private RSPSlotMachineConnector rspConnector;
    
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
        // EnemyAIRSP에서 직접 참조 가져오기
        rspSlotMachine = controller.rspSlotMachine;
        rspConnector = controller.rspConnector;
        slotMachineUIPosition = controller.slotMachineUIPosition;
        
        // 컴포넌트 참조 확인
        if (rspSlotMachine == null)
        {
            Debug.LogWarning("RSP: RSPSlotMachine 참조가 없습니다!");
        }
        
        if (rspConnector == null)
        {
            Debug.LogWarning("RSP: RSPSlotMachineConnector 참조가 없습니다!");
        }
        else 
        {
            // 게임 완료 이벤트 구독
            Debug.Log("RSP: 게임 완료 이벤트 구독");
            
            // 이벤트 중복 구독 방지
            rspConnector.OnGameComplete -= OnSlotMachineGameCompleted;
            rspConnector.OnGameComplete += OnSlotMachineGameCompleted;
        }
        
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
        controller.StartCoroutine(MoveToJumpsquare());
    }

    public void Exit()
    {
        Debug.Log("RSP: 홀딩 상태 종료");
        
        // 슬롯머신 컨넥터가 있으면 이벤트 구독 해제
        if (rspConnector != null)
        {
            rspConnector.OnGameComplete -= OnSlotMachineGameCompleted;
        }
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

        // 슬롯머신 참조 확인
        if (rspSlotMachine == null || rspConnector == null)
        {
            Debug.LogError("RSP: 슬롯머신 또는 커넥터 참조가 없습니다. RSPHoldingState를 사용하려면 반드시 필요합니다.");
            controller.transform.parent = null;
            controller.animator.enabled = true;
            EnableCompoenet(true);
            controller.ChangeState(new RSPPatrolState(controller));
            yield break;
        }
        
        // compulsoryPlayStack이 0이 될 때까지 계속 진행
        while (controller.GetCompulsoryPlayStack() > 0)
        {
            Debug.Log($"RSP: 남은 스택 수: {controller.GetCompulsoryPlayStack()}");
            
            // 현재 스택 수에 맞게 게임 시작
            int stack = controller.GetCompulsoryPlayStack();
            if (stack <= 0) break; // 스택이 0이면 루프 종료
            
            // 게임 진행 중 플래그 초기화
            gameInProgress = true;
            Debug.Log($"RSP: 게임 시작 전 gameInProgress = {gameInProgress}");
            
            // RSPSlotMachineConnector를 통해 게임 시작
            rspConnector.StartGame(stack);
            
            // 게임이 완료될 때까지 대기
            Debug.Log("RSP: 게임 완료 대기 시작");
            yield return new WaitUntil(() => {
                //Debug.Log($"RSP: 게임 진행 상태 확인 - gameInProgress = {gameInProgress}");
                return !IsSlotMachineGameInProgress();
            });
            Debug.Log("RSP: 게임 완료 감지됨");
            
            // 짧은 대기 시간 추가
            yield return new WaitForSeconds(1f);
        }
        
        Debug.Log("RSP: 모든 스택 클리어 완료!");

        controller.transform.parent = null;
        controller.animator.enabled = true;
        EnableCompoenet(true);
        controller.ChangeState(new RSPPatrolState(controller));
    }
    
    /// <summary>
    /// 슬롯머신 게임 완료 이벤트 핸들러
    /// </summary>
    private void OnSlotMachineGameCompleted()
    {
        Debug.Log("RSP: 슬롯머신 게임 완료 이벤트 수신됨!");
        
        // 게임 진행 중 플래그를 false로 설정
        gameInProgress = false;
        Debug.Log($"RSP: gameInProgress = {gameInProgress}로 설정됨");
        
        // 승리 시 적 AI 스택 감소
        if (controller != null)
        {
            int previousStack = controller.GetCompulsoryPlayStack();
            controller.DecrementStack();
            Debug.Log($"RSP: 스택 감소됨 ({previousStack} -> {controller.GetCompulsoryPlayStack()})");
        }
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
}

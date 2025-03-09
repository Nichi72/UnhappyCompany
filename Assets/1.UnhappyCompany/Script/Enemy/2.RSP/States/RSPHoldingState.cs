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

    private Vector3 jumpsquarePosition = new Vector3(0,-2.1099999f,0.889999986f);

    public RSPHoldingState(EnemyAIRSP controller, UtilityCalculator calculator, Player player, RSPSystem rspSystem)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        this.player = player;
        this.rspSystem = rspSystem;
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

        do
        {
            controller.isAnimationEnd = false;
            var rspCoroutine = controller.StartCoroutine(PlayRSP());
            yield return rspCoroutine;
            Debug.Log("RSP: 가위바위보 게임 종료");

            if (rspResult == RSPResult.Win)
            {
                Debug.Log("RSP: 사용자가 이겼습니다.");
                controller.DecrementStack(); // compulsoryPlayStack 감소
                controller.PlayAnimation(controller.LoseAnimationName);
            }
            else if (rspResult == RSPResult.Lose)
            {
                Debug.Log("RSP: 사용자가 졌습니다. 다시 시도합니다.");
                controller.PlayAnimation(controller.WinAnimationName);
            }
            else if (rspResult == RSPResult.Draw)
            {
                Debug.Log("RSP: 무승부입니다. 다시 시도합니다.");
                controller.PlayAnimation(controller.DrawAnimationName);
            }

            // 애니메이션이 끝날 때까지 대기
            yield return new WaitUntil(() => controller.isAnimationEnd == true);
        } 

        while 
        (rspResult == RSPResult.Lose ||
         rspResult == RSPResult.Draw ||
          (controller.IsInRageState() && controller.GetCompulsoryPlayStack() > 0));


        controller.transform.parent = null;
        controller.animator.enabled = true;
        EnableCompoenet(true);
        controller.ChangeState(new RSPPatrolState(controller, utilityCalculator,60f));
    }
    RSPResult rspResult = RSPResult.Draw;
    public IEnumerator PlayRSP()
    {
        Debug.Log("RSP: 가위바위보 게임 시작");
        
        while(true)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Debug.Log("사용자가 바위를 선택했습니다.");
                rspResult = rspSystem.RSPCalculate(RSPChoice.Rock, rspSystem.GetRandomRSPChoice());
                break;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("사용자가 가위를 선택했습니다.");
                rspResult = rspSystem.RSPCalculate(RSPChoice.Scissors, rspSystem.GetRandomRSPChoice());
                break;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("사용자가 보를 선택했습니다.");
                rspResult =  rspSystem.RSPCalculate(RSPChoice.Paper, rspSystem.GetRandomRSPChoice());
                break;
            }
            yield return null;
        }
        Debug.Log("RSP: 가위바위보 게임 종료");
    }

    private void EnableCompoenet(bool isEnable)
    {
        controller.agent.enabled = isEnable;
        controller.GetComponent<Collider>().enabled = isEnable;
    }

    
}

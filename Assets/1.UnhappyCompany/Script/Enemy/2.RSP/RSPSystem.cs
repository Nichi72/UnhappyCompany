using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public enum RSPChoice
// {
//     Rock,
//     Scissors,
//     Paper
// }

// public enum RSPResult
// {
//     Win,
//     Lose,
//     Draw
// }

public class RSPSystem : MonoBehaviour
{
    [SerializeField] private RSPMedalGame medalGame; // Inspector에서 참조 설정
    
    // 가위바위보 결과 확률 설정
    private float winProbability = 30f;
    private float drawProbability = 30f;
    private float loseProbability = 40f;
    
    private bool isRSPActive = false;
    private System.Action<RSPResult, int> onGameComplete; // RSP 결과와 메달 게임 결과 콜백
    private List<GameObject> rspSymbols;
    public RSPUI rspUI;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 확률 값 직접 설정
        SetupProbabilities();
        
        // medalGame 참조 확인
        if (medalGame == null)
        {
            Debug.LogWarning("RSP: 메달 게임 참조가 설정되지 않았습니다.");
        }
        else
        {
            medalGame.Initialize();
        }
        
        // 확률 합계 검증
        ValidateProbabilities();
    }

    // 확률 값 설정
    private void SetupProbabilities()
    {
        // 기본 확률 값 설정
        winProbability = 30f;
        drawProbability = 30f;
        loseProbability = 40f;
        
        // 필요한 경우 난이도나 게임 상태에 따라 확률 조정 가능
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    // 확률 값 유효성 검사
    private void ValidateProbabilities()
    {
        float sum = winProbability + drawProbability + loseProbability;
        if (Mathf.Abs(sum - 100f) > 0.01f)
        {
            Debug.LogWarning($"RSP: 결과 확률의 총합이 100%가 아닙니다 (현재: {sum}%). 자동으로 조정됩니다.");
            
            // 비율 유지하면서 총합을 100%로 조정
            float scale = 100f / sum;
            winProbability *= scale;
            drawProbability *= scale;
            loseProbability *= scale;
        }
    }
    
    // 가위바위보 확률 설정 메서드 (외부에서 호출 가능)
    public void SetRSPProbabilities(float win, float draw, float lose)
    {
        winProbability = win;
        drawProbability = draw;
        loseProbability = lose;
        ValidateProbabilities();
    }
    
    // 설정된 확률에 따라 RSP 결과 결정
    public RSPResult GetWeightedRSPResult()
    {
        float random = Random.Range(0f, 100f);
        float cumulative = 0f;
        
        cumulative += winProbability;
        if (random <= cumulative) return RSPResult.Win;
        
        cumulative += drawProbability;
        if (random <= cumulative) return RSPResult.Draw;
        
        return RSPResult.Lose;
    }

    // 가위바위보 결과 계산
    public RSPResult RSPCalculate(RSPChoice playerChoice, RSPChoice enemyChoice)
    {
        if (playerChoice == enemyChoice)
        {
            return RSPResult.Draw;
        }

        if ((playerChoice == RSPChoice.Rock && enemyChoice == RSPChoice.Scissors) ||
            (playerChoice == RSPChoice.Scissors && enemyChoice == RSPChoice.Paper) ||
            (playerChoice == RSPChoice.Paper && enemyChoice == RSPChoice.Rock))
        {
            return RSPResult.Win;
        }
        return RSPResult.Lose;
    }

    // 확률에 따른 적 선택 생성
    public RSPChoice GetEnemyChoiceByWeight(RSPChoice playerChoice)
    {
        // 원하는 결과에 따라 적의 선택 결정
        RSPResult targetResult = GetWeightedRSPResult();
        
        switch(targetResult)
        {
            case RSPResult.Win:
                // 플레이어가 이기게 하는 적 선택
                switch(playerChoice)
                {
                    case RSPChoice.Rock: return RSPChoice.Scissors;     // 바위 > 가위
                    case RSPChoice.Scissors: return RSPChoice.Paper;    // 가위 > 보
                    case RSPChoice.Paper: return RSPChoice.Rock;        // 보 > 바위
                }
                break;
                
            case RSPResult.Draw:
                // 무승부 되는 선택
                return playerChoice;  // 같은 것 내면 무승부
                
            case RSPResult.Lose:
                // 플레이어가 지게 하는 적 선택
                switch(playerChoice)
                {
                    case RSPChoice.Rock: return RSPChoice.Paper;        // 바위 < 보
                    case RSPChoice.Scissors: return RSPChoice.Rock;     // 가위 < 바위
                    case RSPChoice.Paper: return RSPChoice.Scissors;    // 보 < 가위
                }
                break;
        }
        
        // 예외 처리
        return GetRandomRSPChoice();
    }

    // 기존 랜덤 선택 메서드 (필요시 사용)
    public RSPChoice GetRandomRSPChoice()
    {
        int randomValue = Random.Range(0, 3);
        switch (randomValue)
        {
            case 0:
                return RSPChoice.Rock;
            case 1:
                return RSPChoice.Scissors;
            case 2:
                return RSPChoice.Paper;
            default:
                return RSPChoice.Rock;
        }
    }

    // 전체 가위바위보 + 메달 게임 시작
    public void StartRSPWithMedalGame(System.Action<RSPResult, int> callback)
    {
        onGameComplete = callback;
        isRSPActive = true;
        
        // 코루틴 시작
        StartCoroutine(PlayRSPAndMedalGame());
    }

    // 가위바위보와 메달 게임 연동 코루틴
    private IEnumerator PlayRSPAndMedalGame()
    {
        // 1. 가위바위보 게임 진행
        RSPResult rspResult = RSPResult.Draw; // 기본값
        
        // 간단한 키 입력으로 가위바위보 결과 결정
        Debug.Log("RSP: 가위바위보 게임 시작 (0: 바위, 1: 가위, 2: 보)");
        
        bool rspCompleted = false;
        RSPChoice playerChoice = RSPChoice.Rock; // 기본값
        rspUI.PlayCenterRSPAnimation();
        
        while (!rspCompleted)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerChoice = RSPChoice.Rock;
                Debug.Log("사용자가 바위를 선택했습니다.");
                rspCompleted = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerChoice = RSPChoice.Scissors;
                Debug.Log("사용자가 가위를 선택했습니다.");
                rspCompleted = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerChoice = RSPChoice.Paper;
                Debug.Log("사용자가 보를 선택했습니다.");
                rspCompleted = true;
            }
            yield return null;
        }
        
        // 확률에 따른 적 선택 결정
        RSPChoice enemyChoice = GetEnemyChoiceByWeight(playerChoice);
        rspUI.StopCenterRSPAnimation(enemyChoice);
        Debug.Log($"적이 {enemyChoice}를 선택했습니다.");
        
        // 최종 결과 계산
        rspResult = RSPCalculate(playerChoice, enemyChoice);
        Debug.Log($"RSP 결과: {rspResult}");
        
        // 가위바위보 결과에 따른 사운드 재생
        if (rspResult == RSPResult.Win)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWin, transform, 40f, "RSP Win");
        }
        else if (rspResult == RSPResult.Lose)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspLose, transform, 40f, "RSP Lose");
        }
        
        yield return new WaitForSeconds(1.0f); // 사운드 재생 시간
        
        // 2. 메달 게임은 승리한 경우에만 시작
        int medalResult = 0;
        
        if (rspResult == RSPResult.Win && medalGame != null)
        {
            Debug.Log("가위바위보에서 승리하여 메달 게임을 시작합니다.");
            
            // 메달 게임 시작 및 결과 콜백 등록
            medalGame.StartGame();
            // 메달 게임 완료 대기
            yield return new WaitUntil(() => medalGame.IsGameActive());
        }
        else
        {
            Debug.Log("가위바위보에서 승리하지 못하여 메달 게임을 진행하지 않습니다.");
            yield return new WaitForSeconds(1.0f); // 잠시 대기
        }
        
        // 3. 전체 게임 완료 및 결과 반환
        isRSPActive = false;
        onGameComplete?.Invoke(rspResult, medalResult);
    }

    public void RSPAnimation()
    {
        // 가위바위보 애니메이션 재생
        // 메달 게임 애니메이션 재생
    }


    
    // 게임 진행중인지 확인
    public bool IsGameActive()
    {
        return isRSPActive;
    }
}

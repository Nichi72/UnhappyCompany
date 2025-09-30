using System.Collections;
using UnityEngine;

public class RSPMedalGame : MonoBehaviour
{
    [System.Serializable]
    public class ResultProbability
    {
        public int resultValue; // 결과 값 (1~7)
        public float probability; // 확률 (0~100)

        public ResultProbability(int value, float prob)
        {
            resultValue = value;
            probability = prob;
        }
    }
    
    // 메달 게임 결과 확률 테이블
    private ResultProbability[] resultTable;
    
    private bool isGameActive = false;
    private System.Action<int> onGameComplete; // 게임 완료 시 결과 전달 콜백
    public RSPUI rspUI;
    
    // 초기화 
    public void Initialize()
    {
        // 코드에서 직접 확률 테이블 설정
        SetupResultTable();
        
        // 확률 테이블 유효성 검사
        ValidateProbabilityTable();
    }
    
    // 확률 테이블 초기화
    private void SetupResultTable()
    {
        resultTable = new ResultProbability[]
        {
            new ResultProbability(1, 28f),  // 7번 결과: 15% 확률
            new ResultProbability(2, 28f),  // 6번 결과: 20% 확률
            new ResultProbability(4, 20f),  // 5번 결과: 25% 확률
            new ResultProbability(7, 20f),  // 4번 결과: 15% 확률
            new ResultProbability(20, 4f),  // 3번 결과: 15% 확률
        };
    }
    
    // 확률 테이블 유효성 검사
    private void ValidateProbabilityTable()
    {
        float sum = 0;
        foreach (var item in resultTable)
        {
            sum += item.probability;
        }
        
        // 합계가 100이 아니면 경고
        if (Mathf.Abs(sum - 100f) > 0.01f)
        {
            Debug.LogWarning($"메달 게임 확률 테이블의 총합이 100%가 아닙니다 (현재: {sum}%)");
        }
    }
    
    // 게임 시작 (RSP 결과는 이제 사용하지 않지만 호환성을 위해 유지)
    public void StartGame()
    {
        isGameActive = true;
        // 바로 결과 계산
        DetermineResultWithDelay();
    }
    
    // 결과 계산 코루틴 (지연 시간 포함)
    private void DetermineResultWithDelay()
    {
        // 확률 기반 결과 선택
        int result = GetRandomResultFromTable();
        Debug.Log($"메달 게임 결과: {result}번이 나왔습니다!");
        rspUI.PlayMedalWinAnimation(result);
        // 결과 로그 출력
        
     
    }
    
    // 확률 테이블에서 결과 선택
    private int GetRandomResultFromTable()
    {
        float random = Random.Range(0f, 100f);
        float cumulativeProbability = 0f;
        
        foreach (var item in resultTable)
        {
            cumulativeProbability += item.probability;
            if (random <= cumulativeProbability)
            {
                return item.resultValue;
            }
        }
        
        // 기본값 반환 (테이블에 문제가 있는 경우)
        return resultTable.Length > 0 ? resultTable[0].resultValue : 1;
    }
    
    // 게임 진행 중인지 확인
    public bool IsGameActive()
    {
        return rspUI.isOver;
    }
} 
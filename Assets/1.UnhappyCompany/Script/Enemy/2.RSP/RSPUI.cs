using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public struct RSPChoiceImage
{
    public RSPChoice choice;
    public GameObject image;
}
[Serializable]
public struct RSPResultImage
{
    public RSPResult result;
    public GameObject resultImage;
}
[Serializable]
public struct SlotSpeedProfile
{
    public float[] waitTimeByDistance;    // 남은 칸 수별 대기 시간 [5칸전, 4칸전, 3칸전, 2칸전, 1칸전]
    
    public SlotSpeedProfile(float slot5, float slot4, float slot3, float slot2, float slot1)
    {
        waitTimeByDistance = new float[] { slot5, slot4, slot3, slot2, slot1 };
    }
}
public class RSPUI : MonoBehaviour
{
    // public List<int> scores;
    public GameObject centerRSPFace; // 가위바위보 중앙 얼굴 이미지
    public List<RSPChoiceImage> cenerRSPs; // 가위바위보 중앙 이미지
    public List<RSPResultImage> results; // 결과 이미지
    public List<GameObject> numbers; // 숫자 이미지
    
    [Header("코인 개수 표시 UI (HP)")]
    [Tooltip("남은 코인 개수(HP)를 표시할 TextMeshPro 컴포넌트")]
    [SerializeField] private TextMeshProUGUI remainingCoinsText;
    
    // 이벤트 호출 함수
    public Action onRotationEnd;
    
    // 숫자별 슬롯 속도 프로파일 (남은 칸 수별 직접 대기 시간)
    private Dictionary<int, SlotSpeedProfile> speedProfiles = new Dictionary<int, SlotSpeedProfile>();


    // 결과 이미지 인덱스 상수 정의 (명확성을 위해)
    private const int RESULT_WIN_INDEX_1 = 0;  // Win 결과 이미지 1
    private const int RESULT_WIN_INDEX_2 = 1;  // Win 결과 이미지 2
    private const int RESULT_DRAW_INDEX = 2;   // Draw 결과 이미지
    private const int RESULT_LOSE_INDEX = 3;   // Lose 결과 이미지

    [SerializeField] float defaultSpinIntervalTime = 0.15f; // 기본 대기 시간 (감속 구간 아닐 때)
    [SerializeField] int slowdownStartDistance = 5; // 감속 시작 거리 (남은 칸 수)
    float rspRotationTime = 0.2f; // 가위바위보 회전 시간
    public bool isOver = false;
    private Coroutine centerRSPCoroutine;

    void Start()
    {
        speedProfiles = new Dictionary<int, SlotSpeedProfile>()
        {
            // 각 숫자마다 [5칸전, 4칸전, 3칸전, 2칸전, 1칸전] 대기 시간 (단위: 초)
            { 1,  new SlotSpeedProfile(0.20f, 0.23f, 0.25f, 0.28f, 0.30f) },  // 1점: 로그 증가 (0.2→0.3)
            { 2,  new SlotSpeedProfile(0.20f, 0.28f, 0.35f, 0.42f, 0.50f) },  // 2점: 로그 증가 (0.2→0.5)
            { 4,  new SlotSpeedProfile(0.30f, 0.42f, 0.53f, 0.64f, 0.75f) },  // 4점: 로그 증가 (0.3→0.75)
            { 7,  new SlotSpeedProfile(0.30f, 0.48f, 0.65f, 0.82f, 1.00f) },  // 7점: 로그 증가 (0.3→1.0)
            { 20, new SlotSpeedProfile(0.30f, 0.61f, 0.90f, 1.19f, 1.50f) }   // 20점: 로그 증가 (0.3→1.5)
        };
        
        // 게임 시작 전 모든 Number 비활성화
        HideAllNumbers();
        
        // 게임 시작 전 모든 결과 이미지 비활성화
        HideAllResults();
        
        // 평소 상태: centerRSPFace 켜기, cenerRSPs 끄기
        if (centerRSPFace != null)
        {
            centerRSPFace.SetActive(true);
        }
        
        foreach (var rsp in cenerRSPs)
        {
            rsp.image.SetActive(false);
        }
    }

    // 가위바위보 중앙 이미지 회전 애니메이션 시작
    public void PlayCenterRSPAnimation()
    {
        if (centerRSPCoroutine != null)
        {
            StopCoroutine(centerRSPCoroutine);
        }
        
        // 게임 시작: centerRSPFace 끄고 cenerRSPs 활성화
        if (centerRSPFace != null)
        {
            centerRSPFace.SetActive(false);
        }
        
        centerRSPCoroutine = StartCoroutine(PlayCenterRSPAnimationCo());
    }
    public void GameStartRSPAnimation()
    {

    }

    public void GameEndRSPAnimation()
    {
        // 게임 종료: cenerRSPs 끄기
        cenerRSPs.ForEach(rsp => rsp.image.SetActive(false));
        
        // 게임 종료: centerRSPFace 켜기 (평소 상태로 복귀)
        if (centerRSPFace != null)
        {
            centerRSPFace.SetActive(true);
        }
        
        // 게임 종료 시 모든 Number 비활성화
        HideAllNumbers();
        
        // 게임 종료 시 모든 결과 이미지 비활성화
        HideAllResults();
    }

    // 가위바위보 중앙 이미지 회전 애니메이션 중지
    public void StopCenterRSPAnimation(RSPChoice choice)
    {
        if (centerRSPCoroutine != null)
        {
            StopCoroutine(centerRSPCoroutine);
            centerRSPCoroutine = null;
            
            // 모든 이미지 비활성화
            foreach (var rsp in cenerRSPs)
            {
                rsp.image.SetActive(false);
            }
            cenerRSPs.Find(rsp => rsp.choice == choice).image.SetActive(true);
        }
    }

    // 가위바위보 중앙 이미지 회전 애니메이션 코루틴
    IEnumerator PlayCenterRSPAnimationCo()
    {
        // 모든 이미지 초기 비활성화
        foreach (var rsp in cenerRSPs)
        {
            rsp.image.SetActive(false);
        }
        
        int currentIndex = 0;
        
        while (true)
        {
            // 현재 인덱스의 이미지만 활성화
            cenerRSPs[currentIndex].image.SetActive(true);
            
            // 0.1초 대기
            yield return new WaitForSeconds(rspRotationTime);
            
            // 현재 이미지 비활성화
            cenerRSPs[currentIndex].image.SetActive(false);
            
            // 다음 인덱스로 이동 (순환)
            currentIndex = (currentIndex + 1) % cenerRSPs.Count;
            
            // 소리 재생
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWheelSpin, transform, 20f, "RSP Wheel Spin");
        }
    }

    public void PlayMedalWinAnimation(int targetNumber)
    {
        isOver = false;
        StartCoroutine(PlayMedalWinAnimationCo(targetNumber));
    }

    IEnumerator PlayMedalWinAnimationCo(int targetNumber)
    {
        Debug.Log($"메달 게임 애니메이션 시작 - 목표 점수: {targetNumber}");
        
        // 목표 점수에 해당하는 GameObject 찾기
        GameObject scoreNumberstemp = GetNumberFromScore(targetNumber);
        
        // null 체크 - 찾지 못하면 애니메이션 중단
        if(scoreNumberstemp == null)
        {
            Debug.LogError("메달 애니메이션 실패: 목표 점수에 해당하는 GameObject를 찾을 수 없습니다!");
            isOver = true;
            yield break;
        }
        
        int index = GetIndexFromGameObject(scoreNumberstemp);
        int currentNumber = GetCyclicIndex(index); // 시작 위치
        int maxNumber = numbers.Count;
        int loopCount = 0;
        
        // [디버그] 회전 횟수 추적 변수
        int totalSpinCount = 0;
        int targetIndex = index;
        Debug.Log($"[슬롯 회전 디버그] 시작 - 목표번호: {targetNumber}, 목표인덱스: {targetIndex}, 시작인덱스: {currentNumber}, 전체슬롯수: {maxNumber}");
        
        foreach (var number in numbers)
        {
            number.SetActive(false);
        }
        bool isBlink = false;
        while (true)
        {
            numbers[currentNumber].SetActive(true);
            ToggleResults(isBlink);
            isBlink = !isBlink;
            
            // [디버그] 현재 회전 정보 출력
            totalSpinCount++;
            
            // 목표까지 남은 칸 수 계산
            int remainingSlots = 0;
            if (loopCount >= 1)
            {
                // 한 바퀴 이상 돌았고, 목표 체크 구간
                int nextNumber = (currentNumber + 1) % maxNumber;
                if (nextNumber <= targetIndex)
                {
                    remainingSlots = targetIndex - nextNumber;
                }
                else
                {
                    // 순환해야 하는 경우
                    remainingSlots = (maxNumber - nextNumber) + targetIndex;
                }
            }
            else
            {
                // 아직 첫 바퀴 도는 중 - 충분히 큰 값
                remainingSlots = 999;
            }
            
            // 대기 시간 결정
            float waitTime;
            if (remainingSlots <= slowdownStartDistance && remainingSlots >= 0)
            {
                // 감속 구간: 남은 칸 수에 따른 대기 시간을 테이블에서 직접 가져오기
                waitTime = GetWaitTimeByDistance(targetNumber, remainingSlots);
                
                Debug.Log($"[슬롯 회전 #{totalSpinCount}] <color=yellow>감속구간!</color> 현재: {currentNumber} ({numbers[currentNumber].name}), 남은칸: {remainingSlots}, 대기시간: {waitTime:F4}초</color>");
            }
            else
            {
                // 초반: 등속
                waitTime = defaultSpinIntervalTime;
                Debug.Log($"[슬롯 회전 #{totalSpinCount}] 현재: {currentNumber} ({numbers[currentNumber].name}), 남은칸: {remainingSlots}, 루프: {loopCount}, 대기시간: {waitTime:F4}초");
            }
            
            // 대기
            yield return new WaitForSeconds(waitTime);
            
            numbers[currentNumber].SetActive(false);
            
            currentNumber++;
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWheelSpin, transform, 20f, "RSP Wheel Spin");
            if (currentNumber >= maxNumber)
            {
                Debug.Log($"[슬롯 회전 디버그] 한 바퀴 완료! 루프횟수: {loopCount} → {loopCount + 1}");
                currentNumber = 0;
                loopCount++;
            }
            if (loopCount >= 1)
            {
                Debug.Log($"[슬롯 회전 디버그] 목표 체크 - 목표: {scoreNumberstemp.name} vs 현재: {numbers[currentNumber].name}");
                if(scoreNumberstemp == numbers[currentNumber])
                {
                    numbers[currentNumber].SetActive(true);
                    // results[0].SetActive(true);
                    // results[1].SetActive(true);
                    
                    // [디버그] 최종 회전 횟수 출력
                    Debug.Log($"<color=cyan>[슬롯 회전 완료] 목표번호: {targetNumber}, 총 회전횟수: {totalSpinCount}, 루프횟수: {loopCount}</color>");
                    Debug.Log("메달 게임 당첨!");
                    
                    // 메달 게임 당첨 승리 사운드 재생
                    AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWin, transform, 40f, "RSP Medal Win");
                    
                    // 당첨 숫자를 잠깐 보여준 후 모든 숫자 비활성화
                    yield return new WaitForSeconds(1.5f);
                    HideAllNumbers();
                    
                    Debug.Log("메달 게임 종료");
                    
                    onRotationEnd?.Invoke();
                    isOver = true;
                    break;
                }
            }
        }
        void ToggleResults(bool isActive)
        {
            // results[0].SetActive(isActive);
            // results[1].SetActive(!isActive);
        }
        GameObject GetNumberFromScore(int score)
        {
            List<GameObject> result = new List<GameObject>();

            for(int i = 0; i < numbers.Count; i++)
            {
                // GameObject 이름이 숫자로만 되어있는지 확인하고 파싱
                if(int.TryParse(numbers[i].name, out int extractedNumber))
                {
                    if(extractedNumber == score)
                    {
                        Debug.Log($"찾은 GameObject: {numbers[i].name} (인덱스: {i})");
                        result.Add(numbers[i]);
                    }
                }
                else
                {
                    Debug.LogWarning($"GameObject 이름이 숫자가 아닙니다: {numbers[i].name}");
                }
            }

            if(result.Count == 0)
            {
                Debug.LogError($"점수 {score}에 해당하는 GameObject를 찾을 수 없습니다! numbers 리스트를 확인하세요.");
                return null;
            }

            // 같은 점수가 여러 개 있으면 랜덤으로 선택
            int randomIndex = UnityEngine.Random.Range(0, result.Count);
            Debug.Log($"점수 {score}에 해당하는 GameObject {result.Count}개 중 인덱스 {randomIndex} 선택");
            return result[randomIndex];
        }
        /// <summary>
        /// numbers 리스트의 인덱스가 범위를 벗어날 때 순환되도록 처리하는 함수
        /// </summary>
        /// <param name="index">원래 인덱스</param>
        /// <returns>순환된 인덱스</returns>
        int GetCyclicIndex(int index)
        {
            if (numbers == null || numbers.Count == 0)
                return -1;
                
            // 음수 인덱스 처리
            if (index < 0)
            {
                // 음수 인덱스를 양수로 변환 (-1 → Count-1, -2 → Count-2, 등)
                int positiveIndex = numbers.Count - (Mathf.Abs(index) % numbers.Count);
                return positiveIndex == numbers.Count ? 0 : positiveIndex;
            }
            
            // 양수 인덱스는 단순히 모듈로 연산
            return index % numbers.Count;
        }
        /// <summary>
        /// GameObject를 받아서 numbers 리스트에서의 인덱스를 찾는 함수
        /// </summary>
        /// <param name="targetObject">찾으려는 GameObject</param>
        /// <returns>찾은 인덱스, 없으면 -1 반환</returns>
        int GetIndexFromGameObject(GameObject targetObject)
        {
            if (numbers == null || targetObject == null)
                return -1;
                
            for (int i = 0; i < numbers.Count; i++)
            {
                if (numbers[i] == targetObject)
                    return i;
            }
            
            return -1; // 찾지 못한 경우
        }
    }
    
    /// <summary>
    /// 숫자와 남은 칸 수에 해당하는 대기 시간을 가져오는 함수
    /// </summary>
    /// <param name="number">목표 숫자</param>
    /// <param name="remainingSlots">남은 칸 수 (0~5)</param>
    /// <returns>대기 시간 (초), 찾지 못하면 기본값 0.25초 반환</returns>
    private float GetWaitTimeByDistance(int number, int remainingSlots)
    {
        // speedProfiles에서 해당 숫자 찾기
        if (speedProfiles.ContainsKey(number))
        {
            SlotSpeedProfile profile = speedProfiles[number];
            
            // remainingSlots를 배열 인덱스로 변환
            // 남은 칸 0 → 1칸 전 값 (index 4)
            // 남은 칸 1 → 2칸 전 값 (index 3)
            // 남은 칸 2 → 3칸 전 값 (index 2)
            // 남은 칸 3 → 4칸 전 값 (index 1)
            // 남은 칸 4 → 5칸 전 값 (index 0)
            int index = slowdownStartDistance - remainingSlots - 1;
            
            if (index >= 0 && index < profile.waitTimeByDistance.Length)
            {
                return profile.waitTimeByDistance[index];
            }
            else
            {
                Debug.LogWarning($"남은 칸 수 {remainingSlots}가 범위를 벗어났습니다. 기본값 0.25초 사용");
                return 0.25f;
            }
        }
        else
        {
            Debug.LogWarning($"숫자 {number}에 대한 속도 프로파일이 테이블에 없습니다. 기본값 0.25초 사용");
            return 0.25f;
        }
    }
    
    /// <summary>
    /// 모든 Number GameObject를 비활성화하는 함수
    /// </summary>
    private void HideAllNumbers()
    {
        if (numbers == null || numbers.Count == 0)
            return;
            
        foreach (var number in numbers)
        {
            if (number != null)
            {
                number.SetActive(false);
            }
        }
        
        Debug.Log("모든 Number GameObject 비활성화 완료");
    }

    /// <summary>
    /// 모든 결과 이미지를 비활성화하는 함수
    /// </summary>
    private void HideAllResults()
    {
        if (results == null || results.Count == 0)
            return;
            
        foreach (var result in results)
        {
            if (result.resultImage != null)
            {
                result.resultImage.SetActive(false);
            }
        }
        
        Debug.Log("모든 결과 이미지 비활성화 완료");
    }

    /// <summary>
    /// RSP 결과에 따라 해당하는 결과 이미지를 활성화하는 함수
    /// Win의 경우 랜덤으로 두 개 중 하나를 선택하여 활성화
    /// </summary>
    /// <param name="result">RSP 게임 결과 (Win, Draw, Lose)</param>
    public void ShowResult(RSPResult result)
    {
        // 먼저 모든 결과 이미지 비활성화
        HideAllResults();
        
        // 결과에 따라 해당 이미지 활성화
        switch (result)
        {
            case RSPResult.Win:
                // Win의 경우 랜덤으로 인덱스 0 또는 1 중 하나 선택
                int winIndex = UnityEngine.Random.Range(0, 2); // 0 또는 1
                if (winIndex == 0 && results.Count > RESULT_WIN_INDEX_1 && results[RESULT_WIN_INDEX_1].resultImage != null)
                {
                    results[RESULT_WIN_INDEX_1].resultImage.SetActive(true);
                    Debug.Log($"RSP 결과: Win (인덱스 {RESULT_WIN_INDEX_1} 활성화)");
                }
                else if (winIndex == 1 && results.Count > RESULT_WIN_INDEX_2 && results[RESULT_WIN_INDEX_2].resultImage != null)
                {
                    results[RESULT_WIN_INDEX_2].resultImage.SetActive(true);
                    Debug.Log($"RSP 결과: Win (인덱스 {RESULT_WIN_INDEX_2} 활성화)");
                }
                break;
                
            case RSPResult.Draw:
                if (results.Count > RESULT_DRAW_INDEX && results[RESULT_DRAW_INDEX].resultImage != null)
                {
                    results[RESULT_DRAW_INDEX].resultImage.SetActive(true);
                    Debug.Log($"RSP 결과: Draw (인덱스 {RESULT_DRAW_INDEX} 활성화)");
                }
                break;
                
            case RSPResult.Lose:
                if (results.Count > RESULT_LOSE_INDEX && results[RESULT_LOSE_INDEX].resultImage != null)
                {
                    results[RESULT_LOSE_INDEX].resultImage.SetActive(true);
                    Debug.Log($"RSP 결과: Lose (인덱스 {RESULT_LOSE_INDEX} 활성화)");
                }
                break;
        }
    }
    
    /// <summary>
    /// RSP의 남은 코인 개수(HP)를 UI에 업데이트합니다
    /// </summary>
    /// <param name="remainingCoins">남은 코인 개수(HP)</param>
    public void UpdateRemainingCoins(int remainingCoins)
    {
        if (remainingCoinsText != null)
        {
            // HP가 낮을 때 색상 변경
            if (remainingCoins <= 10)
            {
                remainingCoinsText.color = Color.red; // 낮은 HP는 빨간색
            }
            else if (remainingCoins <= 30)
            {
                remainingCoinsText.color = Color.yellow; // 중간 HP는 노란색
            }
            else
            {
                remainingCoinsText.color = Color.white; // 높은 HP는 흰색
            }
            
            remainingCoinsText.text = $"{remainingCoins}";
            Debug.Log($"RSP UI: HP(코인) 업데이트 - {remainingCoins}");
        }
        else
        {
            Debug.LogWarning("RSP UI: remainingCoinsText가 설정되지 않았습니다!");
        }
    }
    
    /// <summary>
    /// RSP가 죽은 상태를 UI에 표시합니다
    /// </summary>
    public void ShowDeadState()
    {
        // HP 텍스트 업데이트
        if (remainingCoinsText != null)
        {
            remainingCoinsText.text = "DEAD";
            remainingCoinsText.color = Color.red; // 빨간색으로 표시
        }
        
        // 게임 종료 애니메이션 (모든 UI 비활성화)
        GameEndRSPAnimation();
        
        Debug.Log("RSP UI: 죽음 상태 표시");
    }

    
}

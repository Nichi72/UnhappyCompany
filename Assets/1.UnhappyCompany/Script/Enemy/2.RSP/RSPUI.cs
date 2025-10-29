using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public class RSPUI : MonoBehaviour
{
    // public List<int> scores;
    public GameObject centerRSPFace; // 가위바위보 중앙 얼굴 이미지
    public List<RSPChoiceImage> cenerRSPs; // 가위바위보 중앙 이미지
    public List<RSPResultImage> results; // 결과 이미지
    public List<GameObject> numbers; // 숫자 이미지
    // 이벤트 호출 함수
    public Action onRotationEnd;

    float waitTime = 0.25f;
    float rspRotationTime = 0.2f; // 가위바위보 회전 시간
    public bool isOver = false;
    private Coroutine centerRSPCoroutine;

    void Start()
    {
        // scores = new List<int>(){1,2,4,7,20};
        
        // 게임 시작 전 모든 Number 비활성화
        HideAllNumbers();
        
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
            
            // 소리 재생 (기존 코드와 유사하게)
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
        Debug.Log("메달 게임 애니메이션 시작 - 목표 점수: " + targetNumber);
        
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
        int currentNumber = GetCyclicIndex(index + 4); // 시작 위치를 목표보다 4칸 앞으로
        int maxNumber = numbers.Count;
        int loopCount = 0;
        
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
            yield return new WaitForSeconds(waitTime);
            numbers[currentNumber].SetActive(false);
            
            currentNumber++;
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWheelSpin, transform, 20f, "RSP Wheel Spin");
            if (currentNumber >= maxNumber)
            {
                Debug.Log("loopCount: " + loopCount);
                currentNumber = 0;
                loopCount++;
            }
            if (loopCount >= 1)
            {
                Debug.Log("scoreNumberstemp: " + scoreNumberstemp.name + " numbers[currentNumber]: " + numbers[currentNumber].name);
                if(scoreNumberstemp == numbers[currentNumber])
                {
                    numbers[currentNumber].SetActive(true);
                    // results[0].SetActive(true);
                    // results[1].SetActive(true);
                    Debug.Log("메달 게임 당첨!");
                    
                    // 메달 게임 당첨 승리 사운드 재생
                    AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWin, transform, 40f, "RSP Medal Win");
                    
                    // 당첨 숫자를 잠깐 보여준 후 모든 숫자 비활성화
                    yield return new WaitForSeconds(1.5f);
                    HideAllNumbers();
                    
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


    
}

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
public class RSPUI : MonoBehaviour
{
    // public List<int> scores;
  
    public List<RSPChoiceImage> cenerRSPs; // 가위바위보 중앙 이미지
    public List<GameObject> results; // 결과 이미지
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

        
    }

    // 가위바위보 중앙 이미지 회전 애니메이션 시작
    public void PlayCenterRSPAnimation()
    {
        if (centerRSPCoroutine != null)
        {
            StopCoroutine(centerRSPCoroutine);
        }
        centerRSPCoroutine = StartCoroutine(PlayCenterRSPAnimationCo());
    }
    public void GameStartRSPAnimation()
    {

    }

    public void GameEndRSPAnimation()
    {
        cenerRSPs.ForEach(rsp => rsp.image.SetActive(false));

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
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rspWheelSpin, transform, "RSP 중앙 회전 돌아가는 소리");
        }
    }

    public void PlayMedalWinAnimation(int targetNumber)
    {
        isOver = false;
        StartCoroutine(PlayMedalWinAnimationCo(targetNumber));
    }

    IEnumerator PlayMedalWinAnimationCo(int targetNumber)
    {
        Debug.Log("targetNumber: " + targetNumber);
        
        int currentNumber = 0;
        int maxNumber = numbers.Count;
        int loopCount = 0;
        GameObject scoreNumberstemp = GetNumberFromScore(targetNumber);
        int index = GetIndexFromGameObject(scoreNumberstemp);
        currentNumber = GetCyclicIndex(index+4);
        
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
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rspWheelSpin, transform, "RSP 중앙 회전 돌아가는 소리");
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
                    results[0].SetActive(true);
                    results[1].SetActive(true);
                    Debug.Log("만나서 탈출");
                    onRotationEnd?.Invoke();
                    isOver = true;
                    break;
                }
            }
        }
        void ToggleResults(bool isActive)
        {
            results[0].SetActive(isActive);
            results[1].SetActive(!isActive);
        }
        GameObject GetNumberFromScore(int score)
        {
            List<GameObject> result = new List<GameObject>();

            for(int i = 0; i < numbers.Count; i++)
            {
                Debug.Log("number.name: " + numbers[i].name + " score: " + score);
                int extractedNumber = int.Parse(numbers[i].name.Split('(')[1].Split(')')[0]);
                if(extractedNumber == score)
                {
                    // Number (1)(0) 형식에서 첫 번째 괄호 안의 숫자 추출
                    Debug.Log("Extracted Number: " + extractedNumber);
                    result.Add(numbers[i]);
                }
            }

            int randomIndex = UnityEngine.Random.Range(0, result.Count);
            var temp =  result[randomIndex];

            return temp;
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
    


    
}

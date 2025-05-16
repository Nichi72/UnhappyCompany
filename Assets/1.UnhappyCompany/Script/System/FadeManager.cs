using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance; // 싱글톤 인스턴스

    // 각 GameObject에 대한 실행 중인 페이드 코루틴을 추적하는 딕셔너리
    private Dictionary<GameObject, Coroutine> fadeCoroutines = new Dictionary<GameObject, Coroutine>();
    
    // FadeOverlay 오브젝트 참조
    public GameObject fadeOverlay;
    

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 제거
        }
    }

    // 페이드 인 메서드
    public void FadeIn(GameObject obj, float duration, Action onComplete = null)
    {
        if (fadeCoroutines.ContainsKey(obj))
        {
            StopCoroutine(fadeCoroutines[obj]); // 기존 코루틴 중지
        }
        fadeCoroutines[obj] = StartCoroutine(FadeCanvasGroup(obj, 0, 1, duration, onComplete)); // 새로운 페이드 인 코루틴 시작
    }

    // 페이드 아웃 메서드
    public void FadeOut(GameObject obj, float duration, Action onComplete = null)
    {
        if (fadeCoroutines.ContainsKey(obj))
        {
            StopCoroutine(fadeCoroutines[obj]); // 기존 코루틴 중지
        }
        fadeCoroutines[obj] = StartCoroutine(FadeCanvasGroup(obj, 1, 0, duration, onComplete)); // 새로운 페이드 아웃 코루틴 시작
    }

    // CanvasGroup의 알파 값을 변경하여 페이드 효과를 구현하는 코루틴
    private IEnumerator FadeCanvasGroup(GameObject obj, float start, float end, float duration, Action onComplete = null)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>(); // CanvasGroup이 없으면 추가
        }
        float elapsed = 0f;
        canvasGroup.alpha = start; // 시작 알파 값 설정

        // 페이드 효과 진행
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration); // 알파 값 보간
            yield return null; // 다음 프레임까지 대기
        }
        canvasGroup.alpha = end; // 최종 알파 값 설정
        fadeCoroutines.Remove(obj); // 코루틴 추적에서 제거
        
        // 콜백 함수 실행
        onComplete?.Invoke();
    }

    // FadeOverlay 전체 화면 페이드 인 메서드
    public void ScreenFadeIn(float duration, Action onComplete = null)
    {
        if (fadeOverlay != null)
        {
            FadeIn(fadeOverlay, duration, onComplete); // duration은 페이드 인 효과의 지속 시간을 조절합니다.
        }
    }

    // FadeOverlay 전체 화면 페이드 아웃 메서드
    public void ScreenFadeOut(float duration, Action onComplete = null)
    {
        if (fadeOverlay != null)
        {
            FadeOut(fadeOverlay, duration, onComplete);
        }
    }

    // FadeIn 후 지정된 시간(delaySeconds) 후 FadeOut을 수행하는 메서드
    public void FadeInThenFadeOut(float fadeInDuration, float delaySeconds, float fadeOutDuration, Action onFadeInComplete = null, Action onFadeOutComplete = null)
    {
        StartCoroutine(FadeInThenFadeOutCoroutine(fadeOverlay, fadeInDuration, delaySeconds, fadeOutDuration, onFadeInComplete, onFadeOutComplete));
    }

    // FadeIn 후 대기 후 FadeOut을 실행하는 코루틴
    private IEnumerator FadeInThenFadeOutCoroutine(GameObject obj, float fadeInDuration, float delaySeconds, float fadeOutDuration, Action onFadeInComplete = null, Action onFadeOutComplete = null)
    {
        FadeIn(obj, fadeInDuration, onFadeInComplete); // 페이드 인 시작
        yield return new WaitForSeconds(fadeInDuration); // 페이드 인 완료 대기
        yield return new WaitForSeconds(delaySeconds); // 지정된 시간 대기
        FadeOut(obj, fadeOutDuration, onFadeOutComplete); // 페이드 아웃 시작
    }
} 
using UnityEngine;
using System.Collections;

public class DoorTrapHammer : MonoBehaviour
{
    [Header("남은 발사 횟수")]
    public int count = 3;  // 남은 발사 횟수

    [Header("해머 움직임 설정")]
    public Transform startTransform;  // 해머 시작 위치 (위쪽)
    public Transform endTransform;    // 해머 끝 위치 (아래쪽)
    public float fallSpeed = 2f;      // 떨어지는 속도
    public float riseSpeed = 1f;      // 올라가는 속도
    public float waitTime = 1f;       // 떨어진 후 머무르는 시간

    [Header("해머 트랩 상태")]
    private bool isOnProcessing = false;  // On() 함수가 실행 중인지 확인하는 플래그
    private Coroutine autoOffCoroutine;   // 자동 끄기 코루틴 참조
    private Coroutine hammerMovementCoroutine;  // 해머 움직임 코루틴 참조

    // 외부에서 On() 함수 실행 상태를 확인할 수 있도록 하는 프로퍼티
    public bool IsOnProcessing => isOnProcessing;

    // 남은 발사 횟수를 확인할 수 있는 프로퍼티
    public int RemainingCount => count;

    // 발사 횟수를 리셋하는 메서드
    public void ResetCount(int newCount = 2)
    {
        count = newCount;
        Debug.Log($"해머 트랩 발사 횟수가 {newCount}회로 리셋되었습니다.");
    }

    private void Awake()
    {
        // 해머를 시작 위치로 초기화
        if (startTransform != null)
        {
            transform.position = startTransform.position;
        }
    }

    void Start()
    {
        // 세이브 시스템에 의거하여 이후에 새롭게 다시 초기화 해야함.
        ResetCount(0);
    }

    void Update()
    {
        
    }

    public void On()
    {
        // 이미 On() 함수가 실행 중이면 리턴
        if (isOnProcessing)
        {
            Debug.Log("해머 트랩이 이미 실행 중입니다.");   
            AudioManager.instance.PlayOneShot(FMODEvents.instance.trapHammerAlreadyActive, gameObject.transform, "해머 트랩이 이미 실행 중입니다.");
            return;
        }

        // 발사 횟수 제한 체크 (count가 0이면 무제한)
        if (count <= 0)
        {
            Debug.Log("해머 트랩 발사 횟수를 초과했습니다.");
            AudioManager.instance.PlayOneShot(FMODEvents.instance.trapHammerLimitExceeded, gameObject.transform, "해머 트랩 발사 횟수를 초과했습니다.");
            return;
        }

        isOnProcessing = true;
        count--;  // 사용 횟수 감소

        Debug.Log($"해머 트랩 발사! (남은 발사 횟수: {count})");

        // 기존 자동 끄기 코루틴이 있다면 중지
        if (autoOffCoroutine != null)
        {
            StopCoroutine(autoOffCoroutine);
        }
        
        // 해머 애니메이션 또는 움직임 로직은 여기에 추가
        HammerAction();
        
        // 해머 움직임 완료 후 자동으로 끄기
        autoOffCoroutine = StartCoroutine(AutoOffAfterMovement());
        AudioManager.instance.PlayOneShot(FMODEvents.instance.trapHammerStrike, gameObject.transform, "해머 트랩 작동 소리. 무거운 해머가 내려치는 듯한 강력한 타격음. 쾅! 하는 느낌의 임팩트 있는 소리.");
    }

    private void HammerAction()
    {
        // 기존 해머 움직임 코루틴이 있다면 중지
        if (hammerMovementCoroutine != null)
        {
            StopCoroutine(hammerMovementCoroutine);
        }
        
        // 해머 움직임 시작
        hammerMovementCoroutine = StartCoroutine(HammerMovement());
    }

    private IEnumerator HammerMovement()
    {
        if (startTransform == null || endTransform == null)
        {
            Debug.LogWarning("Start Transform 또는 End Transform이 설정되지 않았습니다!");
            yield break;
        }

        Vector3 startPos = startTransform.position;
        Vector3 endPos = endTransform.position;

        // 1. 해머가 떨어지는 동작
        float fallTime = 0f;
        float fallDuration = 1f / fallSpeed;
        
        while (fallTime < fallDuration)
        {
            fallTime += Time.deltaTime;
            float fallProgress = fallTime / fallDuration;
            
            // 떨어질 때는 가속도 효과를 주기 위해 제곱 함수 사용
            float easedProgress = fallProgress * fallProgress;
            
            transform.position = Vector3.Lerp(startPos, endPos, easedProgress);
            yield return null;
        }
        
        // 정확한 끝 위치로 설정
        transform.position = endPos;
        
        // 약간의 대기 시간 (임팩트 느낌)
        yield return new WaitForSeconds(waitTime);
        
        // 2. 해머가 올라가는 동작
        float riseTime = 0f;
        float riseDuration = 1f / riseSpeed;
        
        while (riseTime < riseDuration)
        {
            riseTime += Time.deltaTime;
            float riseProgress = riseTime / riseDuration;
            
            // 올라갈 때는 부드러운 움직임을 위해 일반 lerp 사용
            transform.position = Vector3.Lerp(endPos, startPos, riseProgress);
            yield return null;
        }
        
        // 정확한 시작 위치로 설정
        transform.position = startPos;
        
        hammerMovementCoroutine = null;
        Debug.Log("해머 트랩 움직임 완료!");
    }

    private IEnumerator AutoOffAfterMovement()
    {
        // 해머 움직임이 완료될 때까지 대기
        while (hammerMovementCoroutine != null)
        {
            yield return null;
        }
        
        Off();
        isOnProcessing = false;  // On() 함수 실행 완료
        autoOffCoroutine = null;
    }

    private void Off()
    {
        // 해머 움직임 코루틴이 실행 중이면 중지하고 시작 위치로 복귀
        if (hammerMovementCoroutine != null)
        {
            StopCoroutine(hammerMovementCoroutine);
            hammerMovementCoroutine = null;
            
            if (startTransform != null)
            {
                transform.position = startTransform.position;
            }
        }
        
        Debug.Log("해머 트랩 비활성화");
    }
}

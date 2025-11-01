using UnityEngine;
using FMODUnity;

/// <summary>
/// AudioManager Emitter Pool 시스템 테스트 예제
/// 이 스크립트는 테스트용이므로 실제 사용 후 삭제하셔도 됩니다.
/// </summary>
public class AudioManagerTestExample : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private bool autoTest = false;
    [SerializeField] private float testInterval = 2f;
    
    private float lastTestTime = 0f;
    
    void Update()
    {
        if (autoTest && Time.time - lastTestTime > testInterval)
        {
            TestAllMethods();
            lastTestTime = Time.time;
        }
        
        // 키보드 단축키로 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Test_PlayUISound();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Test_Play3DSound_Position();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Test_Play3DSound_Transform();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Test_PlayLoopSound();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Test_PlayWithEmitter_Advanced();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ShowPoolInfo();
        }
    }
    
    void TestAllMethods()
    {
        Debug.Log("=== AudioManager 테스트 시작 ===");
        Test_Play3DSound_Position();
    }
    
    /// <summary>
    /// 1. UI 사운드 테스트 (기존 PlayOneShot 방식)
    /// </summary>
    void Test_PlayUISound()
    {
        Debug.Log("[테스트 1] PlayUISound - UI 클릭 사운드");
        AudioManager.instance.PlayUISound(
            FMODEvents.instance.computerCursorClick,
            "Test UI Click"
        );
    }
    
    /// <summary>
    /// 2. 3D 사운드 테스트 - 위치 기반
    /// </summary>
    void Test_Play3DSound_Position()
    {
        Debug.Log("[테스트 2] Play3DSoundAtPosition - 가까운 위치에 효과음");
        
        // FMOD Listener 확인
        if (FMODUnity.StudioListener.ListenerCount == 0)
        {
            Debug.LogError("[테스트 실패] FMOD Listener가 씬에 없습니다! 카메라나 플레이어에 'FMOD Studio Listener' 컴포넌트를 추가하세요.");
            return;
        }
        
        // 가까운 위치에 사운드 재생 (반경 2미터)
        Vector3 randomPos = transform.position + Random.insideUnitSphere * 2f;
        Debug.Log($"[테스트 2] 사운드 위치: {randomPos}, 내 위치: {transform.position}, 거리: {Vector3.Distance(transform.position, randomPos):F2}m");
        
        // EventReference 확인
        if (FMODEvents.instance.rampageCollisionObject.IsNull)
        {
            Debug.LogError("[테스트 실패] FMODEvents.instance.rampageCollisionObject가 설정되지 않았습니다!");
            return;
        }
        
        PooledEmitter emitter = AudioManager.instance.Play3DSoundAtPosition(
            FMODEvents.instance.rampageCollisionObject,
            randomPos,
            -1f,  // maxDistance (FMOD 기본값)
            "Test 3D Sound at Position"
        );
        
        if (emitter == null)
        {
            Debug.LogError("[테스트 실패] Emitter 생성 실패!");
        }
        else
        {
            Debug.Log($"[테스트 성공] Emitter 생성됨: {emitter.debugLabel}");
        }
    }
    
    /// <summary>
    /// 3. 3D 사운드 테스트 - Transform 추적
    /// </summary>
    void Test_Play3DSound_Transform()
    {
        Debug.Log("[테스트 3] Play3DSoundFollowingWithDistance - Transform 추적 (MaxDistance 40m)");
        
        // 플레이어가 있으면 플레이어, 없으면 자신
        Transform target = GameManager.instance?.currentPlayer?.transform ?? transform;
        
        // MaxDistance를 40m로 설정
        PooledEmitter emitter = AudioManager.instance.Play3DSoundByTransform(
            FMODEvents.instance.cushionImpact,
            target,
            40f,  // maxDistance = 40m
            "Test 3D Sound Following Transform"
        );
        
        if (emitter != null)
        {
            Debug.Log($"[테스트 3] Emitter 생성됨. Override Attenuation: {emitter.emitter.OverrideAttenuation}, MaxDistance: {emitter.emitter.OverrideMaxDistance}");
        }
        else
        {
            Debug.LogError("[테스트 3] Emitter 생성 실패!");
        }
    }
    
    /// <summary>
    /// 4. 루프 사운드 테스트 - 수동 제어
    /// </summary>
    private PooledEmitter loopEmitter;
    void Test_PlayLoopSound()
    {
        if (loopEmitter != null && loopEmitter.isActive)
        {
            Debug.Log("[테스트 4] PlayLoopSoundByTransform - 루프 사운드 정지");
            AudioManager.instance.StopEmitter(loopEmitter);
            loopEmitter = null;
        }
        else
        {
            Debug.Log("[테스트 4] PlayLoopSoundByTransform - 루프 사운드 시작");
            loopEmitter = AudioManager.instance.PlayLoopSoundByTransform(
                FMODEvents.instance.rampageIdle,
                transform,
                -1f,  // maxDistance (FMOD 기본값)
                "Test Loop Sound"
            );
        }
    }
    
    /// <summary>
    /// 5. 고급 옵션 테스트
    /// </summary>
    void Test_PlayWithEmitter_Advanced()
    {
        Debug.Log("[테스트 5] PlayWithEmitter - 고급 옵션");
        
        // 고급 옵션 테스트 - 새로운 API로 대체
        AudioManager.instance.Play3DSoundAtPosition(
            FMODEvents.instance.rampageBreak,
            transform.position + Vector3.up * 2f,
            40f,  // maxDistance (20m에서 2배)
            "Test Advanced Sound"
        );
    }
    
    /// <summary>
    /// 0. Pool 정보 출력
    /// </summary>
    void ShowPoolInfo()
    {
        string info = AudioManager.instance.GetEmitterPoolInfo();
        Debug.Log($"[Pool 정보] {info}");
        Debug.Log($"[활성 Emitter] {AudioManager.instance.GetActiveEmitterCount()}개");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        GUILayout.Label("=== AudioManager 테스트 ===");
        GUILayout.Space(10);
        
        GUILayout.Label("키보드 단축키:");
        GUILayout.Label("1: UI 사운드 (PlayUISound)");
        GUILayout.Label("2: 3D 사운드 - 위치");
        GUILayout.Label("3: 3D 사운드 - Transform 추적");
        GUILayout.Label("4: 루프 사운드 (토글)");
        GUILayout.Label("5: 고급 옵션");
        GUILayout.Label("0: Pool 정보 출력");
        
        GUILayout.Space(10);
        
        // 시스템 상태
        GUILayout.Label("=== 시스템 상태 ===");
        
        // FMOD Listener 확인
        int listenerCount = FMODUnity.StudioListener.ListenerCount;
        if (listenerCount > 0)
        {
            GUILayout.Label($"✓ FMOD Listener: {listenerCount}개");
        }
        else
        {
            GUI.color = Color.red;
            GUILayout.Label("✗ FMOD Listener 없음!");
            GUI.color = Color.white;
        }
        
        // Pool 상태
        if (AudioManager.instance != null)
        {
            GUILayout.Label(AudioManager.instance.GetEmitterPoolInfo());
            GUILayout.Label($"활성 Emitter: {AudioManager.instance.GetActiveEmitterCount()}개");
        }
        
        GUILayout.EndArea();
    }
}


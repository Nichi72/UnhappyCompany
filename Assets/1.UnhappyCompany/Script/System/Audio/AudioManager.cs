using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using TMPro;

#region Emitter Pool Classes

/// <summary>
/// Emitter Pool 설정
/// </summary>
[System.Serializable]
public class EmitterPoolSettings
{
    [Tooltip("초기 풀 크기")]
    public int initialPoolSize = 20;
    
    [Tooltip("최대 풀 크기 (0 = 무제한)")]
    public int maxPoolSize = 100;
    
    [Tooltip("풀이 부족할 때 자동 확장")]
    public bool autoExpand = true;
    
    [Tooltip("사용되지 않는 Emitter 자동 정리 간격 (초, 0 = 비활성화)")]
    public float autoCleanupInterval = 60f;
}

/// <summary>
/// 풀링된 Emitter 정보
/// </summary>
public class PooledEmitter
{
    public GameObject gameObject;
    public StudioEventEmitter emitter;
    public Transform transform;
    public bool isActive;
    public float lastUsedTime;
    public string currentEventName;
    
    // Transform 추적
    public bool followTarget;
    public Transform targetTransform;
    
    // 디버그 정보
    public string debugLabel;
    public float spawnTime;
    
    public PooledEmitter(GameObject go, StudioEventEmitter emit)
    {
        gameObject = go;
        emitter = emit;
        transform = go.transform;
        isActive = false;
        lastUsedTime = Time.time;
    }
}

/// <summary>
/// PlayWithEmitter 고급 옵션
/// </summary>
public class EmitterPlayOptions
{
    public Vector3 position = Vector3.zero;
    public Transform followTarget = null;
    public float volume = 1f;
    public Dictionary<string, float> parameters = null;
    public float minDistance = -1f;
    public float maxDistance = -1f;
    public bool overrideAttenuation = false;
    public float lifetime = 0f;
    public string debugName = null;
}

#endregion

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // FMOD 버스 참조 변수 추가
    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;
    private Bus voiceBus;

    // 볼륨 값 저장 변수 (인스펙터에서 초기값 설정 가능)
    [Header("볼륨 초기값 설정")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1.0f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1.0f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1.0f;
    [SerializeField, Range(0f, 1f)] private float voiceVolume = 1.0f;
    
    [Header("음소거 설정")]
    [SerializeField] private bool isMuted = false;
    
    [Header("디버그 설정")]
    [SerializeField] private bool showSoundDebug = true;
    [SerializeField] private float debugSphereSize = 0.5f;
    [SerializeField] private Color emitterColor = Color.green;
    
    [Header("Emitter Gizmo 설정")]
    [Tooltip("선택하지 않아도 모든 Emitter의 Gizmo를 항상 표시")]
    [SerializeField] private bool alwaysShowEmitterGizmos = true;
    [Tooltip("Emitter Gizmo의 Distance 범위 표시")]
    [SerializeField] private bool showEmitterDistanceGizmos = true;
    [Tooltip("Distance Gizmo 투명도")]
    [SerializeField, Range(0f, 1f)] private float distanceGizmoAlpha = 0.15f;
    
    [Header("Emitter Pool 시스템 (StudioEventEmitter 기반)")]
    [SerializeField] private bool useEmitterPool = true;
    [SerializeField] private EmitterPoolSettings poolSettings = new EmitterPoolSettings();
    
    // Emitter Pool 내부 변수
    private Transform emitterPoolParent;
    private Queue<PooledEmitter> availableEmitters;
    private List<PooledEmitter> activeEmitters;
    private int totalEmittersCreated = 0;
    private float lastCleanupTime = 0f;
    

    #region Unity Lifecycle

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
            
            // 버스 초기화
            InitializeAudioBuses();
            
            // Emitter Pool 초기화
            if (useEmitterPool)
            {
                InitializeEmitterPool();
            }
            
            // 저장된 설정 불러오기 // 빌드 전 주석 비활성화 해야함
            LoadAudioSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        // Emitter Pool 업데이트
        if (useEmitterPool && activeEmitters != null)
        {
            UpdateFollowingEmitters();
            
            // 주기적 정리
            if (poolSettings.autoCleanupInterval > 0 && 
                Time.time - lastCleanupTime > poolSettings.autoCleanupInterval)
            {
                CleanupUnusedEmitters();
                lastCleanupTime = Time.time;
            }
        }
    }

    #endregion

    #region Initialization

    private void InitializeAudioBuses()
    {
        // 버스 가져오기
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
        voiceBus = RuntimeManager.GetBus("bus:/Voice");
    }

    private void LoadAudioSettings()
    {
        // PlayerPrefs에서 저장된 볼륨 설정 불러오기
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1.0f);
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;

        // 불러온 설정 적용
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        if (isMuted)
        {
            masterBus.setVolume(0f);
        }
        else
        {
            masterBus.setVolume(masterVolume);
            musicBus.setVolume(musicVolume);
            sfxBus.setVolume(sfxVolume);
            voiceBus.setVolume(voiceVolume);
        }
    }

    public void SaveAudioSettings()
    {
        // 현재 볼륨 설정 저장
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion

    #region Volume Control

    // 마스터 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (!isMuted)
        {
            masterBus.setVolume(masterVolume);
        }
    }

    // 음악 볼륨 설정
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!isMuted)
        {
            musicBus.setVolume(musicVolume);
        }
    }

    // 효과음 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (!isMuted)
        {
            sfxBus.setVolume(sfxVolume);
        }
    }

    // 음성 볼륨 설정
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        if (!isMuted)
        {
            voiceBus.setVolume(voiceVolume);
        }
    }

    // 음소거 설정
    public void SetMute(bool mute)
    {
        isMuted = mute;
        ApplyAudioSettings();
    }

    // 볼륨 값 가져오기 함수들
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetVoiceVolume() => voiceVolume;
    public bool IsMuted() => isMuted;

    #endregion

    #region Public API - Sound Playback

    /// <summary>
    /// UI 사운드 재생 (2D)
    /// </summary>
    public EventInstance PlayUISound(EventReference eventRef, string debugName = null)
    {
        if (eventRef.IsNull)
        {
            if (debugName != null)
            {
                Debug.LogWarning($"PlayUISound: EventReference가 null입니다. ({debugName})");
            }
            return default(EventInstance);
        }
        
        try
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventRef);
            eventInstance.start();
            eventInstance.release();
            
            if (showSoundDebug && debugName != null)
            {
                Debug.Log($"PlayUISound: {debugName}");
            }
            
            return eventInstance;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayUISound: FMOD 이벤트를 생성할 수 없습니다. ({debugName})\nPath: {eventRef.Path}\nError: {e.Message}");
            return default(EventInstance);
        }
    }
    
    /// <summary>
    /// 3D 사운드 재생 - 위치 기반 (한 번만 재생)
    /// </summary>
    /// <param name="maxDistance">최대 거리 (-1 = FMOD 기본값)</param>
    public PooledEmitter Play3DSoundAtPosition(EventReference eventRef, Vector3 position, float maxDistance = -1f, string debugName = null)
    {
        // Distance 설정이 있으면 옵션 사용
        if (maxDistance > 0)
        {
            var options = new EmitterPlayOptions
            {
                position = position,
                overrideAttenuation = true,
                minDistance = 0f,  // 항상 0으로 고정
                maxDistance = maxDistance,
                lifetime = 3f,
                debugName = debugName
            };
            return PlayWithEmitter(eventRef, options);
        }
        
        // 기본값 사용
        return PlayWithEmitter(eventRef, position, debugName, 3f);
    }
    
    /// <summary>
    /// 3D 사운드 재생 - Transform 추적 (한 번만 재생)
    /// </summary>
    /// <param name="maxDistance">최대 거리 (-1 = FMOD 기본값)</param>
    public PooledEmitter Play3DSoundByTransform(EventReference eventRef, Transform target, float maxDistance = -1f, string debugName = null)
    {
        // Distance 설정이 있으면 옵션 사용
        if (maxDistance > 0)
        {
            var options = new EmitterPlayOptions
            {
                followTarget = target,
                overrideAttenuation = true,
                maxDistance = maxDistance, // min은 0으로 고정한다.
                lifetime = 5f,
                debugName = debugName
            };
            return PlayWithEmitter(eventRef, options);
        }
        
        // 기본값 사용
        return PlayWithEmitter(eventRef, target, debugName, 5f);
    }
    
    /// <summary>
    /// 루프 사운드 재생 - Transform 추적 (수동 정지 필요)
    /// </summary>
    /// <param name="maxDistance">최대 거리 (-1 = FMOD 기본값)</param>
    public PooledEmitter PlayLoopSoundByTransform(EventReference eventRef, Transform target, float maxDistance = -1f, string debugName = null)
    {
        // Distance 설정이 있으면 옵션 사용
        if (maxDistance > 0)
        {
            var options = new EmitterPlayOptions
            {
                followTarget = target,
                overrideAttenuation = true,
                minDistance = 0f,  // 항상 0으로 고정
                maxDistance = maxDistance,
                lifetime = 0f,
                debugName = debugName
            };
            return PlayWithEmitter(eventRef, options);
        }
        
        // 기본값 사용
        return PlayWithEmitter(eventRef, target, debugName, 0f);
    }
    
    /// <summary>
    /// 특정 PooledEmitter 수동 정지 및 반환
    /// </summary>
    public void StopEmitter(PooledEmitter pooled)
    {
        if (pooled != null && pooled.isActive)
        {
            ReturnEmitterToPool(pooled);
        }
    }
    
    /// <summary>
    /// 현재 활성 Emitter 개수 반환
    /// </summary>
    public int GetActiveEmitterCount()
    {
        return activeEmitters != null ? activeEmitters.Count : 0;
    }
    
    /// <summary>
    /// Emitter Pool 정보 반환
    /// </summary>
    public string GetEmitterPoolInfo()
    {
        if (!useEmitterPool) return "Emitter Pool 비활성화";
        
        int available = availableEmitters != null ? availableEmitters.Count : 0;
        int active = activeEmitters != null ? activeEmitters.Count : 0;
        
        return $"Emitter Pool: 사용 가능 {available}개 / 활성 {active}개 / 총 생성 {totalEmittersCreated}개";
    }

    #endregion

    #region Test & Utility Functions

    public void PlayTestBeep(string eventName, Transform targetTransform)
    {
        Debug.Log($"TestBeep {eventName}");
        PooledEmitter emitter = Play3DSoundByTransform(FMODEvents.instance.TestBeep, targetTransform, -1f, "Test Beep");
        if (emitter != null)
        {
            emitter.emitter.EventInstance.setVolume(0.2f);
        }
    }
    
    // 설정 UI 테스트용 사운드 재생
    public void PlaySettingsTestSound()
    {
        if (GameManager.instance.currentPlayer != null)
        {
            Play3DSoundByTransform(FMODEvents.instance.computerCursorClick, GameManager.instance.currentPlayer.transform, 20f, "UI Click Test");
        }
    }

    #endregion

    #region Safe FMOD Helper Methods
    
    /// <summary>
    /// 루프 사운드를 안전하게 생성하고 재생
    /// </summary>
    public bool SafePlayLoopSound(EventReference eventReference, Transform targetTransform, out EventInstance eventInstance, string debugName = "Sound")
    {
        eventInstance = default(EventInstance);
        
        if (eventReference.IsNull)
        {
            Debug.LogWarning($"[AudioManager] {debugName}: EventReference가 null입니다.");
            return false;
        }
        
        try
        {
            eventInstance = RuntimeManager.CreateInstance(eventReference);
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(targetTransform));
            eventInstance.start();
            Debug.Log($"[AudioManager] {debugName} 재생 시작");
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AudioManager] {debugName} 재생 실패\nPath: {eventReference.Path}\nError: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 루프 사운드를 안전하게 정지
    /// </summary>
    public bool SafeStopSound(ref EventInstance eventInstance, string debugName = "Sound")
    {
        try
        {
            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                eventInstance.release();
                Debug.Log($"[AudioManager] {debugName} 정지");
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AudioManager] {debugName} 정지 실패. Error: {e.Message}");
        }
        return false;
    }
    
    /// <summary>
    /// 사운드 3D 위치를 안전하게 업데이트
    /// </summary>
    public bool SafeUpdate3DAttributes(ref EventInstance eventInstance, Transform targetTransform, string debugName = "Sound")
    {
        if (!eventInstance.isValid())
            return false;
            
        try
        {
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(targetTransform));
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AudioManager] {debugName} 위치 업데이트 실패. Error: {e.Message}");
            return false;
        }
    }
    
    #endregion
    
    #region Emitter Pool System (Internal)

    /// <summary>
    /// Emitter Pool 초기화
    /// </summary>
    private void InitializeEmitterPool()
    {
        // poolSettings가 null이면 기본값으로 초기화
        if (poolSettings == null)
        {
            Debug.LogWarning("[AudioManager] poolSettings가 null입니다. 기본값으로 초기화합니다.");
            poolSettings = new EmitterPoolSettings();
        }
        
        // 풀 부모 오브젝트 생성
        GameObject poolParent = new GameObject("FMOD_EmitterPool");
        poolParent.transform.SetParent(transform);
        emitterPoolParent = poolParent.transform;
        
        availableEmitters = new Queue<PooledEmitter>();
        activeEmitters = new List<PooledEmitter>();
        
        // 초기 풀 생성
        for (int i = 0; i < poolSettings.initialPoolSize; i++)
        {
            CreateNewEmitter();
        }
        
        Debug.Log($"[AudioManager] Emitter Pool 초기화 완료: {poolSettings.initialPoolSize}개 생성");
    }
    
    /// <summary>
    /// 새로운 Emitter 생성
    /// </summary>
    private PooledEmitter CreateNewEmitter()
    {
        GameObject go = new GameObject($"PooledEmitter_{totalEmittersCreated++}");
        go.transform.SetParent(emitterPoolParent);
        go.SetActive(false);
        
        StudioEventEmitter emitter = go.AddComponent<StudioEventEmitter>();
        
        PooledEmitter pooled = new PooledEmitter(go, emitter);
        availableEmitters.Enqueue(pooled);
        
        return pooled;
    }
    
    /// <summary>
    /// 풀에서 Emitter 가져오기
    /// </summary>
    private PooledEmitter GetEmitterFromPool()
    {
        // Pool이 초기화되지 않았으면 초기화
        if (availableEmitters == null || activeEmitters == null)
        {
            Debug.LogWarning("[AudioManager] Emitter Pool이 초기화되지 않았습니다. 자동 초기화 중...");
            InitializeEmitterPool();
        }
        
        // 사용 가능한 Emitter가 있으면 가져오기
        if (availableEmitters.Count > 0)
        {
            return availableEmitters.Dequeue();
        }
        
        // 자동 확장이 활성화되어 있고 최대 크기를 넘지 않았으면 생성
        if (poolSettings.autoExpand && 
            (poolSettings.maxPoolSize == 0 || totalEmittersCreated < poolSettings.maxPoolSize))
        {
            Debug.LogWarning($"[AudioManager] Emitter 풀 확장: {totalEmittersCreated}/{poolSettings.maxPoolSize}");
            return CreateNewEmitter();
        }
        
        Debug.LogError("[AudioManager] Emitter 풀이 고갈되었습니다! maxPoolSize를 늘리거나 사운드 사용을 줄이세요.");
        return null;
    }
    
    /// <summary>
    /// 풀로 Emitter 반환
    /// </summary>
    private void ReturnEmitterToPool(PooledEmitter pooled)
    {
        if (pooled == null) return;
        
        // 재생 중지
        if (pooled.emitter.IsPlaying())
        {
            pooled.emitter.Stop();
        }
        
        // Emitter 초기화
        ResetEmitter(pooled);
        
        pooled.gameObject.SetActive(false);
        pooled.isActive = false;
        pooled.followTarget = false;
        pooled.targetTransform = null;
        pooled.lastUsedTime = Time.time;
        
        activeEmitters.Remove(pooled);
        availableEmitters.Enqueue(pooled);
    }
    
    /// <summary>
    /// Emitter를 기본 상태로 초기화 (재사용 전 필수)
    /// StudioEventEmitter의 내부 캐시(eventDescription, instance)를 완전히 초기화
    /// </summary>
    private void ResetEmitter(PooledEmitter pooled)
    {
        if (pooled == null || pooled.emitter == null) return;
        
        try
        {
            // 1. 재생 중지 및 인스턴스 완전 해제
            if (pooled.emitter.IsPlaying())
            {
                pooled.emitter.Stop();
            }
            
            // 2. Stop() 후에도 남아있을 수 있는 인스턴스 강제 해제
            // Stop()이 AllowFadeout일 때 instance를 즉시 해제하지 않으므로 명시적으로 해제
            if (pooled.emitter.EventInstance.isValid())
            {
                try
                {
                    var instance = pooled.emitter.EventInstance;
                    instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    instance.release();
                    instance.clearHandle();
                }
                catch (System.Exception e)
                {
                    // 이미 해제된 경우 무시
                    if (showSoundDebug)
                    {
                        Debug.LogWarning($"[AudioManager] 인스턴스 해제 중 예외 (무시 가능): {e.Message}");
                    }
                }
            }
            
            // 3. EventReference 초기화 (중요: 이게 없으면 Lookup()이 작동하지 않음)
            pooled.emitter.EventReference = default(EventReference);
            
            // 4. 거리 오버라이드 초기화
            pooled.emitter.OverrideAttenuation = false;
            pooled.emitter.OverrideMinDistance = 0f;
            pooled.emitter.OverrideMaxDistance = 0f;
            
            // 5. 리플렉션을 사용하여 내부 캐시 완전 초기화
            // eventDescription과 내부 상태를 리셋하기 위해
            var emitterType = pooled.emitter.GetType();
            
            // eventDescription 필드 초기화 (private protected)
            var eventDescriptionField = emitterType.GetField("eventDescription", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (eventDescriptionField != null)
            {
                eventDescriptionField.SetValue(pooled.emitter, default(FMOD.Studio.EventDescription));
            }
            
            // instance 필드 초기화 (protected)
            var instanceField = emitterType.GetField("instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (instanceField != null)
            {
                instanceField.SetValue(pooled.emitter, default(FMOD.Studio.EventInstance));
            }
            
            // hasTriggered 플래그 리셋 (TriggerOnce 사용 시 중요)
            var hasTriggeredField = emitterType.GetField("hasTriggered", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hasTriggeredField != null)
            {
                hasTriggeredField.SetValue(pooled.emitter, false);
            }
            
            // cachedParams 리스트 초기화
            var cachedParamsField = emitterType.GetField("cachedParams", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (cachedParamsField != null)
            {
                var cachedParams = cachedParamsField.GetValue(pooled.emitter);
                if (cachedParams is System.Collections.IList list)
                {
                    list.Clear();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AudioManager] Emitter 초기화 중 오류 발생: {e.Message}\nStack: {e.StackTrace}");
        }
    }
    
    /// <summary>
    /// 일정 시간 후 Emitter 반환
    /// </summary>
    private IEnumerator ReturnEmitterAfterTime(PooledEmitter pooled, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (pooled != null && pooled.isActive)
        {
            ReturnEmitterToPool(pooled);
        }
    }
    
    /// <summary>
    /// 재생이 끝나면 Emitter 반환
    /// </summary>
    private IEnumerator ReturnEmitterWhenFinished(PooledEmitter pooled)
    {
        if (pooled == null) yield break;
        
        // 이벤트가 끝날 때까지 대기
        while (pooled.isActive && pooled.emitter.IsPlaying())
        {
            yield return null;
        }
        
        // 추가로 0.1초 대기 (페이드아웃 등)
        yield return new WaitForSeconds(0.1f);
        
        if (pooled.isActive)
        {
            ReturnEmitterToPool(pooled);
        }
    }
    
    /// <summary>
    /// Transform 추적 중인 Emitter들 위치 업데이트
    /// </summary>
    private void UpdateFollowingEmitters()
    {
        for (int i = activeEmitters.Count - 1; i >= 0; i--)
        {
            var pooled = activeEmitters[i];
            
            if (!pooled.isActive || !pooled.emitter.IsPlaying())
            {
                // 재생이 끝난 Emitter 정리
                ReturnEmitterToPool(pooled);
                continue;
            }
            
            if (pooled.followTarget && pooled.targetTransform != null)
            {
                pooled.transform.position = pooled.targetTransform.position;
            }
        }
    }
    
    /// <summary>
    /// 사용되지 않는 Emitter 정리
    /// </summary>
    private void CleanupUnusedEmitters()
    {
        int cleaned = 0;
        
        // 비활성 Emitter 중 오래된 것들 정리 (최소 풀 크기는 유지)
        while (availableEmitters.Count > poolSettings.initialPoolSize)
        {
            var pooled = availableEmitters.Dequeue();
            
            if (Time.time - pooled.lastUsedTime > poolSettings.autoCleanupInterval)
            {
                Destroy(pooled.gameObject);
                totalEmittersCreated--;
                cleaned++;
            }
            else
            {
                // 다시 큐에 추가
                availableEmitters.Enqueue(pooled);
                break;
            }
        }
        
        if (cleaned > 0)
        {
            Debug.Log($"[AudioManager] Emitter {cleaned}개 정리됨. 현재 풀 크기: {availableEmitters.Count + activeEmitters.Count}");
        }
    }
    
    /// <summary>
    /// StudioEventEmitter를 사용하여 3D 사운드 재생 (위치 기반)
    /// </summary>
    private PooledEmitter PlayWithEmitter(EventReference eventRef, Vector3 position, string debugName = null,float lifetime = 0f)
    {
        if (!useEmitterPool)
        {
            Debug.LogWarning("[AudioManager] Emitter Pool이 비활성화되어 있습니다. useEmitterPool을 활성화하세요.");
            return null;
        }
        
        if (eventRef.IsNull)
        {
            Debug.LogWarning($"[AudioManager] PlayWithEmitter: EventReference가 null입니다.");
            return null;
        }
        
    PooledEmitter pooled = GetEmitterFromPool();
    if (pooled == null) return null;
    
    try
    {
        // 재사용 전 Emitter 완전 초기화
        ResetEmitter(pooled);
        
        // Emitter 설정
        pooled.gameObject.SetActive(true);
        pooled.transform.position = position;
        pooled.emitter.EventReference = eventRef;
        pooled.currentEventName = eventRef.Path;
        pooled.debugLabel = debugName ?? eventRef.Path;
        pooled.spawnTime = Time.time;
        pooled.isActive = true;
        pooled.followTarget = false;
        pooled.targetTransform = null;
        
        // 재생 (예외 발생 가능 - FMOD 이벤트가 없으면 여기서 터짐)
        pooled.emitter.Play();
        
        // 디버그
        if (showSoundDebug)
        {
            Debug.Log($"[Emitter] {pooled.debugLabel} 재생 (위치: {position})");
        }
        
        // 자동 반환
        if (lifetime > 0)
        {
            StartCoroutine(ReturnEmitterAfterTime(pooled, lifetime));
        }
        else
        {
            StartCoroutine(ReturnEmitterWhenFinished(pooled));
        }
        
        return pooled;
    }
    catch (System.Exception e)
    {
        // FMOD 이벤트를 찾을 수 없거나 재생에 실패한 경우
        Debug.LogError($"[AudioManager] FMOD 이벤트 재생 실패:\n" +
                        $"Event Path: {eventRef.Path}\n" +
                        $"Debug Name: {debugName}\n" +
                        $"Error: {e.Message}\n" +
                        $"Stack: {e.StackTrace}");
        
        // Emitter를 풀로 반환
        pooled.isActive = false;
        pooled.gameObject.SetActive(false);
        ReturnEmitterToPool(pooled);
        
        return null;
    }
    }
    
    /// <summary>
    /// StudioEventEmitter를 사용하여 3D 사운드 재생 (Transform 추적)
    /// </summary>
    private PooledEmitter PlayWithEmitter(EventReference eventRef,Transform target,string debugName = null,float lifetime = 0f)
    {
        if (target == null)
        {
            Debug.LogWarning("[AudioManager] PlayWithEmitter: target Transform이 null입니다.");
            return null;
        }
        
        PooledEmitter pooled = PlayWithEmitter(eventRef, target.position, debugName, lifetime);
        
        if (pooled != null)
        {
            pooled.followTarget = true;
            pooled.targetTransform = target;
            
            if (!activeEmitters.Contains(pooled))
            {
                activeEmitters.Add(pooled);
            }
        }
        
        return pooled;
    }
    
    /// <summary>
    /// StudioEventEmitter를 사용하여 3D 사운드 재생 (고급 옵션)
    /// </summary>
    private PooledEmitter PlayWithEmitter(EventReference eventRef, EmitterPlayOptions options)
    {
        if (options == null)
        {
            Debug.LogWarning("[AudioManager] PlayWithEmitter: options가 null입니다.");
            return null;
        }
        
    Vector3 position = options.followTarget != null ? options.followTarget.position : options.position;
    PooledEmitter pooled = PlayWithEmitter(eventRef, position, options.debugName, options.lifetime);
    
    if (pooled != null)
    {
        try
        {
            // 볼륨 설정
            if (options.volume != 1f)
            {
                pooled.emitter.EventInstance.setVolume(options.volume);
            }
            
            // 파라미터 설정
            if (options.parameters != null)
            {
                foreach (var param in options.parameters)
                {
                    pooled.emitter.SetParameter(param.Key, param.Value);
                }
            }
            
            // 거리 오버라이드 (항상 명시적으로 설정)
            if (options.overrideAttenuation)
            {
                pooled.emitter.OverrideAttenuation = true;
                pooled.emitter.OverrideMinDistance = 0f; // 항상 0으로 고정
                if (options.maxDistance > 0) 
                {
                    pooled.emitter.OverrideMaxDistance = options.maxDistance;
                }
            }
            else
            {
                // overrideAttenuation이 false면 명시적으로 초기화
                pooled.emitter.OverrideAttenuation = false;
                pooled.emitter.OverrideMinDistance = 0f;
                pooled.emitter.OverrideMaxDistance = 0f;
            }
            
            // Transform 추적
            if (options.followTarget != null)
            {
                pooled.followTarget = true;
                pooled.targetTransform = options.followTarget;
                
                if (!activeEmitters.Contains(pooled))
                {
                    activeEmitters.Add(pooled);
                }
            }
        }
        catch (System.Exception e)
        {
            // 추가 설정 중 예외 발생 (볼륨, 파라미터, 거리 오버라이드 등)
            Debug.LogWarning($"[AudioManager] Emitter 추가 설정 실패:\n" +
                            $"Event Path: {eventRef.Path}\n" +
                            $"Error: {e.Message}");
            // 이미 재생은 시작됐으므로 null을 반환하지 않음
        }
    }
    
    return pooled;
    }
    
    #endregion
    
    #region Debug Visualization
    
    /// <summary>
    /// 씬뷰에서 재생 중인 사운드를 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showSoundDebug)
            return;
        
        // Emitter Pool 사운드 시각화
        if (useEmitterPool)
        {
            DrawEmitterPoolSounds();
        }
    }
    
    /// <summary>
    /// Emitter Pool 사운드 시각화
    /// </summary>
    private void DrawEmitterPoolSounds()
    {
        if (activeEmitters == null)
            return;
        
        foreach (var pooled in activeEmitters)
        {
            if (!pooled.isActive || pooled.emitter == null || !pooled.emitter.IsPlaying())
                continue;
            
            Vector3 pos = pooled.transform.position;
            
            // Distance 범위 표시 (선택 여부 무관)
            if (alwaysShowEmitterGizmos && showEmitterDistanceGizmos)
            {
                DrawEmitterDistanceGizmos(pooled, pos);
            }
            
            // Emitter 아이콘 (녹색 구)
            Gizmos.color = emitterColor;
            Gizmos.DrawWireSphere(pos, debugSphereSize * 1.2f);
            
            // 중심 구
            Gizmos.color = new Color(emitterColor.r, emitterColor.g, emitterColor.b, 0.5f);
            Gizmos.DrawSphere(pos, debugSphereSize * 0.4f);
            
            // 추적 중이면 선 표시
            if (pooled.followTarget && pooled.targetTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, pooled.targetTransform.position);
                
                // 타겟 위치에 작은 원
                Gizmos.DrawWireSphere(pooled.targetTransform.position, debugSphereSize * 0.3f);
            }
            
            #if UNITY_EDITOR
            string followInfo = pooled.followTarget ? "\n[추적 중]" : "";
            float playTime = Time.time - pooled.spawnTime;
            
            // Distance 정보 추가
            string distanceInfo = "";
            if (pooled.emitter.OverrideAttenuation)
            {
                distanceInfo = $"\nMin: {pooled.emitter.OverrideMinDistance:F1}m / Max: {pooled.emitter.OverrideMaxDistance:F1}m";
            }
            
            UnityEditor.Handles.Label(
                pos + Vector3.up * (debugSphereSize * 1.5f),
                $"[EMITTER] {pooled.debugLabel}\n재생: {playTime:F1}초{followInfo}{distanceInfo}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = emitterColor },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    fontStyle = FontStyle.Bold
                }
            );
            #endif
        }
    }
    
    /// <summary>
    /// Emitter의 Min/Max Distance 범위를 Gizmo로 표시
    /// </summary>
    private void DrawEmitterDistanceGizmos(PooledEmitter pooled, Vector3 position)
    {
        if (pooled.emitter == null) return;
        
        float minDistance = 0f;
        float maxDistance = 0f;
        
        // Override된 거리 또는 FMOD 기본 거리 가져오기
        if (pooled.emitter.OverrideAttenuation)
        {
            minDistance = pooled.emitter.OverrideMinDistance;
            maxDistance = pooled.emitter.OverrideMaxDistance;
        }
        else
        {
            // FMOD EventDescription에서 기본 거리 가져오기
            var eventDesc = pooled.emitter.EventDescription;
            if (eventDesc.isValid())
            {
                eventDesc.getMinMaxDistance(out minDistance, out maxDistance);
            }
        }
        
        // Min Distance (내부 원 - 녹색)
        if (minDistance > 0.1f)
        {
            Gizmos.color = new Color(0f, 1f, 0f, distanceGizmoAlpha);
            Gizmos.DrawSphere(position, minDistance);
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(position, minDistance);
        }
        
        // Max Distance (외부 원 - 빨간색)
        if (maxDistance > 0.1f)
        {
            Gizmos.color = new Color(1f, 0f, 0f, distanceGizmoAlpha * 0.5f);
            Gizmos.DrawSphere(position, maxDistance);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawWireSphere(position, maxDistance);
        }
    }
    
    #endregion

}

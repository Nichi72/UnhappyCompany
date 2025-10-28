using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private Color oneShotColor = Color.cyan;
    [SerializeField] private Color loopSoundColor = Color.yellow;
    
    // 재생 중인 사운드 추적
    private class SoundDebugInfo
    {
        public EventInstance eventInstance;
        public Transform targetTransform;
        public Vector3 lastPosition;
        public Vector3 previousPosition;
        public string soundName;
        public float startTime;
        public bool isLoop;
        
        public SoundDebugInfo(EventInstance instance, Transform target, string name, bool loop)
        {
            eventInstance = instance;
            targetTransform = target;
            soundName = name;
            startTime = Time.time;
            isLoop = loop;
            lastPosition = target != null ? target.position : Vector3.zero;
            previousPosition = lastPosition;
        }
        
        public Vector3 GetVelocity()
        {
            return (lastPosition - previousPosition) / Time.deltaTime;
        }
    }
    
    private List<SoundDebugInfo> activeSounds = new List<SoundDebugInfo>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
            
            // 버스 초기화
            InitializeAudioBuses();
            
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
        if (showSoundDebug)
        {
            CleanupInactiveSounds();
            UpdateSoundPositions();
        }
    }
    
    /// <summary>
    /// 재생 완료된 사운드를 리스트에서 제거
    /// </summary>
    private void CleanupInactiveSounds()
    {
        activeSounds.RemoveAll(sound => !sound.eventInstance.isValid());
    }
    
    /// <summary>
    /// 디버그용 사운드 위치 업데이트
    /// </summary>
    private void UpdateSoundPositions()
    {
        foreach (var sound in activeSounds)
        {
            if (sound.targetTransform != null)
            {
                sound.previousPosition = sound.lastPosition;
                sound.lastPosition = sound.targetTransform.position;
            }
        }
    }

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

    #region 볼륨 제어 함수
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

    /// <summary>
    /// 기본적인 3D 사운드 재생 (위치 기반)
    /// </summary>
    public EventInstance PlayOneShot(EventReference eventReference, Transform targetTransform , string logMessage = null)
    {
        // EventReference가 null이거나 유효하지 않으면 실행하지 않음
        if (eventReference.IsNull)
        {
            if(logMessage != null)
            {
                Debug.LogWarning($"PlayOneShot: EventReference가 null입니다. 사운드를 재생할 수 없습니다. ({logMessage})");
            }
            else
            {
                Debug.LogWarning($"PlayOneShot: EventReference가 null입니다. 사운드를 재생할 수 없습니다.");
            }
            return default(EventInstance); // 유효하지 않은 EventInstance 반환
        }

        if(logMessage != null)
        {
            Debug.Log($"PlayOneShot: {logMessage}");
        }
        
        try
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            StartCoroutine(PlayOneShotCoroutine(eventInstance, targetTransform));
            
            // 디버그: 사운드 추적 추가
            if (showSoundDebug)
            {
                string soundName = logMessage ?? eventReference.Path;
                activeSounds.Add(new SoundDebugInfo(eventInstance, targetTransform, soundName, false));
            }
            
            return eventInstance;
        }
        catch (System.Exception e)
        {
            // FMOD에서 이벤트를 찾을 수 없는 경우 예외 처리
            if(logMessage != null)
            {
                Debug.LogError($"PlayOneShot: FMOD 이벤트를 생성할 수 없습니다. ({logMessage})\nPath: {eventReference.Path}\nError: {e.Message}");
            }
            else
            {
                Debug.LogError($"PlayOneShot: FMOD 이벤트를 생성할 수 없습니다.\nPath: {eventReference.Path}\nError: {e.Message}");
            }
            return default(EventInstance); // 유효하지 않은 EventInstance 반환
        }
    }

    private EventInstance PlayOneShotTestBeep(EventReference eventReference, Transform targetTransform)
    {
        // EventReference가 null이거나 유효하지 않으면 실행하지 않음
        if (eventReference.IsNull)
        {
            Debug.LogWarning($"PlayOneShotTestBeep: EventReference가 null입니다. 사운드를 재생할 수 없습니다.");
            return default(EventInstance); // 유효하지 않은 EventInstance 반환
        }

        try
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            StartCoroutine(PlayOneShotCoroutine(eventInstance, targetTransform));
            eventInstance.setVolume(0.2f);
            return eventInstance;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"PlayOneShotTestBeep: FMOD 이벤트를 생성할 수 없습니다.\nPath: {eventReference.Path}\nError: {e.Message}");
            return default(EventInstance); // 유효하지 않은 EventInstance 반환
        }
    }

    private IEnumerator PlayOneShotCoroutine(EventInstance eventInstance, Transform targetTransform)
    {
        // EventInstance가 유효하지 않으면 실행하지 않음
        if (!eventInstance.isValid())
        {
            yield break;
        }

        // 사운드 시작
        eventInstance.start();

        // 사운드가 재생되는 동안 위치 업데이트
        while (eventInstance.isValid())
        {
            if(targetTransform != null)
            {
                // Transform 전체를 전달하여 위치, 회전, 속도 정보 모두 업데이트
                eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(targetTransform));
            }
            yield return null;
        }

        // 사운드가 끝나면 메모리에서 해제
        eventInstance.release();
    }

    public void PlayTestBeep(string eventName, Transform targetTransform)
    {
        Debug.Log($"TestBeep {eventName}");
        AudioManager.instance.PlayOneShotTestBeep(FMODEvents.instance.TestBeep, targetTransform);
    }
    
    // 설정 UI 테스트용 사운드 재생
    public void PlaySettingsTestSound()
    {
        PlayOneShot(FMODEvents.instance.computerCursorClick, GameManager.instance.currentPlayer.transform, "UI Click Test");
    }

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
            
            // 디버그: 루프 사운드 추적 추가
            if (showSoundDebug)
            {
                activeSounds.Add(new SoundDebugInfo(eventInstance, targetTransform, debugName, true));
            }
            
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
    
    #region Debug Visualization
    
    /// <summary>
    /// 씬뷰에서 재생 중인 사운드를 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showSoundDebug || activeSounds == null)
            return;
        
        foreach (var sound in activeSounds)
        {
            if (!sound.eventInstance.isValid())
                continue;
            
            Vector3 position = sound.lastPosition;
            
            // 루프 사운드와 One-Shot 사운드 구분
            Gizmos.color = sound.isLoop ? loopSoundColor : oneShotColor;
            
            // 구 그리기
            Gizmos.DrawWireSphere(position, debugSphereSize);
            Gizmos.DrawSphere(position, debugSphereSize * 0.3f);
            
            // 움직이는 사운드는 속도 방향 화살표 표시
            Vector3 velocity = sound.GetVelocity();
            if (velocity.magnitude > 0.1f) // 움직이고 있으면
            {
                Vector3 arrowEnd = position + velocity.normalized * (debugSphereSize * 2f);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(position, arrowEnd);
                
                // 화살표 머리 그리기
                Vector3 arrowDir = velocity.normalized;
                Vector3 right = Vector3.Cross(arrowDir, Vector3.up).normalized;
                if (right.magnitude < 0.1f) right = Vector3.Cross(arrowDir, Vector3.forward).normalized;
                
                float arrowHeadSize = debugSphereSize * 0.3f;
                Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDir * arrowHeadSize + right * arrowHeadSize * 0.5f);
                Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDir * arrowHeadSize - right * arrowHeadSize * 0.5f);
            }
            
            // 사운드 이름 표시 (에디터에서만)
            #if UNITY_EDITOR
            string velocityInfo = velocity.magnitude > 0.1f ? $"\n속도: {velocity.magnitude:F1} m/s" : "";
            UnityEditor.Handles.Label(
                position + Vector3.up * (debugSphereSize + 0.3f),
                $"{sound.soundName}\n{(sound.isLoop ? "[LOOP]" : "[ONE-SHOT]")}\n재생 시간: {(Time.time - sound.startTime):F1}초{velocityInfo}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = sound.isLoop ? loopSoundColor : oneShotColor },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    fontStyle = FontStyle.Bold
                }
            );
            #endif
        }
    }
    
    /// <summary>
    /// 현재 재생 중인 사운드 개수 반환
    /// </summary>
    public int GetActiveSoundCount()
    {
        return activeSounds.Count;
    }
    
    /// <summary>
    /// 모든 재생 중인 사운드 정보 반환
    /// </summary>
    public string GetActiveSoundsInfo()
    {
        if (activeSounds.Count == 0)
            return "재생 중인 사운드 없음";
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"재생 중인 사운드: {activeSounds.Count}개");
        
        foreach (var sound in activeSounds)
        {
            if (sound.eventInstance.isValid())
            {
                string type = sound.isLoop ? "LOOP" : "ONE-SHOT";
                float duration = Time.time - sound.startTime;
                sb.AppendLine($"  - [{type}] {sound.soundName} ({duration:F1}초)");
            }
        }
        
        return sb.ToString();
    }
    
    #endregion

}

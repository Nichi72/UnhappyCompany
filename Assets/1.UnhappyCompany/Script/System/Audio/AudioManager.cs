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
                eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(targetTransform.position));
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


}

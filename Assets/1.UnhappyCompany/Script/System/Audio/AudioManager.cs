using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 기본적인 3D 사운드 재생 (위치 기반)
    /// </summary>
    public EventInstance PlayOneShot(EventReference eventReference, Transform targetTransform)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        StartCoroutine(PlayOneShotCoroutine(eventInstance, targetTransform));
        // eventInstance.setVolume(0.5f);
        return eventInstance;
    }

    private EventInstance PlayOneShotTestBeep(EventReference eventReference, Transform targetTransform)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        StartCoroutine(PlayOneShotCoroutine(eventInstance, targetTransform));
        eventInstance.setVolume(0.2f);
        return eventInstance;
    }

    private IEnumerator PlayOneShotCoroutine(EventInstance eventInstance, Transform targetTransform)
    {
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
}

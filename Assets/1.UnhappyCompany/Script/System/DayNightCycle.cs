using UnityEngine;
using System;

public class DayNightCycle : MonoBehaviour
{
    public float realTimeHourLength = 1f; // 현실의 1시간 동안 게임 내 1시간이 지나도록 설정 (N시간을 의미)
    public float gameHoursPerRealHour = 24f; // 현실 시간으로 몇 시간 동안 게임 내 하루가 지나야 하는지 설정

    private float timeMultiplier; // 게임 시간과 현실 시간의 비율을 계산
    private float currentGameTime; // 현재 게임 내 시간 (0 ~ 24)
    private bool isTimePaused = false; // 시간이 멈추는지 여부

    public event Action OnDayStart; // 낮이 시작될 때 이벤트
    public event Action OnNightStart; // 밤이 시작될 때 이벤트
    public event Action OnDayToNight; // 낮에서 밤으로 바뀔 때 이벤트
    public event Action OnNightToDay; // 밤에서 낮으로 바뀔 때 이벤트

    private bool isDaytime;

    void Start()
    {
        timeMultiplier = 24f / gameHoursPerRealHour; // 게임 내 하루가 지나야 하는 시간 설정
        currentGameTime = 0f; // 자정부터 시작
        isDaytime = currentGameTime >= 6f && currentGameTime < 18f;
    }

    void Update()
    {
        if (!isTimePaused)
        {
            UpdateGameTime();
        }
    }

    private void UpdateGameTime()
    {
        // 현실 시간의 경과에 따라 게임 시간을 업데이트합니다.
        currentGameTime += Time.deltaTime / (realTimeHourLength * 3600f) * 24f;

        // 하루가 끝나면 시간을 다시 설정합니다.
        if (currentGameTime >= 24f)
        {
            currentGameTime = 0f;
        }

        UpdateDayNightCycle();
    }

    private void UpdateDayNightCycle()
    {
        bool wasDaytime = isDaytime;
        isDaytime = currentGameTime >= 6f && currentGameTime < 18f;

        if (isDaytime)
        {
            // 낮일 경우의 처리
            Debug.Log("현재 시간은 낮입니다.");
            if (!wasDaytime)
            {
                OnNightToDay?.Invoke();
                OnDayStart?.Invoke();
            }
        }
        else
        {
            // 밤일 경우의 처리
            Debug.Log("현재 시간은 밤입니다.");
            if (wasDaytime)
            {
                OnDayToNight?.Invoke();
                OnNightStart?.Invoke();
            }
        }
    }

    public void PauseTime()
    {
        isTimePaused = true;
    }

    public void ResumeTime()
    {
        isTimePaused = false;
    }
}

using UnityEngine;
using System;
using System.Collections.Generic;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    public TimeSpan GameTime { get => gameTime; }
    public bool IsDay { get => isDay;}

    [Header("시작 시간")]
    public int startTime;
    [Header("현실 시간의 x배")]
    public float realTimeMinutesPerGameDay = 10f;
    [Header("경과 일수")]
    public int days;

    private TimeSpan gameTime; // 게임 내 시간
    private TimeSpan lastGameTime;
    private float timeMultiplier;
    private readonly TimeSpan oneDay = TimeSpan.FromHours(24);

    private TimeSpan sunriseTime = TimeSpan.FromHours(6);  // 해 뜨는 시간
    private TimeSpan sunsetTime = TimeSpan.FromHours(18);  // 해 지는 시간

    public Action OnDayStarted;
    public Action OnNightStarted;

    private Dictionary<TimeSpan, Action> timeEvents = new Dictionary<TimeSpan, Action>();

    private bool isDay;
    public bool isStop;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // 시간 배율 계산: 현실 시간 x배 동안 게임 내 24시간이 흐르도록 설정
        timeMultiplier = (float)(oneDay.TotalSeconds) / (realTimeMinutesPerGameDay * 60f);
        gameTime = TimeSpan.Zero;
        gameTime = gameTime.Add(TimeSpan.FromHours(startTime));
        lastGameTime = gameTime;
        isDay = IsCurrentTimeDay();
    }

    void Update()
    {
        if(!isStop)
        {
            // 이전 게임 시간 저장
            lastGameTime = gameTime;

            // 게임 시간 업데이트
            double deltaTimeInSeconds = Time.deltaTime * timeMultiplier;
            gameTime = gameTime.Add(TimeSpan.FromSeconds(deltaTimeInSeconds));

            if (gameTime >= oneDay)
            {
                gameTime = gameTime.Subtract(oneDay); // 하루가 지나면 시간 초기화
                lastGameTime = TimeSpan.Zero;
                days++;
            }

            // 시간 이벤트 체크
            CheckTimeEvents();

            // 낮밤 상태 체크
            UpdateDayNightCycle();
        }
        // Debug.Log($"gameTime {gameTime.Minutes}");
    }

    public void RegisterTimeEvent(TimeSpan targetTime, Action callback)
    {
        if (!timeEvents.ContainsKey(targetTime))
        {
            timeEvents.Add(targetTime, callback);
        }
    }

    private void CheckTimeEvents()
    {
        List<TimeSpan> keysToRemove = new List<TimeSpan>();
        foreach (var timeEvent in timeEvents)
        {
            if (IsTimeEventTriggered(timeEvent.Key))
            {
                timeEvent.Value.Invoke();
                keysToRemove.Add(timeEvent.Key);
            }
        }

        // 시간 이벤트 체크 후 불필요한 이벤트 제거
        foreach (var key in keysToRemove)
        {
            timeEvents.Remove(key);
        }
    }

    private bool IsTimeEventTriggered(TimeSpan eventTime)
    {
        // 하루 전환을 고려하여 이벤트 발생 여부를 확인
        if (lastGameTime <= eventTime && gameTime >= eventTime)
        {
            return true;
        }
        else if (lastGameTime > gameTime) // 하루가 넘어갔을 때
        {
            if (lastGameTime <= eventTime && eventTime < oneDay)
            {
                // 아직 다음 이벤트
                return false;
            }
            else if (TimeSpan.Zero <= eventTime && gameTime >= eventTime)
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateDayNightCycle()
    {
        bool currentIsDay = IsCurrentTimeDay();

        if (currentIsDay != isDay)
        {
            isDay = currentIsDay;
            if (isDay)
            {
                OnDayStarted?.Invoke(); // 낮 시작
            }
            else
            {
                OnNightStarted?.Invoke(); // 밤 시작
            }
        }
    }

    private bool IsCurrentTimeDay()
    {
        if (sunriseTime < sunsetTime)
        {
            // 해 뜨는 시간과 해 지는 시간 사이에 있는지 확인
            return gameTime >= sunriseTime && gameTime < sunsetTime;
        }
        else
        {
            // 해 뜨는 시간이 해 지는 시간보다 늦을 때
            return gameTime >= sunriseTime || gameTime < sunsetTime;
        }
    }

    // 게임 시간 저장 (세이브)
    public void SaveGameTime()
    {
        PlayerPrefs.SetFloat("GameTime", (float)gameTime.TotalSeconds);
    }

    // 게임 시간 로드 (로드)
    public void LoadGameTime()
    {
        float savedTimeInSeconds = PlayerPrefs.GetFloat("GameTime", 0f);
        gameTime = TimeSpan.FromSeconds(savedTimeInSeconds);
    }

    public string GetCurrentGameTime()
    {
        return gameTime.ToString(@"hh\:mm\:ss");
    }
}

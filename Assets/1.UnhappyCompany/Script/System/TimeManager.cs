using UnityEngine;
using System;
using System.Collections.Generic;

public enum TimeOfDay
{
    Morning,
    Afternoon
}

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    public TimeSpan GameTime { get; private set; }
    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public event Action<TimeOfDay> OnTimeOfDayChanged;

    [Header("시작 시간")]
    public int startTime;
    [Header("해당 시간 동안 하루가 지남")]
    public float realTimeMinutesPerGameDay = 10f;
    [Header("경과 일수")]
    public int days;

    private TimeSpan lastGameTime;
    private float timeMultiplier;
    private readonly TimeSpan oneDay = TimeSpan.FromHours(24);

    private TimeSpan sunriseTime = TimeSpan.FromHours(6);  // 해 뜨는 시간
    private TimeSpan sunsetTime = TimeSpan.FromHours(18);  // 해 지는 시간


    /// <summary>
    /// 낮 시작 이벤트
    /// </summary>
    public Action OnMorningStarted;
    /// <summary>
    /// 밤 시작 이벤트
    /// </summary>
    public Action OnNightStarted;

    /// <summary>
    /// 하루가 지났을 때 호출되는 이벤트
    /// </summary>
    public event Action OnDayPassed;

    private Dictionary<TimeSpan, Action> timeEvents = new Dictionary<TimeSpan, Action>();

    private bool isDay;
    private bool isStop;

    // 새로운 변수 추가: 하루가 지났는지 여부를 나타내는 플래그
    public bool HasDayPassed { get; private set; } = false;
    public bool IsStop 
    { 
        get => isStop; 
        set 
        {
            isStop = value;
            Debug.Log($"isStop: {isStop}");
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 시간 배율 계산: 현실 시간 x배 동안 게임 내 24시간이 흐르도록 설정
        timeMultiplier = (float)(oneDay.TotalSeconds) / (realTimeMinutesPerGameDay * 60f);
        GameTime = TimeSpan.Zero;
        GameTime = GameTime.Add(TimeSpan.FromHours(startTime));
        lastGameTime = GameTime;
        isDay = IsCurrentTimeDay();

        // 이벤트 초기화
        OnDayPassed += () =>
        {
            UIManager.instance.screenDayText.text = $"Day {days}";
            Debug.Log($"Day {days}");
        };
        OnMorningStarted += () =>
        {
            Debug.Log("낮이 시작되었습니다.");
            days++;
            HasDayPassed = true;
            OnDayPassed?.Invoke();
        };
        OnNightStarted += () =>
        {
            Debug.Log("밤이 시작되었습니다.");
        };
    }

    void Update()
    {
        if (!IsStop)
        {
            // 이전 게임 시간 저장
            lastGameTime = GameTime;

            // 게임 시간 업데이트
            double deltaTimeInSeconds = Time.deltaTime * timeMultiplier;
            GameTime = GameTime.Add(TimeSpan.FromSeconds(deltaTimeInSeconds));

            if (GameTime >= oneDay)
            {
                GameTime = GameTime.Subtract(oneDay); // 하루가 지나면 시간 초기화
                lastGameTime = TimeSpan.Zero;
            }

            // 시간 이벤트 체크
            CheckTimeEvents();

            // 낮밤 상태 체크
            UpdateDayNightCycle();

            TimeOfDay previousTimeOfDay = CurrentTimeOfDay;
            CurrentTimeOfDay = GameTime.Hours < 12 ? TimeOfDay.Morning : TimeOfDay.Afternoon;

            if (previousTimeOfDay != CurrentTimeOfDay)
            {
                OnTimeOfDayChanged?.Invoke(CurrentTimeOfDay);
            }
        }
        // Debug.Log($"gameTime {GameTime.Minutes}");
    }

    /// <summary>
    /// 하루가 지났는지 확인한 후 플래그를 리셋하는 메서드입니다.
    /// 하루가 지났다면 True를 반환하고 플래그를 리셋합니다.
    /// </summary>
    /// <returns>하루가 지났다면 True, 아니면 False</returns>
    public bool CheckAndResetDayPassed()
    {
        if (HasDayPassed)
        {
            HasDayPassed = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 시간 이벤트 생성
    /// </summary>
    /// <param name="targetTime"></param>
    /// <param name="callback"></param>
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
        if (lastGameTime <= eventTime && GameTime >= eventTime)
        {
            return true;
        }
        else if (lastGameTime > GameTime) // 하루가 넘어갔을 때
        {
            if (lastGameTime <= eventTime && eventTime < oneDay)
            {
                // 아직 다음 이벤트
                return false;
            }
            else if (TimeSpan.Zero <= eventTime && GameTime >= eventTime)
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
                OnMorningStarted?.Invoke(); // 낮 시작
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
            return GameTime >= sunriseTime && GameTime < sunsetTime;
        }
        else
        {
            // 해 뜨는 시간이 해 지는 시간보다 늦을 때
            return GameTime >= sunriseTime || GameTime < sunsetTime;
        }
    }

    // 게임 시간 저장 (세이브)
    public void SaveGameTime()
    {
        PlayerPrefs.SetFloat("GameTime", (float)GameTime.TotalSeconds);
    }

    // 게임 시간 로드 (로드)
    public void LoadGameTime()
    {
        float savedTimeInSeconds = PlayerPrefs.GetFloat("GameTime", 0f);
        GameTime = TimeSpan.FromSeconds(savedTimeInSeconds);
    }

    public string GetCurrentGameTime()
    {
        return GameTime.ToString(@"hh\:mm\:ss");
    }
}

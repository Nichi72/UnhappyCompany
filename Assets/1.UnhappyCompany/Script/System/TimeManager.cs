using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TimeOfDay
{
    Morning,
    Afternoon
}

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    private NotificationSystem notificationSystem;


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
    public TimeSpan SunriseTime { get => sunriseTime;}
    public TimeSpan SunsetTime { get => sunsetTime;}
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
    private Dictionary<TimeSpan, Action> enemyScheduleEvents = new Dictionary<TimeSpan, Action>();

    /// <summary>
    /// true 일 때 낮, false 일 때 밤
    /// </summary>
    private bool IsDay;
    public bool isStop;

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
        IsDay = IsCurrentTimeDay();

        // 이벤트 초기화
        OnDayPassed += () =>
        {
            UIManager.instance.screenDayText.text = $"Day {days}";
            Debug.Log($"Day {days}");

            // 알림 시스템에서 알림을 꺼내 디버그 출력
            if (notificationSystem != null)
            {
                while (notificationSystem.delayedNotificationQueue.Count > 0)
                {
                    notificationSystem.delayedNotificationQueue.Dequeue();
                }
            }
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
            // 적 스케줄 이벤트 체크
            CheckEnemyScheduleEvents();

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
    public void AddTimeEvent(TimeSpan targetTime, Action callback)
    {
        if (!timeEvents.ContainsKey(targetTime))
        {
            timeEvents.Add(targetTime, callback);
        }
    }

    /// <summary>
    /// 적 스케줄 이벤트 생성
    /// </summary>
    /// <param name="targetTime">이벤트가 발생할 시간</param>
    /// <param name="callback">이벤트 발생 시 실행할 콜백 함수</param>
    public void AddEnemyScheduleEvent(TimeSpan targetTime, Action callback)
    {
        if (!enemyScheduleEvents.ContainsKey(targetTime))
        {
            enemyScheduleEvents.Add(targetTime, callback);
        }
        else
        {
            Debug.LogError($"이미 존재하는 이벤트입니다. {targetTime}");
        }
    }

    public int GetEnemyScheduleEventCount()
    {
        return enemyScheduleEvents.Count;
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

    private void CheckEnemyScheduleEvents()
    {
        List<TimeSpan> keysToRemove = new List<TimeSpan>();
        foreach (var scheduleEvent in enemyScheduleEvents)
        {
            if (IsTimeEventTriggered(scheduleEvent.Key))
            {
                scheduleEvent.Value.Invoke();
                keysToRemove.Add(scheduleEvent.Key);
            }
        }

        // 적 스케줄 이벤트 체크 후 불필요한 이벤트 제거
        foreach (var key in keysToRemove)
        {
            enemyScheduleEvents.Remove(key);
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

        if (currentIsDay != IsDay)
        {
            IsDay = currentIsDay;
            if (IsDay)
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

    /// <summary>
    /// 등록된 모든 적 스케줄을 콘솔에 출력합니다.
    /// </summary>
    public void PrintEnemySchedules()
    {
        if (enemyScheduleEvents.Count == 0)
        {
            Debug.Log("등록된 적 스케줄이 없습니다.");
            return;
        }

        Debug.Log("=== 등록된 적 스케줄 목록 ===");
        foreach (var schedule in enemyScheduleEvents)
        {
            Debug.Log($"시간: {schedule.Key.ToString(@"hh\:mm\:ss")}");
        }
        Debug.Log("=========================");
    }

    /// <summary>
    /// 가장 가까운 스케줄 이벤트의 5초 전까지 시간을 빠르게 진행합니다.
    /// </summary>
    public void FastForwardToNearestSchedule()
    {
        if (enemyScheduleEvents.Count == 0)
        {
            Debug.Log("등록된 스케줄 이벤트가 없습니다.");
            return;
        }

        // 가장 가까운 스케줄 시간 찾기
        TimeSpan nearestSchedule = TimeSpan.MaxValue;
        foreach (var schedule in enemyScheduleEvents.Keys)
        {
            if (schedule > GameTime && schedule < nearestSchedule)
            {
                nearestSchedule = schedule;
            }
        }

        if (nearestSchedule == TimeSpan.MaxValue)
        {
            Debug.Log("다음 스케줄 이벤트가 없습니다.");
            return;
        }

        // 5초 전 시간으로 설정
        TimeSpan targetTime = nearestSchedule.Subtract(TimeSpan.FromSeconds(5));
        if (targetTime < GameTime)
        {
            targetTime = targetTime.Add(oneDay);
        }

        // 시간 설정
        GameTime = targetTime;
        lastGameTime = GameTime;
        Debug.Log($"시간이 {targetTime}로 설정되었습니다. 다음 스케줄까지 5초 남았습니다.");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TimeManager))]
public class TimeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TimeManager timeManager = (TimeManager)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("스케줄 목록 출력"))
        {
            timeManager.PrintEnemySchedules();
        }

        if (GUILayout.Button("가장 가까운 스케줄로 이동"))
        {
            timeManager.FastForwardToNearestSchedule();
        }
    }
}
#endif

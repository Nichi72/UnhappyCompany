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
    [Header("currentTimeText")]
    public string currentTimeText;
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
    [Header("시간 배율")]
    [Range(0.5f, 4f)]
    public float timeMultiplier = 1f;

    private TimeSpan lastGameTime;
    private float timeScaleFactor;
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

    /// <summary>
    /// enemyScheduleEvents를 외부에서 접근하기 위한 속성
    /// </summary>
    public IReadOnlyDictionary<TimeSpan, Action> EnemyScheduleEvents => enemyScheduleEvents;

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
        // timeScaleFactor // timeMultiplier
        timeScaleFactor = (float)(oneDay.TotalSeconds) / (realTimeMinutesPerGameDay * 60f);
        GameTime = TimeSpan.Zero;
        GameTime = GameTime.Add(TimeSpan.FromHours(startTime));
        lastGameTime = GameTime;
        IsDay = IsCurrentTimeDay();

        // 이벤트 초기화
        OnDayPassed += () =>
        {
            UIManager.instance.statusBarDayText.text = $"Day {days}";
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
        currentTimeText = GameTime.ToString(@"hh\:mm\:ss");
        if (!IsStop)
        {
            // 이전 게임 시간 저장
            lastGameTime = GameTime;

            // 게임 시간 업데이트
            double deltaTimeInSeconds = Time.deltaTime * timeScaleFactor * timeMultiplier;
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
        UIManager.instance.UpdateStatusBarTimeText(GameTime.ToString(@"hh\:mm\:ss"));
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

    /// <summary>
    /// 현재 시간이 기준 시간을 지났는지 확인하는 메서드
    /// 하루가 넘어가는 상황도 처리합니다.
    /// </summary>
    /// <param name="currentTime">현재 시간</param>
    /// <param name="targetTime">확인할 대상 시간</param>
    /// <param name="referenceTime">기준 시간 (보통 객체 생성 시간)</param>
    /// <returns>대상 시간을 지났으면 true, 아니면 false</returns>
    public bool HasTimePassed(TimeSpan currentTime, TimeSpan targetTime, TimeSpan referenceTime)
    {
        // 현재 시간과 생성 시간 사이의 실제 경과 시간 계산 (일수 고려)
        float realElapsedMinutes = GetRealElapsedMinutes(referenceTime, currentTime);
        
        // 기준 시간에서 목표 시간까지의 게임 내 경과 시간을 현실 시간(분)으로 변환
        float targetGameSeconds = 0;
        
        if (targetTime >= referenceTime)
        {
            // 같은 날인 경우
            targetGameSeconds = (float)(targetTime - referenceTime).TotalSeconds;
        }
        else
        {
            // 다음 날로 넘어가는 경우
            targetGameSeconds = (float)((TimeSpan.FromHours(24) - referenceTime) + targetTime).TotalSeconds;
        }
        
        // 게임 내 시간(초)을 현실 시간(분)으로 변환
        float targetRealMinutes = targetGameSeconds * (realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
        
        // 현실 경과 시간이 목표 시간을 넘었는지 확인
        return realElapsedMinutes >= targetRealMinutes;
    }
    
    /// <summary>
    /// 두 게임 내 시간 사이의 실제 경과된 현실 시간(분)을 계산합니다.
    /// 일수를 고려하여 정확한 경과 시간을 반환합니다.
    /// </summary>
    /// <param name="startTime">시작 시간</param>
    /// <param name="currentTime">현재 시간</param>
    /// <returns>경과된 현실 시간(분)</returns>
    public float GetRealElapsedMinutes(TimeSpan startTime, TimeSpan currentTime)
    {
        // 게임 시작 후 지금까지 총 현실 경과 시간(분)
        float totalRealMinutesSinceStart = Time.time / 60f;
        
        // 게임 시작 후 startTime까지의 게임 내 경과 시간(초)
        float gameSecondsAtStart = 0;
        if (startTime > GameTime)
        {
            // 하루가 넘어간 경우
            gameSecondsAtStart = (float)((TimeSpan.FromHours(24) - startTime) + GameTime).TotalSeconds;
        }
        else
        {
            // 같은 날인 경우
            gameSecondsAtStart = (float)(GameTime - startTime).TotalSeconds;
        }
        
        // 게임 내 시간(초)을 현실 시간(분)으로 변환
        float realMinutesAtStart = gameSecondsAtStart * (realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
        
        // 실제 경과 시간(분)
        return totalRealMinutesSinceStart - realMinutesAtStart;
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
    /// 게임 내 시간으로 하루(24시간)에 해당하는 초 값을 반환합니다.
    /// </summary>
    /// <returns>게임 내 하루를 초 단위로 변환한 값</returns>
    public float GetDayInSeconds()
    {
        return (float)oneDay.TotalSeconds;
    }
    
    /// <summary>
    /// 게임 내 시간으로 지정된 시간(시)에 해당하는 초 값을 반환합니다.
    /// </summary>
    /// <param name="hours">변환할 시간(시)</param>
    /// <returns>게임 내 시간을 초 단위로 변환한 값</returns>
    public float GetHoursInSeconds(float hours)
    {
        return hours * 3600f;
    }
    
    /// <summary>
    /// 게임 내 시간으로 지정된 시간(분)에 해당하는 초 값을 반환합니다.
    /// </summary>
    /// <param name="minutes">변환할 시간(분)</param>
    /// <returns>게임 내 분을 초 단위로 변환한 값</returns>
    public float GetMinutesInSeconds(float minutes)
    {
        return minutes * 60f;
    }

    /// <summary>
    /// 현실 시간(분)을 게임 내 시간(초)으로 변환합니다.
    /// </summary>
    /// <param name="realMinutes">현실 시간(분)</param>
    /// <returns>게임 내 시간(초)</returns>
    public float ConvertRealMinutesToGameSeconds(float realMinutes)
    {
        // 현실 시간 : 게임 시간 = realTimeMinutesPerGameDay : 24시간(86400초)
        float gameSeconds = realMinutes * ((float)oneDay.TotalSeconds / realTimeMinutesPerGameDay);
        return gameSeconds;
    }
    
    /// <summary>
    /// 현실 시간(시간)을 게임 내 시간(초)으로 변환합니다.
    /// </summary>
    /// <param name="realHours">현실 시간(시간)</param>
    /// <returns>게임 내 시간(초)</returns>
    public float ConvertRealHoursToGameSeconds(float realHours)
    {
        return ConvertRealMinutesToGameSeconds(realHours * 60f);
    }
    
    /// <summary>
    /// 현실 시간(초)을 게임 내 시간(초)으로 변환합니다.
    /// </summary>
    /// <param name="realSeconds">현실 시간(초)</param>
    /// <returns>게임 내 시간(초)</returns>
    public float ConvertRealSecondsToGameSeconds(float realSeconds)
    {
        return ConvertRealMinutesToGameSeconds(realSeconds / 60f);
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

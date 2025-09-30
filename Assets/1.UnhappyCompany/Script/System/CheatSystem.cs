using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CheatSystem : MonoBehaviour
{
    public static CheatSystem instance;
    public GameObject cheatPanel;
    public Transform InitEnemyPoint;

    [SerializeField] private Button btnInitEgg;
    [SerializeField] private Button btnShowSchedules;
    [SerializeField] private Button btnInitEnemyFromInitEnemyPoint;
    [SerializeField] private Button btnInitDefaultSchedule;
    [SerializeField] private Button btnFastForwardToNearestSchedule;
    [SerializeField] private Button btnCheatEggHatchIntoEnemy;
    [SerializeField] private Transform scheduleContentParent;
    [SerializeField] private GameObject scheduleItemPrefab;
    [SerializeField] private GameObject schedulePanel;
    private List<GameObject> scheduleItems = new List<GameObject>();
    
    void Awake()
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
        btnInitEgg.onClick.AddListener(InitEgg);
        
        if (btnShowSchedules != null)
        {
            btnShowSchedules.onClick.AddListener(ToggleSchedulePanel);
        }
        if (btnInitEnemyFromInitEnemyPoint != null)
        {
            btnInitEnemyFromInitEnemyPoint.onClick.AddListener(InitEnemy);
        }
        if(btnInitDefaultSchedule != null)
        {
            btnInitDefaultSchedule.onClick.AddListener(RegionManager.instance.InitDefaultSchedule);
        }
        if(btnInitEnemyFromInitEnemyPoint != null)
        {
            btnInitEnemyFromInitEnemyPoint.onClick.AddListener(InitEnemy);
        }

        if(btnFastForwardToNearestSchedule != null)
        {
            btnFastForwardToNearestSchedule.onClick.AddListener(TimeManager.instance.FastForwardToNearestSchedule);
        }
        if(btnCheatEggHatchIntoEnemy != null)
        {
            btnCheatEggHatchIntoEnemy.onClick.AddListener(EnemyManager.instance.CheatEggHatchIntoEnemy);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            // EnemyManager.instance.RandomSpawnEnemy();
            InitEnemy();
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            RegionManager.instance.InitDefaultSchedule();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TimeManager.instance.FastForwardToNearestSchedule();
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ToggleSchedulePanel();
        }
    }

    public void ToggleSchedulePanel()
    {
        if (schedulePanel != null)
        {
            bool isActive = !schedulePanel.activeSelf;
            schedulePanel.SetActive(isActive);
            
            if (isActive)
            {
                UpdateScheduleUI();
            }
        }
    }

    /// <summary>
    /// enemyScheduleEvents의 내용을 UI에 표시
    /// </summary>
    private void UpdateScheduleUI()
    {
        // 기존 항목 제거
        foreach (var item in scheduleItems)
        {
            Destroy(item);
        }
        scheduleItems.Clear();

        // enemyScheduleEvents 가져오기
        var scheduleEvents = TimeManager.instance.EnemyScheduleEvents;
        
        if (scheduleEvents.Count == 0)
        {
            CreateScheduleItem("등록된 적 스케줄이 없습니다.", TimeSpan.Zero);
            return;
        }

        // 시간별로 정렬하기 위한 리스트
        List<TimeSpan> sortedTimes = new List<TimeSpan>(scheduleEvents.Keys);
        sortedTimes.Sort();

        foreach (var time in sortedTimes)
        {
            CreateScheduleItem($"스케줄 이벤트", time);
        }
    }
    
    private void CreateScheduleItem(string text, TimeSpan time)
    {
        if (scheduleItemPrefab == null || scheduleContentParent == null)
        {
            Debug.LogError("scheduleItemPrefab 또는 scheduleContentParent가 할당되지 않았습니다.");
            return;
        }
        
        GameObject item = Instantiate(scheduleItemPrefab, scheduleContentParent);
        scheduleItems.Add(item);
        
        TextMeshProUGUI timeText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (timeText != null)
        {
            if (time == TimeSpan.Zero && text.Contains("없습니다"))
            {
                timeText.text = text;
            }
            else
            {
                timeText.text = $"{text} - {time.ToString(@"hh\:mm\:ss")}";
            }
        }
    }

    private void InitEgg()
    {
        Debug.Log("InitEgg");
        EnemyManager.instance.SpawnEggsInEachRoom();
    }

    private void RandomInitEnemy()
    {
        Debug.Log("InitEnemy");
        EnemyManager.instance.RandomSpawnEnemy();
    }

    private void InitEnemy()
    {
        Debug.Log("InitEnemy");
        EnemyManager.instance.SpawnEnemy(InitEnemyPoint);
    }
    // 아이템 세이브 & 로드 관련
    private void SaveAndLoad()
    {
        // if (Input.GetKeyDown(KeyCode.F1))
        // {
        //     SaveManager.Instance.SaveGame();
        // }
        // if (Input.GetKeyDown(KeyCode.F2))
        // {
        //     SaveManager.Instance.LoadGame();
        // }
        // if (Input.GetKeyDown(KeyCode.F3))
        // {
        //     ItemLoader.Instance.ClearSceneItems();
        // }
    }
}

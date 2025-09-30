using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{

    public static UIManager instance; // 싱글톤 인스턴스
    [Header("처음 비활성화 될 UI")]
    public List<GameObject> unActiveObjects = new List<GameObject>();

    [Header("골드 관련 UI")]
    public TMP_Text totalGoldText; // 총 골드 텍스트

    [Header("Player Status UI")]
    public Slider healthBar; // HP 상태바 슬라이더
    public Slider staminaBar; // 스태미나 상태바 슬라이더
    
    [Header("CCTV 관련 UI")]
    public GameObject cctvButtonPrefab; // CCTV 버튼 프리팹
    [SerializeField] private Transform cctvButtonParent; // CCTV 버튼 부모 트랜스폼
    public List<GameObject> cctvButtons = new List<GameObject>(); // 생성된 CCTV 버튼 리스트\

    [Header("게임 진행 관련 UI")]
    public TMP_Text screenDayText;
    public GameObject gameOverImage; // 게임 오버 이미지 오브젝트

    [Header("모바일 화면 관련 UI")]
    public TMP_Text statusBarTimeText;
    public TMP_Text statusBarDayText;

    [Header("배터리 관련 UI")]
    public GameObject batteryStatusItemPrefab;
    public Transform batteryStatusItemParent;
    public TMP_Text totalConsumersText; // 전체 소비자 수 텍스트 (TMP 사용)
    public TMP_Text totalBatteryDrainPerSecondText; // 초당 전체 배터리 소모량 텍스트
    public TMP_Text centerBatteryLevelText; 
    public Image centerBatteryLevelImage;
    public Dictionary<string, GameObject> batteryStatusItems = new Dictionary<string, GameObject>();

    [Header("Settings UI")]
    public GameObject settingsUI;

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 제거
        }
    }

    private void Start()
    {
        // 상태바 초기화
        if (healthBar != null)
        {
            healthBar.fillRect.GetComponent<Image>().color = Color.red; // HP 바 색상 설정
        }
        if (staminaBar != null)
        {
            staminaBar.fillRect.GetComponent<Image>().color = Color.yellow; // 스태미나 바 색상 설정
        }
        foreach(var obj in unActiveObjects)
        {
            obj.SetActive(false);
        }
        screenDayText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        UpdateCenterBatteryLevel();
        // if(Input.GetKeyDown(KeyCode.Escape))
        // {
        //     ToggleObject(settingsUI);
        // }
    }

    public void UpdateCenterBatteryLevel()
    {
        float currentBatteryLevel = CentralBatterySystem.Instance.currentBatteryLevel;
        centerBatteryLevelImage.fillAmount = currentBatteryLevel / CentralBatterySystem.Instance.MaxBatteryLevel;
        centerBatteryLevelText.text = $"{currentBatteryLevel}";

        totalBatteryDrainPerSecondText.text = $"{CentralBatterySystem.Instance.CalculateTotalBatteryDrainPerSecond()}";
    }
    

    // 총 골드 텍스트 업데이트 메서드
    public void UpdateGold(int totalGold)
    {
        // totalGoldText.text = $"Total Gold: {totalGold}";
    }

    // 전체 소비자 수 업데이트 메서드
    public void UpdateTotalConsumers(int count)
    {
        totalConsumersText.text = $"Total Consumers: {count}";
    }
    public void UpdateTotalBattery()
    {

    }
    public void RemoveBatteryStatusItem(ICentralBatteryConsumer consumer)
    {
        if (consumer == null) return;
        
        string consumerId = consumer.ID;
        if (batteryStatusItems.ContainsKey(consumerId))
        {
            GameObject uiItem = batteryStatusItems[consumerId];
            batteryStatusItems.Remove(consumerId);
            
            if (uiItem != null)
            {
                Destroy(uiItem);
            }
        }
    }
    public void InitBatteryStatusItem(ICentralBatteryConsumer consumer)
    {
        if (consumer == null) return;
        
        string consumerId = consumer.ID;
        // // 이미 존재하는 경우 기존 항목 제거
        // if (batteryStatusItems.ContainsKey(consumerId))
        // {
        //     RemoveBatteryStatusItem(consumer);
        // }
        GameObject batteryStatusItem = Instantiate(batteryStatusItemPrefab, batteryStatusItemParent);
        batteryStatusItems.Add(consumerId, batteryStatusItem); 
        batteryStatusItem.GetComponent<BattaryStatusItem>().Init(consumer);
    }

    // 총 배터리 레벨 업데이트 메서드
    public void UpdateTotalBatteryLevel(float level)
    {
        centerBatteryLevelText.text = $"{level:F1}";
    }

    // 플레이어의 HP 상태바 업데이트 메서드
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth; // HP 비율에 따라 슬라이더 값 설정
        }
    }

    // 플레이어의 스태미나 상태바 업데이트 메서드
    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina; // 스태미나 비율에 따라 슬라이더 값 설정
        }
    }

    // CCTV 버튼 초기화 메서드
    public void InitCCTVButton()
    {
        GameObject tempButton = Instantiate(cctvButtonPrefab, cctvButtonParent); // CCTV 버튼 생성
        cctvButtons.Add(tempButton); // CCTV 버튼 리스트에 추가
    }
    public void RemoveCCTVButton(GameObject button)
    {
        if(cctvButtons.Count > 0)
        {
            cctvButtons.Remove(button);
            Destroy(button);
        }
    }

    // 오브젝트 활성화/비활성화 메서드
    public void ToggleObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(!obj.activeSelf); // 오브젝트의 활성 상태 토글
        }
    }

    // 커서의 가시성 토글 메서드
    public void ToggleCursor(bool isVisible)
    {
        var player = GameManager.instance.currentPlayer;
        player.firstPersonController._input.cursorLocked = isVisible; // 커서 잠금 상태 설정
        player.firstPersonController._input.cursorInputForLook = isVisible; // 커서 입력 여부 설정
        Cursor.visible = !isVisible; // 커서 가시성 설정
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Mouse.current.WarpCursorPosition(screenCenter); // 커서를 화면 중앙으로 이동
    }

    // 게임 오버 화면 표시 메서드
    public void ShowGameOver()
    {
        gameOverImage.SetActive(true); // 게임 오버 이미지 활성화
        FadeManager.instance.FadeIn(gameOverImage, 2f); // 게임 오버 이미지 페이드 인
    }

    // 게임 오버 화면 숨기기 메서드
    public void HideGameOver()
    {
        FadeManager.instance.FadeOut(gameOverImage, 2f); // 게임 오버 이미지 페이드 아웃
    }

    public void UpdateStatusBarTimeText(string time)
    {
        statusBarTimeText.text = time;
    }

    

    /// <summary>
    /// CCTV용 배터리 상태 UI 아이템을 생성하는 함수
    /// </summary>
    /// <param name="prefab">배터리 상태 UI 프리팹</param>
    /// <param name="cctv">연결할 CCTV 객체</param>
    /// <returns>생성된 배터리 상태 UI GameObject</returns>
    public GameObject CreateBatteryStatusItem(GameObject prefab, CCTV cctv)
    {
        if (prefab == null)
        {
            Debug.LogWarning("배터리 상태 UI 프리팹이 null입니다.");
            return null;
        }

        // 배터리 상태 UI 아이템 생성
        GameObject batteryStatusItem = Instantiate(prefab, transform);
        
        // 배터리 상태 UI 아이템 설정
        // 예: 이름 설정, 위치 조정 등
        batteryStatusItem.name = $"BatteryStatus_CCTV_{cctv.gameObject.name}";
        
        // 여기에 배터리 UI 컴포넌트 참조 설정 및 초기화 코드 추가
        // 예: batteryStatusItem.GetComponent<BatteryStatusUI>().SetCCTV(cctv);
        
        return batteryStatusItem;
    }
}



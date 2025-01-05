using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{

    public static UIManager instance; // 싱글톤 인스턴스
    public List<GameObject> unActiveObjects = new List<GameObject>();
    [Header("소비자 UI")]
    public TMP_Text consumerInfoText; // 소비자 정보 표시 텍스트 (TMP 사용)
    public TMP_Text totalConsumersText; // 전체 소비자 수 텍스트 (TMP 사용)
    public TMP_Text totalBatteryLevelText; // 총 배터리 레벨 표시 텍스트 (TMP 사용)
    public TMP_Text currentTimeText; // 현재 시간 표시 텍스트 (TMP 사용)

    [Header("")]
    public TMP_Text totalGoldText; // 총 골드 텍스트

    [Header("Player Status UI")]
    public Slider healthBar; // HP 상태바 슬라이더
    public Slider staminaBar; // 스태미나 상태바 슬라이더

    public GameObject gameOverImage; // 게임 오버 이미지 오브젝트
    public GameObject computerView; // 컴퓨터 뷰 오브젝트

    public GameObject cctvButtonPrefab; // CCTV 버튼 프리팹
    [SerializeField] private Transform cctvButtonParent; // CCTV 버튼 부모 트랜스폼
    public List<GameObject> cctvButtons = new List<GameObject>(); // 생성된 CCTV 버튼 리스트
    public TMP_Text screenDayText;


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

    // 현재 시간을 텍스트로 업데이트하는 코루틴
    IEnumerator UpdateTimeText()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f); // 1초 대기
            currentTimeText.text = TimeManager.instance.GetCurrentGameTime(); // 현재 게임 시간 업데이트
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
        if(currentTimeText != null)
        {
            StartCoroutine(UpdateTimeText()); // 시간 업데이트 코루틴 시작
        }
        foreach(var obj in unActiveObjects)
        {
            obj.SetActive(false);
        }
    }
    
    private void Update()
    {
        // 현재 사용되지 않는 Update 메서드
    }

    // 총 골드 텍스트 업데이트 메서드
    public void UpdateGold(int totalGold)
    {
        // totalGoldText.text = $"Total Gold: {totalGold}";
    }

    // 소비자 정보 추가 메서드
    public void AddConsumerInfo(string info)
    {
        consumerInfoText.text += info + "\n"; // 새로운 소비자 정보 추가
    }

    // 소비자 정보 초기화 메서드
    public void ClearConsumerInfo()
    {
        consumerInfoText.text = string.Empty; // 소비자 정보 텍스트 초기화
    }

    // 전체 소비자 수 업데이트 메서드
    public void UpdateTotalConsumers(int count)
    {
        totalConsumersText.text = $"Total Consumers: {count}";
    }

    // 총 배터리 레벨 업데이트 메서드
    public void UpdateTotalBatteryLevel(float level)
    {
        totalBatteryLevelText.text = $"Total Battery Level: {level:F2}";
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

    // 예시: 씬 전환 시 전체 화면 페이드 인/아웃 사용
    public void TransitionToScene(string sceneName)
    {
        FadeManager.instance.FadeInThenFadeOut(1f, 2f, 1f); // 페이드 인 후 2초 대기 후 페이드 아웃
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return new WaitForSeconds(1f); // 페이드 인 완료 대기
        // 씬 로드 로직 추가 (예: UnityEngine.SceneManagement.SceneManager.LoadScene)
        // 예시:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(1f); // 페이드 아웃 준비 대기
        FadeManager.instance.ScreenFadeOut(1f); // 페이드 아웃 시작
    }
}



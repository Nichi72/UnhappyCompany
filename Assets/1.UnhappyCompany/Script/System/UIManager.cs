using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [Header("소비자 UI")]
    public TMP_Text consumerInfoText; // 소비자 정보 표시 텍스트 (TMP 사용)
    public TMP_Text totalConsumersText; // 전체 소비자 수 텍스트 (TMP 사용)
    public TMP_Text totalBatteryLevelText; // 총 배터리 레벨 표시 텍스트 (TMP 사용)
    public TMP_Text currentTimeText; // 현재 시간 표시 텍스트 (TMP 사용)
    [Header("")]
    public TMP_Text totalGoldText;
    [Header("Player Status UI")]
    public Slider healthBar; // HP 상태바 슬라이더
    public Slider staminaBar; // 스태미나 상태바 슬라이더

    public GameObject gameOverImage;
    public GameObject computerView;

    public GameObject cctvButtonPrefab;
    [SerializeField] private Transform cctvButtonParent;
    public List<GameObject> cctvButtons = new List<GameObject>();

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator UpdateTimeText()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            currentTimeText.text = TimeManager.instance.GetCurrentGameTime();
        }
    }

    private void Start()
    {
        // 상태바 초기화
        if (healthBar != null)
        {
            healthBar.fillRect.GetComponent<Image>().color = Color.red;
        }
        if (staminaBar != null)
        {
            staminaBar.fillRect.GetComponent<Image>().color = Color.yellow;
        }
        if(currentTimeText != null)
        {
            StartCoroutine(UpdateTimeText());
        }
    }
    
    private void Update()
    {
        
    }

    public void UpdateGold(int totalGold)
    {
        totalGoldText.text = $"Total Gold: {totalGold}";
    }

    // 소비자 정보 추가 메서드
    public void AddConsumerInfo(string info)
    {
        consumerInfoText.text += info + "\n";
    }

    // 소비자 정보 초기화 메서드
    public void ClearConsumerInfo()
    {
        consumerInfoText.text = string.Empty;
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
            healthBar.value = currentHealth / maxHealth;
        }
    }

    // 플레이어의 스태미나 상태바 업데이트 메서드
    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
    }

    public void InitCCTVButton()
    {
        GameObject tempButton = Instantiate(cctvButtonPrefab, cctvButtonParent);
        cctvButtons.Add(tempButton);
    }
    // 오브젝트 활성화/비활성화 메서드
    public void ToggleObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }
}



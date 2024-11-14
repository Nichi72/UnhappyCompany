using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text consumerInfoText; // 각 개체의 전력 소모 정보 (TMP로 변경)
    public TMP_Text totalConsumersText; // 전체 개체 수 (TMP로 변경)
    public TMP_Text totalBatteryLevelText; // 중앙 배터리의 전력 소모량 (TMP로 변경)

    [Header("Player Status UI")]
    public Slider healthBar; // HP 게이지 바
    public Slider staminaBar; // 스테미나 게이지 바

    public GameObject gameOverImage;
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 초기 색상 설정
        if (healthBar != null)
        {
            healthBar.fillRect.GetComponent<Image>().color = Color.red;
        }
        if (staminaBar != null)
        {
            staminaBar.fillRect.GetComponent<Image>().color = Color.yellow;
        }
    }

    // 소비자 정보를 추가하는 메서드
    public void AddConsumerInfo(string info)
    {
        consumerInfoText.text += info + "\n";
    }

    // 소비자 정보를 초기화하는 메서드
    public void ClearConsumerInfo()
    {
        consumerInfoText.text = string.Empty;
    }

    // 총 개체 수를 업데이트하는 메서드
    public void UpdateTotalConsumers(int count)
    {
        totalConsumersText.text = $"Total Consumers: {count}";
    }

    // 중앙 배터리 전력 레벨을 업데이트하는 메서드
    public void UpdateTotalBatteryLevel(float level)
    {
        totalBatteryLevelText.text = $"Total Battery Level: {level:F2}";
    }

    // 플레이어의 HP를 업데이트하는 메서드
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    // 플레이어의 스테미나를 업데이트하는 메서드
    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
    }
}



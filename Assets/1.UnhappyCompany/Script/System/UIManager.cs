using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text consumerInfoText; // �� ��ü�� ���� �Ҹ� ���� (TMP�� ����)
    public TMP_Text totalConsumersText; // ��ü ��ü �� (TMP�� ����)
    public TMP_Text totalBatteryLevelText; // �߾� ���͸��� ���� �Ҹ� (TMP�� ����)

    [Header("Player Status UI")]
    public Slider healthBar; // HP ������ ��
    public Slider staminaBar; // ���׹̳� ������ ��

    public GameObject gameOverImage;
    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
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
        // �ʱ� ���� ����
        if (healthBar != null)
        {
            healthBar.fillRect.GetComponent<Image>().color = Color.red;
        }
        if (staminaBar != null)
        {
            staminaBar.fillRect.GetComponent<Image>().color = Color.yellow;
        }
    }

    // �Һ��� ������ �߰��ϴ� �޼���
    public void AddConsumerInfo(string info)
    {
        consumerInfoText.text += info + "\n";
    }

    // �Һ��� ������ �ʱ�ȭ�ϴ� �޼���
    public void ClearConsumerInfo()
    {
        consumerInfoText.text = string.Empty;
    }

    // �� ��ü ���� ������Ʈ�ϴ� �޼���
    public void UpdateTotalConsumers(int count)
    {
        totalConsumersText.text = $"Total Consumers: {count}";
    }

    // �߾� ���͸� ���� ������ ������Ʈ�ϴ� �޼���
    public void UpdateTotalBatteryLevel(float level)
    {
        totalBatteryLevelText.text = $"Total Battery Level: {level:F2}";
    }

    // �÷��̾��� HP�� ������Ʈ�ϴ� �޼���
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    // �÷��̾��� ���׹̳��� ������Ʈ�ϴ� �޼���
    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
    }
}



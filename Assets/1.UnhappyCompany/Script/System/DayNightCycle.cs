using UnityEngine;
using System;

public class DayNightCycle : MonoBehaviour
{
    public float realTimeHourLength = 1f; // ������ 1�ð� ���� ���� �� 1�ð��� �������� ���� (N�ð��� �ǹ�)
    public float gameHoursPerRealHour = 24f; // ���� �ð����� �� �ð� ���� ���� �� �Ϸ簡 ������ �ϴ��� ����

    private float timeMultiplier; // ���� �ð��� ���� �ð��� ������ ���
    private float currentGameTime; // ���� ���� �� �ð� (0 ~ 24)
    private bool isTimePaused = false; // �ð��� ���ߴ��� ����

    public event Action OnDayStart; // ���� ���۵� �� �̺�Ʈ
    public event Action OnNightStart; // ���� ���۵� �� �̺�Ʈ
    public event Action OnDayToNight; // ������ ������ �ٲ� �� �̺�Ʈ
    public event Action OnNightToDay; // �㿡�� ������ �ٲ� �� �̺�Ʈ

    private bool isDaytime;

    void Start()
    {
        timeMultiplier = 24f / gameHoursPerRealHour; // ���� �� �Ϸ簡 ������ �ϴ� �ð� ����
        currentGameTime = 0f; // �������� ����
        isDaytime = currentGameTime >= 6f && currentGameTime < 18f;
    }

    void Update()
    {
        if (!isTimePaused)
        {
            UpdateGameTime();
        }
    }

    private void UpdateGameTime()
    {
        // ���� �ð��� ����� ���� ���� �ð��� ������Ʈ�մϴ�.
        currentGameTime += Time.deltaTime / (realTimeHourLength * 3600f) * 24f;

        // �Ϸ簡 ������ �ð��� �ٽ� �����մϴ�.
        if (currentGameTime >= 24f)
        {
            currentGameTime = 0f;
        }

        UpdateDayNightCycle();
    }

    private void UpdateDayNightCycle()
    {
        bool wasDaytime = isDaytime;
        isDaytime = currentGameTime >= 6f && currentGameTime < 18f;

        if (isDaytime)
        {
            // ���� ����� ó��
            Debug.Log("���� �ð��� ���Դϴ�.");
            if (!wasDaytime)
            {
                OnNightToDay?.Invoke();
                OnDayStart?.Invoke();
            }
        }
        else
        {
            // ���� ����� ó��
            Debug.Log("���� �ð��� ���Դϴ�.");
            if (wasDaytime)
            {
                OnDayToNight?.Invoke();
                OnNightStart?.Invoke();
            }
        }
    }

    public void PauseTime()
    {
        isTimePaused = true;
    }

    public void ResumeTime()
    {
        isTimePaused = false;
    }
}

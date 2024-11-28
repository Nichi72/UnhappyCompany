using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("플레이어 상태")]
    [Header("최대 체력")] 
    public float MaxHealth = 100.0f;
    [Header("최대 스태미나")] 
    public float MaxStamina = 100.0f;
    [Header("스태미나 회복 속도 (초당 회복량)")] 
    public float StaminaRecoveryRate = 5.0f;
    [Header("달리기 시 스태미나 감소량")] 
    public float StaminaReduce = 30;
    [Header("점프 시 스태미나 감소량")] 
    public float StaminaJumpReduce = 10;
    [Header("달리기 또는 점프를 위한 최소 스태미나")] 
    public float StaminaThresholdToRunOrJump = 20.0f;

    [ReadOnly] [SerializeField] private float _currentHealth;
    [ReadOnly] [SerializeField] private float _currentStamina;
    [ReadOnly] [SerializeField] private bool _isConsumingStamina;
    [ReadOnly] [SerializeField] private bool _canRunOrJump;

    public float CurrentHealth { get => _currentHealth; set => _currentHealth = value; }

    private void Awake()
    {
        _currentHealth = MaxHealth;
        _currentStamina = MaxStamina;
        _canRunOrJump = true;

        // UI 초기화
        UIManager.instance.UpdateHealthBar(_currentHealth, MaxHealth);
        UIManager.instance.UpdateStaminaBar(_currentStamina, MaxStamina);
    }
    private void Start()
    {
       
    }

    private void Update()
    {
        if (!_isConsumingStamina)
        {
            RecoverStamina();
        }
        else if (_currentStamina <= 0)
        {
            _canRunOrJump = false;
        }
        if (_currentStamina >= StaminaThresholdToRunOrJump)
        {
            _canRunOrJump = true;
        }

        UIManager.instance.UpdateStaminaBar(_currentStamina, MaxStamina);
    }

    public void ReduceHealth(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
        UIManager.instance.UpdateHealthBar(_currentHealth, MaxHealth);

        if (_currentHealth <= 0)
        {
            // Handle player death here
            Debug.Log("Player is dead.");
        }
    }

    public void ReduceStamina(float amount)
    {
        _currentStamina -= amount;
        _currentStamina = Mathf.Clamp(_currentStamina, 0, MaxStamina);
        _isConsumingStamina = true;
        UIManager.instance.UpdateStaminaBar(_currentStamina, MaxStamina);
    }

    public void StopConsumingStamina()
    {
        _isConsumingStamina = false;
    }

    public bool CanRunOrJump()
    {
        return _canRunOrJump;
    }

    private void RecoverStamina()
    {
        if (_currentStamina < MaxStamina)
        {
            _currentStamina += StaminaRecoveryRate * Time.deltaTime;
            _currentStamina = Mathf.Clamp(_currentStamina, 0, MaxStamina);
        }
    }
}
using UnityEngine;

public class PlayerStatus : MonoBehaviour , IDamageable
{
    [Header("�÷��̾� ����")]
    [Header("�ִ� ü��")] 
    public float MaxHealth = 100.0f;
    [Header("�ִ� ���¹̳�")] 
    public float MaxStamina = 100.0f;
    [Header("���¹̳� ȸ�� �ӵ� (�ʴ� ȸ����)")] 
    public float StaminaRecoveryRate = 5.0f;
    [Header("�޸��� �� ���¹̳� ���ҷ�")] 
    public float StaminaReduce = 30;
    [Header("���� �� ���¹̳� ���ҷ�")] 
    public float StaminaJumpReduce = 10;
    [Header("�޸��� �Ǵ� ������ ���� �ּ� ���¹̳�")] 
    public float StaminaThresholdToRunOrJump = 20.0f;

    [ReadOnly] [SerializeField] private float _currentHealth;
    [ReadOnly] [SerializeField] private float _currentStamina;
    [ReadOnly] [SerializeField] private bool _isConsumingStamina;
    [ReadOnly] [SerializeField] private bool _canRunOrJump;

    public float CurrentHealth { get => _currentHealth; set => _currentHealth = value; }
    
    // IDamageable 인터페이스 구현
    public int hp { get => (int)_currentHealth; set => _currentHealth = value; }

    public void TakeDamage(int damage, DamageType damageType)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
        UIManager.instance.UpdateHealthBar(_currentHealth, MaxHealth);

        Debug.Log($"Player {damage}의 피해 입음 남은 체력:{hp}");
        
        if (_currentHealth <= 0)
        {
            // Handle player death here
            Debug.Log("Player 사망");
        }
    }

    private void Awake()
    {
        _currentHealth = MaxHealth;
        _currentStamina = MaxStamina;
        _canRunOrJump = true;

        // UI �ʱ�ȭ
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
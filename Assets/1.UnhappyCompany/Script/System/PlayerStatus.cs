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

    [Header("Player Reference")]
    private Player _player;
    private StarterAssets.FirstPersonController _firstPersonController;

    public float CurrentHealth { get => _currentHealth; set => _currentHealth = value; }
    
    /// <summary>
    /// 플레이어가 현재 달리는 중인지 여부
    /// </summary>
    public bool IsCurrentRun
    {
        get
        {
            if (_firstPersonController == null) return false;
            return _firstPersonController._input != null && _firstPersonController._input.sprint;
        }
    }

    /// <summary>
    /// 플레이어가 현재 공중에 있는지 (점프 중인지) 여부
    /// </summary>
    public bool IsCurrentJump
    {
        get
        {
            if (_firstPersonController == null) return false;
            return !_firstPersonController.Grounded;
        }
    }

    /// <summary>
    /// 플레이어가 현재 걷는 중인지 여부 (이동 중이지만 달리지 않음)
    /// </summary>
    public bool IsCurrentWalk
    {
        get
        {
            if (_firstPersonController == null) return false;
            if (_firstPersonController._input == null) return false;
            
            // 이동 입력이 있고, 달리지 않고 있으며, 지면에 있을 때
            bool hasMovementInput = _firstPersonController._input.move.sqrMagnitude > 0.01f;
            bool isNotSprinting = !_firstPersonController._input.sprint;
            bool isGrounded = _firstPersonController.Grounded;
            
            return hasMovementInput && isNotSprinting && isGrounded;
        }
    }

    /// <summary>
    /// 플레이어가 이동 중인지 여부 (걷기 또는 달리기)
    /// </summary>
    public bool IsMoving
    {
        get
        {
            if (_firstPersonController == null) return false;
            if (_firstPersonController._input == null) return false;
            
            return _firstPersonController._input.move.sqrMagnitude > 0.01f;
        }
    }
    
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

        // Player 및 FirstPersonController 참조 초기화
        _player = GetComponent<Player>();
        if (_player != null)
        {
            _firstPersonController = _player.firstPersonController;
        }
        else
        {
            Debug.LogWarning("PlayerStatus: Player 컴포넌트를 찾을 수 없습니다.");
        }

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
using UnityEngine;

/// <summary>
/// 레이캐스트를 사용하여 땅과의 접촉을 감지하는 컴포넌트입니다.
/// 큐브가 굴러다니면서 바닥에 닿는 순간을 감지하기 위해 사용됩니다.
/// </summary>
public class GroundContactRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("레이캐스트 방향 (로컬 좌표계)")]
    [SerializeField] private Vector3 rayDirection = Vector3.down;
    
    [Tooltip("레이캐스트 거리")]
    [SerializeField] private float rayDistance = 0.6f;
    
    [Tooltip("땅으로 인식할 레이어 (Ground 레이어만 선택 권장)")]
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Contact Detection")]
    [Tooltip("접촉 감지 쿨다운 (초) - 너무 자주 발동되지 않도록")]
    [SerializeField] private float contactCooldown = 0.1f;
    
    [Tooltip("최소 속도 - 이 속도 이하에서는 접촉 감지 안함")]
    [SerializeField] private float minVelocityThreshold = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private Color rayColorNoHit = Color.green;
    [SerializeField] private Color rayColorHit = Color.red;
    
    // 상태
    private bool wasGrounded = false;
    private float lastContactTime = -999f;
    private Rigidbody rb;
    
    // 접촉 이벤트
    public System.Action OnGroundContact;
    
    // 속성
    public bool IsGrounded { get; private set; }
    public RaycastHit LastHit { get; private set; }
    public float CurrentVelocity => rb != null ? rb.linearVelocity.magnitude : 0f;
    
    private void Awake()
    {
        // Rigidbody는 부모에서 찾기
        rb = GetComponentInParent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogWarning($"[GroundContactRaycast] {gameObject.name}: Rigidbody를 찾을 수 없습니다. 속도 체크가 작동하지 않습니다.", this);
        }
    }
    
    private void FixedUpdate()
    {
        CheckGroundContact();
    }
    
    /// <summary>
    /// 땅과의 접촉을 체크합니다.
    /// </summary>
    private void CheckGroundContact()
    {
        // 월드 좌표계로 방향 변환
        Vector3 worldRayDirection = transform.TransformDirection(rayDirection.normalized);
        
        // 레이캐스트 발사 (groundLayer 필터링으로 이미 땅만 감지됨)
        RaycastHit[] hits = Physics.RaycastAll(transform.position, worldRayDirection, rayDistance, groundLayer);
        
        bool hitGround = hits.Length > 0;
        IsGrounded = hitGround;
        
        if (hitGround)
        {
            // 가장 가까운 히트 찾기
            RaycastHit closestHit = hits[0];
            float closestDistance = closestHit.distance;
            
            for (int i = 1; i < hits.Length; i++)
            {
                if (hits[i].distance < closestDistance)
                {
                    closestHit = hits[i];
                    closestDistance = hits[i].distance;
                }
            }
            
            LastHit = closestHit;
            
            // 이전 프레임에서 땅에 닿지 않았고, 지금 닿았으면 -> 접촉 발생
            if (!wasGrounded && CanTriggerContact())
            {
                TriggerContact();
            }
        }
        
        wasGrounded = hitGround;
    }
    
    /// <summary>
    /// 접촉 이벤트를 발동할 수 있는지 확인합니다.
    /// </summary>
    private bool CanTriggerContact()
    {
        // 쿨다운 체크
        if (Time.time - lastContactTime < contactCooldown)
        {
            return false;
        }
        
        // 속도 체크 (너무 느리면 발동 안함)
        if (rb != null && rb.linearVelocity.magnitude < minVelocityThreshold)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 접촉 이벤트를 발동합니다.
    /// </summary>
    private void TriggerContact()
    {
        Debug.Log($"[GroundContactRaycast] 접촉이 감지되었습니다.");
        lastContactTime = Time.time;
        OnGroundContact?.Invoke();
    }
    
    /// <summary>
    /// Gizmos로 레이캐스트 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugRay) return;
        
        Vector3 worldRayDirection = Application.isPlaying 
            ? transform.TransformDirection(rayDirection.normalized) 
            : transform.TransformDirection(rayDirection.normalized);
        
        Gizmos.color = IsGrounded ? rayColorHit : rayColorNoHit;
        Gizmos.DrawLine(transform.position, transform.position + worldRayDirection * rayDistance);
        
        // 레이캐스트 끝점에 작은 구 그리기
        Gizmos.DrawWireSphere(transform.position + worldRayDirection * rayDistance, 0.05f);
        
        // 접촉 지점 표시
        if (IsGrounded && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(LastHit.point, 0.1f);
        }
    }
}


using UnityEngine;

/// <summary>
/// 기본 시설물(Built-in Facility)의 베이스 클래스
/// 센터 배터리, 발전기 등의 시설물이 상속받습니다.
/// </summary>
public abstract class BuiltInFacilityBase : MonoBehaviour, IScannable
{
    [Header("Facility Info")]
    [SerializeField] protected string facilityName = "Facility";
    [SerializeField] protected string facilityDescription = "Built-in Facility";
    
    #region IScannable Implementation
    
    /// <summary>
    /// 스캔 시 표시될 시설물 이름을 반환합니다.
    /// 자식 클래스에서 오버라이드하여 커스터마이즈 가능
    /// </summary>
    public virtual string GetScanName()
    {
        return facilityName;
    }

    /// <summary>
    /// 스캔 시 표시될 시설물 상세 정보를 반환합니다.
    /// 자식 클래스에서 오버라이드하여 추가 정보 표시 가능
    /// </summary>
    public virtual string GetScanDescription()
    {
        return facilityDescription;
    }

    /// <summary>
    /// UI 위치 참조를 위한 Transform 반환
    /// </summary>
    public Transform GetTransform()
    {
        return transform;
    }

    /// <summary>
    /// 스캔 UI 타입 반환
    /// 기본적으로 CollectibleItem 타입을 사용하지만,
    /// 나중에 새로운 Facility 타입을 추가할 수 있습니다.
    /// </summary>
    public virtual EObjectTrackerUIType GetUIType()
    {
        // TODO: 나중에 EObjectTrackerUIType.Facility 추가 고려
        return EObjectTrackerUIType.CollectibleItem; // 임시로 사용
    }

    /// <summary>
    /// 시설물이 스캔되었을 때 호출되는 메서드
    /// 자식 클래스에서 오버라이드하여 스캔 반응 추가 가능
    /// </summary>
    public virtual void OnScanned()
    {
        Debug.Log($"[Facility] {GetScanName()} 스캔됨!");
        // 기본 동작: 스캔 효과음이나 이펙트를 여기에 추가 가능
    }
    
    #endregion
}


using UnityEngine;

public class CollectibleItem : Item, IScannable
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    #region IScannable Implementation
    
    public string GetScanName()
    {
        // ItemData가 있으면 해당 이름 사용, 없으면 GameObject 이름 사용
        if (itemData != null)
        {
            return itemData.itemName;
        }
        return $"[TEST] {gameObject.name}";
    }

    public string GetScanDescription()
    {
        if (itemData != null)
        {
            return $"Value: {itemData.SellPrice}G";
        }
        // 테스트용: ItemData 없을 때도 작동
        return $"Test Collectible Item | Position: {transform.position}";
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public EObjectTrackerUIType GetUIType()
    {
        return EObjectTrackerUIType.CollectibleItem;
    }

    public void OnScanned()
    {
        // 수집 아이템 스캔 시 추가 동작이 필요하면 여기에 구현
        if (itemData != null)
        {
            Debug.Log($"[CollectibleItem] {GetScanName()} 스캔됨 - 가치: {itemData.SellPrice}G");
        }
        else
        {
            Debug.Log($"[CollectibleItem - TEST MODE] {gameObject.name} 스캔됨! ItemData가 없지만 스캔은 작동합니다.");
        }
    }
    
    #endregion
}

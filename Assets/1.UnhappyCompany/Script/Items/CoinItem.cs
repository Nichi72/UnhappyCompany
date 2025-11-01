using UnityEngine;

/// <summary>
/// RSP 게임에서 사용되는 코인 아이템
/// RSP 게임 시작 시 소모되며, 승리 시 리워드로 생성됩니다.
/// </summary>
public class CoinItem : CollectibleItem
{
    [Header("코인 설정")]
    [Tooltip("이 코인의 가치 (기본값은 ItemData의 SellPrice 사용)")]
    [SerializeField] private int coinValue = 1;
    
    /// <summary>
    /// 코인의 가치를 반환합니다
    /// </summary>
    public int GetCoinValue()
    {
        // ItemData가 있으면 SellPrice를 사용, 없으면 coinValue 사용
        if (itemData != null)
        {
            return itemData.SellPrice;
        }
        return coinValue;
    }
    
    /// <summary>
    /// 코인 가치 설정 (동적 생성 시 사용)
    /// </summary>
    public void SetCoinValue(int value)
    {
        coinValue = value;
    }
}


using UnityEngine;
using System;

[Serializable]
public class SavableItemData
{
    [SerializeField] public int itemID;

    // 이미 주워진 아이템인지 여부 (true면 월드 스폰 생략)
    public bool isPickedUp;

    public int GetItemID()
    {
        return itemID;
    }
    public void SetItemID(int id)
    {
        itemID = id;
    }

    public string itemType;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public float durability; // 내구도

    public SavableItemData() {}

    public SavableItemData(int itemID , Vector3 position , Quaternion rotation , Vector3 scale)
    {
        this.itemID = itemID;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        durability = 100;
        isPickedUp = false;
    }
}
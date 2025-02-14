using UnityEngine;
using System;

[Serializable]
public class SavableItemData
{
    private int itemID;

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

    public SavableItemData()
    {
        itemType = "";
    }
}
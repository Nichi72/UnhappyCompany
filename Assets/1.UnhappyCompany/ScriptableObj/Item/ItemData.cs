using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName; // 아이템 이름
    public float weight; // 아이템 무게
    public Sprite icon; // 아이템 아이콘
    public GameObject prefab;
}

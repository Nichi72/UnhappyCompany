using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    //Game Data
    public string itemName; // ������ �̸�
    public float weight; // ������ ����
    public int SellPrice;
    public int BuyPrice;

    //Game Obj Data
    public Sprite icon; // ������ ������
    public GameObject prefab;
   
}

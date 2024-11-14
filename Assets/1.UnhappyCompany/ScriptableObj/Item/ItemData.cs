using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName; // ������ �̸�
    public float weight; // ������ ����
    public Sprite icon; // ������ ������
    public GameObject prefab;
}

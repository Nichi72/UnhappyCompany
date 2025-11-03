using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 아이템의 스택 타입을 정의합니다.
/// </summary>
public enum ItemStackType
{
    Single = 1,        // 일반 아이템 (무기, 도구 등) - 스택 불가
    Small = 9,         // 소모품 (코인 등)
    Medium = 99,       // 중형 스택
    Large = 999        // 대형 스택
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField, ReadOnly]
    public string itemName; 
    public float weight; 
    public int SellPrice;
    public int BuyPrice;

    [Header("Stack Settings")]
    [Tooltip("아이템의 스택 타입을 설정합니다. Single은 스택 불가, Small/Medium/Large는 스택 가능")]
    public ItemStackType stackType = ItemStackType.Single;

    //Game Obj Data
    public Sprite icon; 
    public GameObject prefab;

    public Vector3 ItemPosition;
    public Vector3 ItemRotation;
    public Vector3 ItemScale;
    // 세이브 데이터
    public SavableItemData savableItemData;



    private void OnEnable()
    {
        if (savableItemData.GetItemID() == 0)
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            // 파일 이름에서 ID 추출
            int extractedID = ExtractIDFromFileName(fileName);

            if (extractedID > 0)
            {
                savableItemData.SetItemID(extractedID);
            }
            else
            {
                // 새로운 ID 할당
                int newID = GenerateNewID();
                savableItemData.SetItemID(newID);
                // 파일 이름에 ID 추가
                RenameAssetWithID(assetPath, newID);
            }
        }
    }

    private int ExtractIDFromFileName(string fileName)
    {
        string[] parts = fileName.Split('_');
        if (parts.Length > 2 && parts[0] == "ItemData" && int.TryParse(parts[1], out int id))
        {
            return id;
        }
        return 0;
    }

    private int GenerateNewID()
    {
        // 모든 ItemData 파일을 검색하여 가장 큰 ID를 찾음
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        int maxID = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            int id = ExtractIDFromFileName(name);
            if (id > maxID)
            {
                maxID = id;
            }
        }

        return maxID + 1;
    }

    private void RenameAssetWithID(string assetPath, int id)
    {
        string newFileName = $"ItemData_{id}_{itemName}";
        string newPath = Path.Combine(Path.GetDirectoryName(assetPath), newFileName);
        AssetDatabase.RenameAsset(assetPath, newFileName);
        AssetDatabase.SaveAssets();
    }
}

using UnityEngine;
using UnityEditor;
using System.IO;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField, ReadOnly]
    private int itemID;
    public int ItemID => itemID; // 외부에서 읽기만 가능

    public string itemName; 
    public float weight; 
    public int SellPrice;
    public int BuyPrice;

    //Game Obj Data
    public Sprite icon; 
    public GameObject prefab;

    private void OnEnable()
    {
        if (itemID == 0)
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            // 파일 이름에서 ID 추출
            int extractedID = ExtractIDFromFileName(fileName);

            if (extractedID > 0)
            {
                itemID = extractedID;
            }
            else
            {
                // 새로운 ID 할당
                itemID = GenerateNewID();
                // 파일 이름에 ID 추가
                RenameAssetWithID(assetPath, itemID);
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

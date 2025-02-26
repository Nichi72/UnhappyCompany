using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class ItemLoader : MonoBehaviour
{
    [Header("Item Prefab List")]
    public List<ItemData> itemDataList; 
    // 저장된 ID(아이템 종류) -> 어떤 ItemData를 쓸지 매핑하는 용도.
    // 예: itemDataList에 있는 각 아이템의 savableItemData.GetItemID()와 매칭

    public void LoadItemsFromSaveFile(string saveFileName)
    {
        if(!ES3.FileExists(saveFileName))
        {
            Debug.LogWarning($"{saveFileName} 파일이 없습니다.");
            return;
        }
        // (씬 아이템을 보존할지, 제거할지 상황에 맞게 결정)
        // ClearSceneItems(); // 만약 기존 월드 오브젝트 초기화가 필요하다면 사용
        var allKeys = ES3.GetKeys(saveFileName);
        var itemKeys = allKeys.Where(k => k.StartsWith("ItemInstance_"));

        // 씬에 이미 있는 아이템 Dictionary(고유 ID => Item) 파악
        Dictionary<string, Item> existingItemsDict = CollectSceneItems();

        foreach (var key in itemKeys)
        {
            SavableItemData loadedData = ES3.Load<SavableItemData>(key, saveFileName);

            // 이미 isPickedUp == true라면, 이 아이템은 월드에 나타나지 않도록 스킵
            if(loadedData.isPickedUp)
            {
                Debug.Log($"로드: {key} 아이템은 이미 주워진 상태이므로 월드 스폰 생략");
                continue;
            }

            string uniqueID = key.Substring("ItemInstance_".Length);
            int itemDataID = loadedData.GetItemID();
            ItemData matchedData = itemDataList.FirstOrDefault(x => x.savableItemData.GetItemID() == itemDataID);

            if(matchedData == null)
            {
                Debug.LogWarning($"{key} 로드 실패 - itemDataList에 ID({itemDataID})가 없습니다.");
                continue;
            }

            // 중복 체크(씬에 이미 존재하는 아이템이면 transform만 갱신)
            if(existingItemsDict.TryGetValue(uniqueID, out Item sceneItem))
            {
                sceneItem.transform.position = loadedData.position;
                sceneItem.transform.rotation = loadedData.rotation;
                sceneItem.transform.localScale = loadedData.scale;
                Debug.Log($"기존 아이템({uniqueID})의 상태 갱신 (isPickedUp == false)");
            }
            else
            {
                // 새로 Instantiate
                GameObject newItemObj = Instantiate(matchedData.prefab);
                newItemObj.name = $"SpawnedItem_{uniqueID}";
                var itemComponent = newItemObj.GetComponent<Item>();
                if(itemComponent != null)
                {
                    itemComponent.itemData = matchedData;
                    itemComponent.AssignUniqueInstanceID(uniqueID);

                    newItemObj.transform.position = loadedData.position;
                    newItemObj.transform.rotation = loadedData.rotation;
                    newItemObj.transform.localScale = loadedData.scale;

                    Debug.Log($"새 아이템({uniqueID}) 생성 (isPickedUp == false)");
                }
            }
        }
        Debug.Log("로드 완료");
    }

    private Dictionary<string, Item> CollectSceneItems()
    {
        Dictionary<string, Item> dict = new Dictionary<string, Item>();
        var existingSceneItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
        foreach(var sceneItem in existingSceneItems)
        {
            string id = sceneItem.GetUniqueInstanceID();
            if(!dict.ContainsKey(id))
            {
                dict.Add(id, sceneItem);
            }
        }
        return dict;
    }

    public void SaveItemsToSaveFile(string saveFileName)
    {
        var sceneItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
        try
        {
            foreach(var item in sceneItems)
            {
                Debug.Log($"아이템 저장 중: {item.gameObject.name}");
                string key = $"ItemInstance_{item.GetUniqueInstanceID()}";
                SavableItemData data = new SavableItemData
                (
                    item.itemData.savableItemData.GetItemID(),
                    item.transform.position,
                    item.transform.rotation,
                    item.transform.localScale
                );
                ES3.Save(key, data, saveFileName);
                Debug.Log($"아이템 데이터 저장 완료: {key}");
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"아이템 저장 중 오류 발생: {e.Message}");
        }
    }

    public void ClearSceneItems()
    {
        var sceneItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
        foreach(var i in sceneItems)
        {
            Destroy(i.gameObject);
        }
        Debug.Log("씬 내 기존 아이템 오브젝트 정리 완료");
    }
} 
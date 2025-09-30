using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemDataEditorWindow : EditorWindow
{
    private Vector2 scrollPos;
    private ItemData[] itemDataArray;
    private SerializedObject[] serializedItemDataArray;
    private string currentSortField = "ItemID";
    private bool ascending = true;

    [MenuItem(Structures.LAB_WATCHER + "/Item Data Viewer")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataEditorWindow>("Item Data Viewer");
    }

    private void OnEnable()
    {
        LoadAndSortData();
    }

    private void LoadAndSortData()
    {
        // Load all ItemData assets
        itemDataArray = Resources.FindObjectsOfTypeAll<ItemData>();

        // Sort based on current field and direction
        itemDataArray = currentSortField switch
        {
            "ItemID" => ascending ? itemDataArray.OrderBy(item => item.savableItemData.GetItemID()).ToArray() : itemDataArray.OrderByDescending(item => item.savableItemData.GetItemID()).ToArray(),
            "ItemName" => ascending ? itemDataArray.OrderBy(item => item.itemName).ToArray() : itemDataArray.OrderByDescending(item => item.itemName).ToArray(),
            "Weight" => ascending ? itemDataArray.OrderBy(item => item.weight).ToArray() : itemDataArray.OrderByDescending(item => item.weight).ToArray(),
            "SellPrice" => ascending ? itemDataArray.OrderBy(item => item.SellPrice).ToArray() : itemDataArray.OrderByDescending(item => item.SellPrice).ToArray(),
            "BuyPrice" => ascending ? itemDataArray.OrderBy(item => item.BuyPrice).ToArray() : itemDataArray.OrderByDescending(item => item.BuyPrice).ToArray(),
            _ => itemDataArray
        };

        serializedItemDataArray = new SerializedObject[itemDataArray.Length];
        for (int i = 0; i < itemDataArray.Length; i++)
        {
            serializedItemDataArray[i] = new SerializedObject(itemDataArray[i]);
        }
    }

    private void OnGUI()
    {
        if (itemDataArray == null || itemDataArray.Length == 0)
        {
            EditorGUILayout.LabelField("No ItemData found.");
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Item ID", GUILayout.Width(50))) ToggleSort("ItemID");
        if (GUILayout.Button("Item Name", GUILayout.Width(100))) ToggleSort("ItemName");
        if (GUILayout.Button("Weight", GUILayout.Width(50))) ToggleSort("Weight");
        if (GUILayout.Button("Sell Price", GUILayout.Width(70))) ToggleSort("SellPrice");
        if (GUILayout.Button("Buy Price", GUILayout.Width(70))) ToggleSort("BuyPrice");
        EditorGUILayout.LabelField("Icon", GUILayout.Width(70));
        EditorGUILayout.LabelField("Prefab", GUILayout.Width(100));
        EditorGUILayout.LabelField("Scriptable Object", GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < itemDataArray.Length; i++)
        {
            var serializedItemData = serializedItemDataArray[i];
            serializedItemData.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(itemDataArray[i].savableItemData.GetItemID().ToString(), GUILayout.Width(50));
            EditorGUILayout.PropertyField(serializedItemData.FindProperty("itemName"), GUIContent.none, GUILayout.Width(100));
            EditorGUILayout.PropertyField(serializedItemData.FindProperty("weight"), GUIContent.none, GUILayout.Width(50));
            EditorGUILayout.PropertyField(serializedItemData.FindProperty("SellPrice"), GUIContent.none, GUILayout.Width(70));
            EditorGUILayout.PropertyField(serializedItemData.FindProperty("BuyPrice"), GUIContent.none, GUILayout.Width(70));
            EditorGUILayout.PropertyField(serializedItemData.FindProperty("icon"), GUIContent.none, GUILayout.Width(70));
            EditorGUILayout.PropertyField(serializedItemData.FindProperty("prefab"), GUIContent.none, GUILayout.Width(100));
            EditorGUILayout.ObjectField(itemDataArray[i], typeof(ItemData), false, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            serializedItemData.ApplyModifiedProperties();
        }

        EditorGUILayout.EndScrollView();

        // 무결성 검사 버튼을 한 줄에 정렬
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check ID Duplicates", GUILayout.Width(150)))
        {
            CheckForDuplicateIDs();
        }

        if (GUILayout.Button("Check Name Duplicates", GUILayout.Width(150)))
        {
            CheckForDuplicateNames();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ToggleSort(string field)
    {
        if (currentSortField == field)
        {
            ascending = !ascending; // Toggle sort direction
        }
        else
        {
            currentSortField = field;
            ascending = true; // Default to ascending when changing field
        }
        LoadAndSortData();
    }

    private void CheckForDuplicateIDs()
    {
        var duplicateIDs = itemDataArray.GroupBy(item => item.savableItemData.GetItemID())
                                        .Where(group => group.Count() > 1)
                                        .Select(group => group.Key)
                                        .ToList();

        if (duplicateIDs.Count > 0)
        {
            Debug.LogWarning("Duplicate IDs found: " + string.Join(", ", duplicateIDs));
        }
        else
        {
            Debug.Log("No duplicate IDs found.");
        }
    }

    private void CheckForDuplicateNames()
    {
        var duplicateNames = itemDataArray.GroupBy(item => item.itemName)
                                          .Where(group => group.Count() > 1)
                                          .Select(group => group.Key)
                                          .ToList();

        if (duplicateNames.Count > 0)
        {
            Debug.LogWarning("Duplicate item names found: " + string.Join(", ", duplicateNames));
        }
        else
        {
            Debug.Log("No duplicate item names found.");
        }
    }
} 
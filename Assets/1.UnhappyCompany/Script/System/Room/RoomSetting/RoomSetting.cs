using System.Collections.Generic;
using UnityEngine;

public class RoomSetting : MonoBehaviour
{
    public List<Transform> eggSpawnPoints;
    public List<Transform> itemSpawnPoints;
    public List<Light> roomLights;
    
    public List<GameObject> doorObjects;
    public List<GameObject> wallObjects;
    public List<GameObject> groundObjects;
    public List<GameObject> otherObjects;

    [ContextMenu("Update Room Lights")]
    private void UpdateRoomLights()
    {
        roomLights = new List<Light>(GetComponentsInChildren<Light>());
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    [ContextMenu("Update Room Objects")]
    private void UpdateRoomObjects()
    {
        doorObjects = new List<GameObject>();
        wallObjects = new List<GameObject>();
        groundObjects = new List<GameObject>();
        otherObjects = new List<GameObject>();

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child == transform) continue; // 자기 자신은 제외

            if (child.gameObject.layer == LayerMask.NameToLayer("Door"))
            {
                doorObjects.Add(child.gameObject);
            }
            else if (child.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                wallObjects.Add(child.gameObject);
            }
            else if (child.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                groundObjects.Add(child.gameObject);
            }
            else
            {
                otherObjects.Add(child.gameObject);
            }
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    [ContextMenu("Toggle Other Objects")]
    private void ToggleOtherObjects()
    {
        if (otherObjects == null || otherObjects.Count == 0)
        {
            Debug.LogWarning("No other objects found. Please run 'Update Room Objects' first.");
            return;
        }

        bool newState = !otherObjects[0].activeSelf;
        foreach (GameObject obj in otherObjects)
        {
            obj.SetActive(newState);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    [ContextMenu("Remove Non-Prefab Objects")]
    private void RemoveNonPrefabObjects()
    {
        if (otherObjects == null || otherObjects.Count == 0)
        {
            Debug.LogWarning("No other objects found. Please run 'Update Room Objects' first.");
            return;
        }

        List<GameObject> prefabObjects = new List<GameObject>();
        foreach (GameObject obj in otherObjects)
        {
            #if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.GetPrefabAssetType(obj) != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                prefabObjects.Add(obj);
            }
            #endif
        }

        otherObjects = prefabObjects;
        Debug.Log($"Removed non-prefab objects. {prefabObjects.Count} prefab objects remaining.");

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}

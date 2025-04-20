using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class RoomSetting : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    public List<Transform> eggSpawnPoints;
    public List<Transform> itemSpawnPoints;
    public List<Light> roomLights;
    
    public List<GameObject> doorObjects;
    public List<GameObject> wallObjects;
    public List<GameObject> groundObjects;
    public List<GameObject> otherObjects;

    public void UpdateRoomLights()
    {
        roomLights = new List<Light>(GetComponentsInChildren<Light>(true));
        
        Debug.Log($"Updated room lights for {gameObject.name}: Found {roomLights.Count} lights.");

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    public void TurnOffAllLights()
    {
        foreach (Light light in roomLights)
        {
            light.enabled = !light.enabled;
        }

        Debug.Log($"Turned off all lights for {gameObject.name}: Found {roomLights.Count} lights.");

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }


    public void UpdateRoomObjects()
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

    public void ToggleOtherObjects()
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

    public void RemoveNonPrefabObjects()
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

    public void FindNavMeshSurface()
    {
        navMeshSurface = GetComponentInChildren<NavMeshSurface>(true); // 비활성 오브젝트도 포함하여 검색

        if (navMeshSurface != null)
        {
            Debug.Log($"Found NavMeshSurface '{navMeshSurface.name}' in children of {gameObject.name} and assigned it.", this);
        }
        else
        {
            Debug.LogWarning($"Could not find NavMeshSurface in children of {gameObject.name}.", this);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}

[CustomEditor(typeof(RoomSetting))]
public class RoomSettingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomSetting roomSetting = (RoomSetting)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("NavMesh", EditorStyles.boldLabel); // NavMesh 섹션 추가
        if (GUILayout.Button("All Setting"))
        {
            roomSetting.FindNavMeshSurface(); // 버튼 클릭 시 함수 호출
            roomSetting.UpdateRoomLights();
            FindSpawnPoints(roomSetting);
            // roomSetting.UpdateRoomObjects();
            // roomSetting.RemoveNonPrefabObjects();
            // roomSetting.ToggleOtherObjects();
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("NavMesh", EditorStyles.boldLabel); // NavMesh 섹션 추가
        if (GUILayout.Button("Find NavMesh Surface"))
        {
            roomSetting.FindNavMeshSurface(); // 버튼 클릭 시 함수 호출
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Points", EditorStyles.boldLabel);
        if (GUILayout.Button("Find Spawn Points"))
        {
            FindSpawnPoints(roomSetting);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lights", EditorStyles.boldLabel);
        if (GUILayout.Button("Update Room Lights"))
        {
            roomSetting.UpdateRoomLights();
        }
        if (GUILayout.Button("Turn Off All Lights"))
        {
            roomSetting.TurnOffAllLights();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Objects", EditorStyles.boldLabel);
        if (GUILayout.Button("Update Room Objects"))
        {
            roomSetting.UpdateRoomObjects();
        }
        if (GUILayout.Button("Toggle Other Objects"))
        {
            roomSetting.ToggleOtherObjects();
        }
        if (GUILayout.Button("Remove Non-Prefab Objects"))
        {
            roomSetting.RemoveNonPrefabObjects();
        }
    }

    private void FindSpawnPoints(RoomSetting roomSetting)
    {
        roomSetting.eggSpawnPoints = new List<Transform>();
        roomSetting.itemSpawnPoints = new List<Transform>();

        Transform[] allChildren = roomSetting.GetComponentsInChildren<Transform>(true); // 비활성 오브젝트도 포함

        foreach (Transform child in allChildren)
        {
            if (child == roomSetting.transform) continue; // 자기 자신은 제외

            // 태그 대신 GetComponent로 컴포넌트 확인
            if (child.GetComponent<EggSpawnPoint>() != null)
            {
                roomSetting.eggSpawnPoints.Add(child);
            }
            else if (child.GetComponent<ItemSpawnPoint>() != null)
            {
                roomSetting.itemSpawnPoints.Add(child);
            }
        }

        // 변경 사항 저장
        EditorUtility.SetDirty(roomSetting);
        Debug.Log($"Spawn points updated for {roomSetting.name}: Found {roomSetting.eggSpawnPoints.Count} egg spawn points and {roomSetting.itemSpawnPoints.Count} item spawn points.");
    }
}

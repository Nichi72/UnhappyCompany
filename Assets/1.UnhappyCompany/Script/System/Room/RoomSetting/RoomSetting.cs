using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomSetting : MonoBehaviour
{
    [Header("Room Components")]
    public NavMeshSurface navMeshSurface;
    public List<EggSpawnPoint> eggSpawnPoints;
    public List<ItemSpawnPoint> itemSpawnPoints;
    public List<Light> roomLights;
    
    [Header("Spawn Point Prefabs")]
    [SerializeField] private string eggSpawnPointPrefabPath = "Assets/1.UnhappyCompany/Prefab/Room/Common/EggSpawnPoint.prefab";
    [SerializeField] private string itemSpawnPointPrefabPath = "Assets/1.UnhappyCompany/Prefab/Room/Common/ItemSpawnPoint.prefab";

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

#if UNITY_EDITOR
    public void CreateEggSpawnPoint()
    {
        // EggSpawnPoints 부모 오브젝트 찾기 또는 생성
        Transform eggSpawnPointsParent = transform.Find("EggSpawnPoints");
        if (eggSpawnPointsParent == null)
        {
            GameObject eggSpawnPointsObj = new GameObject("EggSpawnPoints");
            eggSpawnPointsObj.transform.SetParent(transform);
            eggSpawnPointsObj.transform.localPosition = Vector3.zero;
            eggSpawnPointsParent = eggSpawnPointsObj.transform;
            Debug.Log($"EggSpawnPoints 부모 오브젝트를 생성했습니다: {gameObject.name}");
        }

        // 프리팹 로드
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(eggSpawnPointPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"EggSpawnPoint 프리팹을 찾을 수 없습니다: {eggSpawnPointPrefabPath}");
            return;
        }

        // 새로운 스폰 포인트 생성
        GameObject newSpawnPoint = PrefabUtility.InstantiatePrefab(prefab, eggSpawnPointsParent) as GameObject;
        if (newSpawnPoint != null)
        {
            // 이름 설정 (중복 방지)
            int count = eggSpawnPointsParent.childCount;
            newSpawnPoint.name = $"EggSpawnPoint_{count:00}";
            
            // 위치 설정 (기존 스폰 포인트들과 겹치지 않게)
            Vector3 newPosition = Vector3.zero;
            if (count > 1)
            {
                newPosition = new Vector3((count - 1) * 2f, 0, 0);
            }
            newSpawnPoint.transform.localPosition = newPosition;

            // 리스트 업데이트
            UpdateSpawnPointLists();
            
            Debug.Log($"새로운 EggSpawnPoint를 생성했습니다: {newSpawnPoint.name}");
            EditorUtility.SetDirty(this);
        }
    }

    public void CreateItemSpawnPoint()
    {
        // ItemSpawnPoints 부모 오브젝트 찾기 또는 생성
        Transform itemSpawnPointsParent = transform.Find("ItemSpawnPoints");
        if (itemSpawnPointsParent == null)
        {
            GameObject itemSpawnPointsObj = new GameObject("ItemSpawnPoints");
            itemSpawnPointsObj.transform.SetParent(transform);
            itemSpawnPointsObj.transform.localPosition = Vector3.zero;
            itemSpawnPointsParent = itemSpawnPointsObj.transform;
            Debug.Log($"ItemSpawnPoints 부모 오브젝트를 생성했습니다: {gameObject.name}");
        }

        // 프리팹 로드
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(itemSpawnPointPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"ItemSpawnPoint 프리팹을 찾을 수 없습니다: {itemSpawnPointPrefabPath}");
            return;
        }

        // 새로운 스폰 포인트 생성
        GameObject newSpawnPoint = PrefabUtility.InstantiatePrefab(prefab, itemSpawnPointsParent) as GameObject;
        if (newSpawnPoint != null)
        {
            // 이름 설정 (중복 방지)
            int count = itemSpawnPointsParent.childCount;
            newSpawnPoint.name = $"ItemSpawnPoint_{count:00}";
            
            // 위치 설정 (기존 스폰 포인트들과 겹치지 않게)
            Vector3 newPosition = Vector3.zero;
            if (count > 1)
            {
                newPosition = new Vector3((count - 1) * 2f, 0, 0);
            }
            newSpawnPoint.transform.localPosition = newPosition;

            // 리스트 업데이트
            UpdateSpawnPointLists();
            
            Debug.Log($"새로운 ItemSpawnPoint를 생성했습니다: {newSpawnPoint.name}");
            EditorUtility.SetDirty(this);
        }
    }

    public void UpdateSpawnPointLists()
    {
        eggSpawnPoints = new List<EggSpawnPoint>();
        itemSpawnPoints = new List<ItemSpawnPoint>();

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child == transform) continue;

            if (child.GetComponent<EggSpawnPoint>() != null)
            {
                eggSpawnPoints.Add(child.GetComponent<EggSpawnPoint>());
            }
            else if (child.GetComponent<ItemSpawnPoint>() != null)
            {
                itemSpawnPoints.Add(child.GetComponent<ItemSpawnPoint>());
            }
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RoomSetting))]
public class RoomSettingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomSetting roomSetting = (RoomSetting)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Room Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("모든 설정 자동 찾기"))
        {
            roomSetting.FindNavMeshSurface();
            roomSetting.UpdateRoomLights();
            FindSpawnPoints(roomSetting);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("NavMesh", EditorStyles.boldLabel);
        if (GUILayout.Button("NavMesh Surface 찾기"))
        {
            roomSetting.FindNavMeshSurface();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Points", EditorStyles.boldLabel);
        if (GUILayout.Button("스폰 포인트 찾기"))
        {
            FindSpawnPoints(roomSetting);
        }
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("알 스폰 포인트 생성"))
        {
            roomSetting.CreateEggSpawnPoint();
        }
        if (GUILayout.Button("아이템 스폰 포인트 생성"))
        {
            roomSetting.CreateItemSpawnPoint();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lights", EditorStyles.boldLabel);
        if (GUILayout.Button("조명 목록 업데이트"))
        {
            roomSetting.UpdateRoomLights();
        }
        if (GUILayout.Button("모든 조명 끄기/켜기"))
        {
            roomSetting.TurnOffAllLights();
        }
    }

    private void FindSpawnPoints(RoomSetting roomSetting)
    {
        roomSetting.UpdateSpawnPointLists();
        
        // 변경 사항 저장
        EditorUtility.SetDirty(roomSetting);
        Debug.Log($"스폰 포인트 업데이트 완료 {roomSetting.name}: 알 스폰 포인트 {roomSetting.eggSpawnPoints.Count}개, 아이템 스폰 포인트 {roomSetting.itemSpawnPoints.Count}개 발견.");
    }
}
#endif

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
    
    [Header("Spawned Objects")]
    [Tooltip("이 방에 생성된 알들")]
    public List<GameObject> spawnedEggs = new List<GameObject>();
    
    [Header("LOD Settings")]
    public LODGroup lodGroup;
    public bool enableLODLightControl = true; // LOD에 따른 라이트 제어 활성화
    private int currentLODLevel = -1; // 현재 LOD 레벨 캐싱
    
    [Header("Spawn Point Prefabs")]
    [SerializeField] private string eggSpawnPointPrefabPath = "Assets/1.UnhappyCompany/Prefab/Room/Common/EggSpawnPoint.prefab";
    [SerializeField] private string itemSpawnPointPrefabPath = "Assets/1.UnhappyCompany/Prefab/Room/Common/ItemSpawnPoint.prefab";

    private void Start()
    {
        // LODGroup이 없다면 자동으로 찾기
        if (lodGroup == null)
        {
            lodGroup = GetComponent<LODGroup>();
        }
    }

    private void Update()
    {
        // LOD에 따른 라이트 제어가 활성화되어 있고 LODGroup이 있을 때만 실행
        if (enableLODLightControl && lodGroup != null && roomLights != null && roomLights.Count > 0)
        {
            UpdateLightsBasedOnLOD();
        }
    }

    /// <summary>
    /// 현재 LOD 레벨에 따라 라이트를 제어합니다.
    /// LOD 0일 때만 라이트를 활성화하고, 나머지는 비활성화합니다.
    /// LODGroup이 자동으로 계산한 LOD 레벨을 활용합니다.
    /// </summary>
    private void UpdateLightsBasedOnLOD()
    {
        // LODGroup의 각 LOD 레벨 렌더러를 체크하여 현재 활성화된 LOD 레벨을 찾음
        LOD[] lods = lodGroup.GetLODs();
        int newLODLevel = -1; // -1은 컬링된 상태
        
        for (int i = 0; i < lods.Length; i++)
        {
            // 해당 LOD 레벨의 모든 렌더러를 체크
            if (lods[i].renderers != null && lods[i].renderers.Length > 0)
            {
                // 렌더러가 실제로 보이는지 확인 (isVisible 사용)
                foreach (Renderer renderer in lods[i].renderers)
                {
                    if (renderer != null && renderer.isVisible)
                    {
                        newLODLevel = i;
                        break;
                    }
                }
                
                // 현재 LOD를 찾았으면 루프 종료
                if (newLODLevel == i)
                    break;
            }
        }

        // LOD 레벨이 변경되었을 때만 라이트 상태 업데이트
        if (newLODLevel != currentLODLevel)
        {
            currentLODLevel = newLODLevel;
            bool shouldLightsBeOn = (currentLODLevel == 0);

            foreach (Light light in roomLights)
            {
                if (light != null)
                {
                    light.enabled = shouldLightsBeOn;
                }
            }

            Debug.Log($"[{gameObject.name}] LOD Level Changed: {currentLODLevel}, Lights: {(shouldLightsBeOn ? "ON" : "OFF")}");
        }
    }

    public void FindLODGroup()
    {
        lodGroup = GetComponent<LODGroup>();

        if (lodGroup != null)
        {
            Debug.Log($"Found LODGroup in {gameObject.name}.", this);
        }
        else
        {
            Debug.LogWarning($"Could not find LODGroup in {gameObject.name}.", this);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

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
            roomSetting.FindLODGroup();
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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("LOD", EditorStyles.boldLabel);
        if (GUILayout.Button("LODGroup 찾기"))
        {
            roomSetting.FindLODGroup();
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

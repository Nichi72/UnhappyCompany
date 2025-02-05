using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal; // ReorderableList가 있는 네임스페이스
using System.Collections.Generic;

#region PrefabData Definition
[System.Serializable]
public class PrefabData
{
    public string displayName;
    public GameObject prefab;
    [HideInInspector]
    public Texture2D preview;
}
#endregion

public class LabWatcherEditor : EditorWindow
{
    // 기존 탭에 ReorderableList를 이용한 "Prefab Placement" 탭을 추가
    private enum Tab
    {
        GridSnap,
        Array,
        RandomPlacement,
        ObjectPainter,
        BatchEdit,
        AlignDistribute,
        PresetProfile,
        PrefabPlacement // 여기에서 ReorderableList 사용
    }

    private Tab currentTab;

    // ------------------- 공용 프로필 -------------------
    private PlacementToolProfile profile;

    // ------------------- Object Painter 예제 변수 -------------------
    private bool isPainting = false;
    private Vector3 lastPaintPosition;

    // ------------------- ReorderableList 관련 -------------------
    [SerializeField]
    private List<PrefabData> prefabList = new List<PrefabData>(); 
    private ReorderableList reorderableList;  // 드래그 앤 드롭으로 순서 변경 가능

    // ------------------- Prefab Placement -------------------
    private bool isPrefabPlacing = false;  
    private GameObject ghostObject;        
    // ReorderableList.index로 현재 선택된 PrefabData 인덱스를 얻을 수 있음

    [MenuItem("Lab Watcher/Placement Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<LabWatcherEditor>("LabWatcher");
        window.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        // ReorderableList 생성
        CreateReorderableList();
        // 미리보기 이미지 갱신
        UpdatePrefabPreviews();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyGhost();
    }

    private void OnGUI()
    {
        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new string[]
        {
            "Grid Snap", 
            "Array", 
            "Random", 
            "Painter", 
            "Batch Edit", 
            "Align & Dist.", 
            "Preset", 
            "Prefab Placement"
        });

        EditorGUILayout.Space();

        // 공용 프로필
        profile = (PlacementToolProfile)EditorGUILayout.ObjectField("Profile", profile, typeof(PlacementToolProfile), false);
        if (profile == null)
        {
            if (GUILayout.Button("Create New Profile"))
            {
                profile = CreateInstance<PlacementToolProfile>();
            }
            EditorGUILayout.HelpBox("프로필이 없습니다. 생성 후 진행하세요.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();

        // 탭에 따라 다른 UI
        switch (currentTab)
        {
            case Tab.GridSnap:
                DrawGridSnapTab();
                break;
            case Tab.Array:
                DrawArrayTab();
                break;
            case Tab.RandomPlacement:
                DrawRandomPlacementTab();
                break;
            case Tab.ObjectPainter:
                DrawObjectPainterTab();
                break;
            case Tab.BatchEdit:
                DrawBatchEditTab();
                break;
            case Tab.AlignDistribute:
                DrawAlignDistributeTab();
                break;
            case Tab.PresetProfile:
                DrawPresetTab();
                break;
            case Tab.PrefabPlacement:
                DrawPrefabPlacementTab();
                break;
        }
    }

    //--------------------------------------------------------------------------
    // 1) Grid Snap Tab (기존 예시)
    //--------------------------------------------------------------------------
    private void DrawGridSnapTab()
    {
        EditorGUILayout.LabelField("그리드 스냅 설정", EditorStyles.boldLabel);
        profile.enableGridSnap = EditorGUILayout.Toggle("Enable Grid Snap", profile.enableGridSnap);
        profile.gridSize = EditorGUILayout.FloatField("Grid Size", profile.gridSize);
        EditorGUILayout.HelpBox("씬 뷰에서 오브젝트 이동 시 자동 스냅 적용.", MessageType.Info);
    }

    private void HandleGridSnap(SceneView sceneView)
    {
        if (!profile.enableGridSnap || profile.gridSize <= 0f) return;
        Event e = Event.current;
        if (e.type == EventType.MouseUp || e.type == EventType.MouseDrag)
        {
            GameObject[] selectedObjs = Selection.gameObjects;
            foreach (var go in selectedObjs)
            {
                Undo.RecordObject(go.transform, "Grid Snap");
                var pos = go.transform.position;
                go.transform.position = new Vector3(
                    Mathf.Round(pos.x / profile.gridSize) * profile.gridSize,
                    Mathf.Round(pos.y / profile.gridSize) * profile.gridSize,
                    Mathf.Round(pos.z / profile.gridSize) * profile.gridSize
                );
            }
        }
    }

    //--------------------------------------------------------------------------
    // 2) Array Tab (기존 예시)
    //--------------------------------------------------------------------------
    private void DrawArrayTab()
    {
        EditorGUILayout.LabelField("오브젝트 배열/복제 설정", EditorStyles.boldLabel);

        profile.arrayPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", profile.arrayPrefab, typeof(GameObject), false);
        profile.arrayStartPos = EditorGUILayout.Vector3Field("Start Position", profile.arrayStartPos);
        profile.arraySpacing = EditorGUILayout.Vector3Field("Spacing", profile.arraySpacing);
        profile.countX = EditorGUILayout.IntField("Count X", profile.countX);
        profile.countY = EditorGUILayout.IntField("Count Y", profile.countY);
        profile.countZ = EditorGUILayout.IntField("Count Z", profile.countZ);

        if (GUILayout.Button("Create Array"))
        {
            CreateArray(profile.arrayPrefab, profile.arrayStartPos, profile.arraySpacing, profile.countX, profile.countY, profile.countZ);
        }
    }

    private void CreateArray(GameObject prefab, Vector3 startPosition, Vector3 spacing, int countX, int countY, int countZ)
    {
        if (prefab == null) return;

        Undo.SetCurrentGroupName("Create Array");
        int groupIndex = Undo.GetCurrentGroup();

        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    Vector3 pos = startPosition + Vector3.Scale(spacing, new Vector3(x, y, z));
                    GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Undo.RegisterCreatedObjectUndo(newObj, "Create Array Element");
                    newObj.transform.position = pos;
                }
            }
        }

        Undo.CollapseUndoOperations(groupIndex);
    }

    //--------------------------------------------------------------------------
    // 3) Random Placement Tab (기존 예시)
    //--------------------------------------------------------------------------
    private void DrawRandomPlacementTab()
    {
        EditorGUILayout.LabelField("랜덤 배치/랜덤 변형", EditorStyles.boldLabel);

        profile.randomPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", profile.randomPrefab, typeof(GameObject), false);
        profile.randomCount = EditorGUILayout.IntField("Count", profile.randomCount);
        profile.positionRange = EditorGUILayout.FloatField("Position Range", profile.positionRange);
        profile.rotationMaxAngle = EditorGUILayout.FloatField("Rotation Max Angle", profile.rotationMaxAngle);
        profile.scaleMin = EditorGUILayout.FloatField("Scale Min", profile.scaleMin);
        profile.scaleMax = EditorGUILayout.FloatField("Scale Max", profile.scaleMax);

        if (GUILayout.Button("Generate Random"))
        {
            GenerateRandomPlacement();
        }
    }

    private void GenerateRandomPlacement()
    {
        if (profile.randomPrefab == null) return;

        Undo.SetCurrentGroupName("Random Placement");
        int groupIndex = Undo.GetCurrentGroup();

        for (int i = 0; i < profile.randomCount; i++)
        {
            Vector3 randPos = GetRandomPosition(Vector3.zero, profile.positionRange);
            Quaternion randRot = GetRandomRotation(profile.rotationMaxAngle);
            Vector3 randScale = GetRandomScale(profile.scaleMin, profile.scaleMax);

            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(profile.randomPrefab);
            Undo.RegisterCreatedObjectUndo(newObj, "Create Random Object");
            newObj.transform.position = randPos;
            newObj.transform.rotation = randRot;
            newObj.transform.localScale = randScale;
        }

        Undo.CollapseUndoOperations(groupIndex);
    }

    private Vector3 GetRandomPosition(Vector3 basePos, float range)
    {
        return basePos + new Vector3(
            Random.Range(-range, range),
            0f,
            Random.Range(-range, range)
        );
    }

    private Quaternion GetRandomRotation(float maxAngle)
    {
        return Quaternion.Euler(
            0f,
            Random.Range(0f, maxAngle),
            0f
        );
    }

    private Vector3 GetRandomScale(float minScale, float maxScale)
    {
        float scale = Random.Range(minScale, maxScale);
        return new Vector3(scale, scale, scale);
    }

    //--------------------------------------------------------------------------
    // 4) Object Painter Tab (기존 예시)
    //--------------------------------------------------------------------------
    private void DrawObjectPainterTab()
    {
        EditorGUILayout.LabelField("오브젝트 페인터", EditorStyles.boldLabel);

        profile.paintPrefab = (GameObject)EditorGUILayout.ObjectField("Paint Prefab", profile.paintPrefab, typeof(GameObject), false);
        profile.brushSize = EditorGUILayout.FloatField("Brush Size", profile.brushSize);
        profile.brushDensity = EditorGUILayout.FloatField("Brush Density", profile.brushDensity);
        profile.paintRandomRotationMax = EditorGUILayout.FloatField("Random Rotation Max", profile.paintRandomRotationMax);
        profile.paintUseRandomScale = EditorGUILayout.Toggle("Use Random Scale", profile.paintUseRandomScale);
        if (profile.paintUseRandomScale)
        {
            profile.paintScaleMin = EditorGUILayout.FloatField("Min Scale", profile.paintScaleMin);
            profile.paintScaleMax = EditorGUILayout.FloatField("Max Scale", profile.paintScaleMax);
        }

        EditorGUILayout.Space();

        isPainting = GUILayout.Toggle(isPainting, "Painting Mode", "Button", GUILayout.Height(30));
        if (isPainting)
        {
            EditorGUILayout.HelpBox("씬 뷰에서 마우스를 드래그하여 페인트합니다.", MessageType.Info);
        }
    }

    private void HandleObjectPainter(SceneView sceneView)
    {
        if (!isPainting || profile.paintPrefab == null) return;

        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            // 브러시 시각화
            Handles.color = new Color(1, 0, 0, 0.4f);
            Handles.DrawSolidDisc(hit.point, Vector3.up, profile.brushSize);

            // 배치
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                if (Vector3.Distance(hit.point, lastPaintPosition) > (1f / profile.brushDensity))
                {
                    GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(profile.paintPrefab);
                    Undo.RegisterCreatedObjectUndo(newObj, "Paint Object");
                    newObj.transform.position = GetRandomPointInCircle(hit.point, profile.brushSize);
                    newObj.transform.rotation = GetRandomRotation(profile.paintRandomRotationMax);

                    if (profile.paintUseRandomScale)
                    {
                        newObj.transform.localScale = GetRandomScale(profile.paintScaleMin, profile.paintScaleMax);
                    }

                    lastPaintPosition = hit.point;
                    e.Use();
                }
            }
        }

        HandleUtility.AddDefaultControl(controlID);
    }

    private Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        Vector2 randPos = Random.insideUnitCircle * radius;
        return center + new Vector3(randPos.x, 0f, randPos.y);
    }

    //--------------------------------------------------------------------------
    // 5) Batch Edit Tab (기존 예시)
    //--------------------------------------------------------------------------
    private void DrawBatchEditTab()
    {
        EditorGUILayout.LabelField("배치된 오브젝트 일괄 편집", EditorStyles.boldLabel);

        profile.batchPositionOffset = EditorGUILayout.Vector3Field("Position Offset", profile.batchPositionOffset);
        profile.batchRotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", profile.batchRotationOffset);
        profile.batchScaleMultiplier = EditorGUILayout.Vector3Field("Scale Multiplier", profile.batchScaleMultiplier);

        if (GUILayout.Button("Apply Batch Edit to Selected"))
        {
            ApplyBatchEdit(Selection.gameObjects);
        }
    }

    private void ApplyBatchEdit(GameObject[] selectedObjects)
    {
        Undo.RecordObjects(selectedObjects, "Batch Edit");
        foreach (GameObject go in selectedObjects)
        {
            // 위치
            go.transform.position += profile.batchPositionOffset;

            // 회전
            var euler = go.transform.eulerAngles;
            euler += profile.batchRotationOffset;
            go.transform.eulerAngles = euler;

            // 스케일
            var scale = go.transform.localScale;
            scale = Vector3.Scale(scale, profile.batchScaleMultiplier);
            go.transform.localScale = scale;
        }
    }

    //--------------------------------------------------------------------------
    // 6) Align & Distribute Tab (기존 예시)
    //--------------------------------------------------------------------------
    private void DrawAlignDistributeTab()
    {
        EditorGUILayout.LabelField("자동 정렬(Align & Distribute)", EditorStyles.boldLabel);

        profile.alignX = EditorGUILayout.Toggle("Align X", profile.alignX);
        profile.alignY = EditorGUILayout.Toggle("Align Y", profile.alignY);
        profile.alignZ = EditorGUILayout.Toggle("Align Z", profile.alignZ);

        EditorGUILayout.HelpBox("정렬: 선택된 오브젝트를 평균 위치에 맞춤.\n분배: X축 기준으로 균등 간격 배치 등.", MessageType.Info);

        if (GUILayout.Button("Align Selected"))
        {
            AlignSelectedObjects(Selection.gameObjects);
        }

        if (GUILayout.Button("Distribute Selected (X axis)"))
        {
            DistributeSelectedObjects(Selection.gameObjects);
        }
    }

    private void AlignSelectedObjects(GameObject[] selectedObjects)
    {
        if (selectedObjects.Length == 0) return;

        Vector3 avgPos = Vector3.zero;
        foreach (var go in selectedObjects)
        {
            avgPos += go.transform.position;
        }
        avgPos /= selectedObjects.Length;

        Undo.RecordObjects(selectedObjects, "Align Objects");
        foreach (var go in selectedObjects)
        {
            var pos = go.transform.position;
            if (profile.alignX) pos.x = avgPos.x;
            if (profile.alignY) pos.y = avgPos.y;
            if (profile.alignZ) pos.z = avgPos.z;
            go.transform.position = pos;
        }
    }

    private void DistributeSelectedObjects(GameObject[] selectedObjects)
    {
        if (selectedObjects.Length < 2) return;

        System.Array.Sort(selectedObjects, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        float minX = selectedObjects[0].transform.position.x;
        float maxX = selectedObjects[selectedObjects.Length - 1].transform.position.x;
        float gap = (maxX - minX) / (selectedObjects.Length - 1);

        Undo.RecordObjects(selectedObjects, "Distribute Objects");
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            var pos = selectedObjects[i].transform.position;
            pos.x = minX + gap * i;
            selectedObjects[i].transform.position = pos;
        }
    }

    //--------------------------------------------------------------------------
    // 7) Preset/Profile Tab (기존 예시)
    //--------------------------------------------------------------------------
    private void DrawPresetTab()
    {
        EditorGUILayout.LabelField("프리셋 / 프로필 관리", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("현재 Profile을 ScriptableObject로 저장 가능.", MessageType.Info);

        if (GUILayout.Button("Save Profile As..."))
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Profile", "PlacementToolProfile", "asset", "Save Profile As");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(profile, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = profile;
            }
        }
    }

    //--------------------------------------------------------------------------
    // (중요) 8) Prefab Placement Tab (ReorderableList 활용)
    //--------------------------------------------------------------------------
    private void DrawPrefabPlacementTab()
    {
        EditorGUILayout.LabelField("Prefab Placement (ReorderableList)", EditorStyles.boldLabel);

        // ReorderableList 그리기
        reorderableList.DoLayoutList();

        EditorGUILayout.Space(10);

        // 현재 선택된 슬롯 인덱스
        int idx = reorderableList.index;
        if (idx >= 0 && idx < prefabList.Count)
        {
            var selectedData = prefabList[idx];
            if (selectedData.prefab != null)
            {
                EditorGUILayout.LabelField("Selected Prefab: " + selectedData.displayName);
            }
            else
            {
                EditorGUILayout.HelpBox("Selected slot has no prefab assigned.", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No Prefab Selected", MessageType.Info);
        }

        EditorGUILayout.Space();

        // 배치 모드
        isPrefabPlacing = GUILayout.Toggle(isPrefabPlacing, "Prefab Placement Mode", "Button", GUILayout.Height(30));
        if (!isPrefabPlacing)
        {
            DestroyGhost();
        }
    }

    //--------------------------------------------------------------------------
    // ReorderableList 생성 및 콜백 설정
    //--------------------------------------------------------------------------
    private void CreateReorderableList()
    {
        reorderableList = new ReorderableList(prefabList, typeof(PrefabData), 
            draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

        // 헤더 표시
        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Prefab List (Drag & Drop to Reorder)");
        };

        // 각 Element의 높이 (기본 16, 조금 여유를 주면 50)
        reorderableList.elementHeight = 50f;

        // 각 Element의 그리기 (index별)
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var data = prefabList[index];
            float padding = 4f;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            // 프리뷰 아이콘 영역
            Rect iconRect = new Rect(rect.x + padding, rect.y + padding, 40, 40);

            // displayName 입력필드 영역
            Rect nameRect = new Rect(iconRect.xMax + 5, rect.y + padding, rect.width - 60 - iconRect.width, lineHeight);

            // Prefab ObjectField 영역
            Rect prefabRect = new Rect(nameRect.x, rect.y + padding + lineHeight + 2, nameRect.width, lineHeight);

            // 아이콘 표시
            var icon = data.preview != null ? data.preview : EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
            GUI.Box(iconRect, icon);

            // displayName
            data.displayName = EditorGUI.TextField(nameRect, data.displayName);

            // prefab ObjectField
            EditorGUI.BeginChangeCheck();
            var newPrefab = (GameObject)EditorGUI.ObjectField(prefabRect, data.prefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                data.prefab = newPrefab;
                if (data.prefab != null)
                {
                    data.preview = AssetPreview.GetAssetPreview(data.prefab);
                }
                else
                {
                    data.preview = null;
                }
            }
        };

        // + 버튼 누를 때
        reorderableList.onAddCallback = (ReorderableList list) =>
        {
            prefabList.Add(new PrefabData { displayName = "New Prefab" });
        };

        // – 버튼 누를 때
        reorderableList.onRemoveCallback = (ReorderableList list) =>
        {
            // 선택된 인덱스 제거
            if (list.index >= 0 && list.index < prefabList.Count)
            {
                prefabList.RemoveAt(list.index);
                list.index = -1;
            }
        };

        // 리스트 항목이 선택되었을 때
        reorderableList.onSelectCallback = (ReorderableList list) =>
        {
            int idx = list.index;
            if (idx >= 0 && idx < prefabList.Count)
            {
                var data = prefabList[idx];
                CreateGhost(data.prefab);
            }
        };
    }

    //--------------------------------------------------------------------------
    // SceneView 이벤트
    //--------------------------------------------------------------------------
    private void OnSceneGUI(SceneView sceneView)
    {
        // Grid Snap
        HandleGridSnap(sceneView);

        // Object Painter
        HandleObjectPainter(sceneView);

        // Prefab Placement (ReorderableList)
        if (currentTab == Tab.PrefabPlacement)
        {
            HandlePrefabPlacement(sceneView);
        }
    }

    private void HandlePrefabPlacement(SceneView sceneView)
    {
        if (!isPrefabPlacing) return;

        // ReorderableList.index → 현재 선택된
        int idx = reorderableList.index;
        if (idx < 0 || idx >= prefabList.Count) return;

        var data = prefabList[idx];
        if (data.prefab == null) return;

        // 고스트 없으면 생성
        if (ghostObject == null)
        {
            CreateGhost(data.prefab);
        }

        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 300f))
        {
            ghostObject.transform.position = hit.point;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlacePrefabAtPoint(hit.point);
                e.Use();
            }

            // 시각화 디스크
            Handles.color = new Color(0, 1, 0, 0.3f);
            Handles.DrawSolidDisc(hit.point, Vector3.up, 0.5f);
        }

        HandleUtility.AddDefaultControl(controlID);
    }

    private void PlacePrefabAtPoint(Vector3 position)
    {
        // 현재 선택된 PrefabData
        int idx = reorderableList.index;
        if (idx < 0 || idx >= prefabList.Count) return;

        var data = prefabList[idx];
        if (data.prefab == null) return;

        Undo.SetCurrentGroupName("Place Prefab");
        int group = Undo.GetCurrentGroup();

        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(data.prefab);
        Undo.RegisterCreatedObjectUndo(newObj, "Place Prefab");
        newObj.transform.position = position;

        Undo.CollapseUndoOperations(group);
    }

    private void CreateGhost(GameObject prefab)
    {
        DestroyGhost(); 
        if (prefab == null) return;

        ghostObject = Instantiate(prefab);
        ghostObject.name = "GhostPreview_" + prefab.name;
        
        // 에디터 전용으로 Hierarchy에서 감춤
        ghostObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

        // [1] 고스트 오브젝트와 자식에 포함된 모든 콜라이더 제거
        var colliders = ghostObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            DestroyImmediate(col);
        }

        // [2] 고스트 오브젝트와 자식에 포함된 모든 리지드바디 제거
        var rigidbodies = ghostObject.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
        {
            DestroyImmediate(rb);
        }

        // [3] 반투명 머티리얼 적용 (기존 코드)
        var renderers = ghostObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            if (r.sharedMaterial != null)
            {
                Material mat = new Material(r.sharedMaterial);
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.3f);
                r.sharedMaterial = mat;
            }
        }
    }


    private void DestroyGhost()
    {
        if (ghostObject != null)
        {
            DestroyImmediate(ghostObject);
        }
    }

    private void UpdatePrefabPreviews()
    {
        foreach (var data in prefabList)
        {
            if (data.prefab != null)
            {
                data.preview = AssetPreview.GetAssetPreview(data.prefab);
            }
        }
    }
}

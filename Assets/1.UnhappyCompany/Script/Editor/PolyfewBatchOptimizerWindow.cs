using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using BrainFailProductions.PolyFewRuntime;
using BrainFailProductions.PolyFew;
using UnhappyCompany.Utility;

namespace UnhappyCompany.Editor
{
    public class PolyfewBatchOptimizerWindow : EditorWindow
    {
        [MenuItem("UnhappyCompany/Polyfew Batch Optimizer")]
        public static void ShowWindow()
        {
            var window = GetWindow<PolyfewBatchOptimizerWindow>("Polyfew Batch Optimizer");
            window.minSize = new Vector2(400, 600);
        }
        
        [SerializeField]
        private List<GameObject> targetObjects = new List<GameObject>();
        
        [SerializeField]
        private PolyfewOptimizationProfile optimizationProfile;
        
        [SerializeField]
        private string saveAssetsPath = "Assets/OptimizedMeshes";
        
        [SerializeField]
        private ErrorHandlingMode errorHandlingMode = ErrorHandlingMode.SkipAndContinue;
        
        [SerializeField]
        private bool generateLODs = true;
        
        [SerializeField]
        private bool isProcessing = false;
        
        private Vector2 scrollPosition;
        private SerializedObject serializedObject;
        private SerializedProperty targetObjectsProperty;
        
        public enum ErrorHandlingMode
        {
            StopOnError,
            SkipAndContinue
        }
        
        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            targetObjectsProperty = serializedObject.FindProperty("targetObjects");
            
            // 기본 프로필이 없으면 생성
            if (optimizationProfile == null)
            {
                CreateDefaultProfile();
            }
        }
        
        private void OnGUI()
        {
            serializedObject.Update();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawTargetObjectsSection();
            DrawOptimizationProfileSection();
            DrawSettingsSection();
            DrawProcessingSection();
            
            EditorGUILayout.EndScrollView();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Polyfew 배치 최적화 도구", headerStyle);
            EditorGUILayout.LabelField("여러 오브젝트를 한 번에 폴리곤 최적화합니다.", EditorStyles.centeredGreyMiniLabel);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
        }
        
        private void DrawTargetObjectsSection()
        {
            EditorGUILayout.LabelField("대상 오브젝트", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("최적화할 GameObject들을 드래그 앤 드롭으로 추가하세요.", MessageType.Info);
            
            // 드래그 앤 드롭 영역
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "여기에 GameObject를 드래그 앤 드롭하세요", EditorStyles.helpBox);
            
            HandleDragAndDrop(dropArea);
            
            // 오브젝트 리스트
            EditorGUILayout.PropertyField(targetObjectsProperty, new GUIContent("대상 오브젝트 리스트"), true);
            
            // 리스트 관리 버튼들
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("선택된 오브젝트 추가", GUILayout.Height(25)))
            {
                AddSelectedObjects();
            }
            if (GUILayout.Button("리스트 비우기", GUILayout.Height(25)))
            {
                targetObjects.Clear();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawOptimizationProfileSection()
        {
            EditorGUILayout.LabelField("최적화 프로필", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            optimizationProfile = (PolyfewOptimizationProfile)EditorGUILayout.ObjectField(
                "프로필", optimizationProfile, typeof(PolyfewOptimizationProfile), false);
            
            if (EditorGUI.EndChangeCheck() && optimizationProfile == null)
            {
                CreateDefaultProfile();
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("새 프로필 생성", GUILayout.Height(25)))
            {
                CreateNewProfile();
            }
            if (GUILayout.Button("기본 프로필 생성", GUILayout.Height(25)))
            {
                CreateDefaultProfile();
            }
            EditorGUILayout.EndHorizontal();
            
            // 프로필 미리보기
            if (optimizationProfile != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("프로필 미리보기:", EditorStyles.miniBoldLabel);
                
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"폴리곤 감소 강도: {optimizationProfile.simplificationStrength}%");
                EditorGUILayout.LabelField($"LOD 레벨: {optimizationProfile.lodLevels}개");
                EditorGUILayout.LabelField($"배치 크기: {optimizationProfile.batchSize}개");
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);
            
            // 저장 경로 설정
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("저장 경로:", GUILayout.Width(80));
            saveAssetsPath = EditorGUILayout.TextField(saveAssetsPath);
            if (GUILayout.Button("찾기", GUILayout.Width(50)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("저장 폴더 선택", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 절대 경로를 상대 경로로 변환
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        saveAssetsPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        saveAssetsPath = selectedPath;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // LOD 생성 옵션
            generateLODs = EditorGUILayout.Toggle("LOD 자동 생성", generateLODs);
            
            // 에러 처리 모드
            errorHandlingMode = (ErrorHandlingMode)EditorGUILayout.EnumPopup("에러 처리 방식", errorHandlingMode);
            
            EditorGUILayout.HelpBox(
                errorHandlingMode == ErrorHandlingMode.StopOnError 
                    ? "에러 발생 시 전체 작업을 중단합니다."
                    : "에러 발생 시 해당 오브젝트를 건너뛰고 계속 진행합니다.",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawProcessingSection()
        {
            EditorGUILayout.LabelField("처리", EditorStyles.boldLabel);
            
            // 처리 가능 여부 확인
            bool canProcess = !isProcessing && targetObjects.Count > 0 && optimizationProfile != null;
            string buttonText = isProcessing ? "처리 중..." : "최적화 시작";
            
            EditorGUI.BeginDisabledGroup(!canProcess);
            if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                StartOptimization();
            }
            EditorGUI.EndDisabledGroup();
            
            if (!canProcess && !isProcessing)
            {
                if (targetObjects.Count == 0)
                    EditorGUILayout.HelpBox("최적화할 오브젝트를 추가하세요.", MessageType.Warning);
                if (optimizationProfile == null)
                    EditorGUILayout.HelpBox("최적화 프로필을 선택하세요.", MessageType.Warning);
            }
            
            // 처리 상태 표시
            if (isProcessing)
            {
                EditorGUILayout.HelpBox("최적화가 진행 중입니다. Unity Console에서 진행 상황을 확인하세요.", MessageType.Info);
            }
        }
        
        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject gameObject)
                            {
                                if (!targetObjects.Contains(gameObject))
                                {
                                    targetObjects.Add(gameObject);
                                }
                            }
                        }
                    }
                    break;
            }
        }
        
        private void AddSelectedObjects()
        {
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                if (!targetObjects.Contains(selectedObject))
                {
                    targetObjects.Add(selectedObject);
                }
            }
        }
        
        private void CreateDefaultProfile()
        {
            // 기본 프로필을 프로젝트에서 찾기
            string[] guids = AssetDatabase.FindAssets("t:PolyfewOptimizationProfile");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<PolyfewOptimizationProfile>(path);
                if (profile.name.Contains("Default"))
                {
                    optimizationProfile = profile;
                    return;
                }
            }
            
            // 기본 프로필이 없으면 생성
            var defaultProfile = CreateInstance<PolyfewOptimizationProfile>();
            defaultProfile.name = "Default Polyfew Profile";
            
            // 기본값 설정 (요구사항에 맞게)
            defaultProfile.preserveUVFoldover = false;
            defaultProfile.preserveBorders = true;
            defaultProfile.preserveUVSeams = true;
            defaultProfile.useEdgeSort = false;
            defaultProfile.recalculateNormals = true;
            defaultProfile.clearBlendshapes = true;
            defaultProfile.generateUV2 = true;
            
            string savePath = "Assets/1.UnhappyCompany/Settings";
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            AssetDatabase.CreateAsset(defaultProfile, $"{savePath}/Default Polyfew Profile.asset");
            AssetDatabase.SaveAssets();
            
            optimizationProfile = defaultProfile;
        }
        
        private void CreateNewProfile()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "새 Polyfew 프로필 생성",
                "New Polyfew Profile",
                "asset",
                "새 최적화 프로필을 저장할 위치를 선택하세요.");
            
            if (!string.IsNullOrEmpty(path))
            {
                var newProfile = CreateInstance<PolyfewOptimizationProfile>();
                AssetDatabase.CreateAsset(newProfile, path);
                AssetDatabase.SaveAssets();
                
                optimizationProfile = newProfile;
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newProfile;
            }
        }
        
        private async void StartOptimization()
        {
            if (isProcessing) return;
            
            isProcessing = true;
            
            try
            {
                var optimizer = new PolyfewBatchOptimizer();
                await optimizer.OptimizeObjectsAsync(
                    targetObjects,
                    optimizationProfile,
                    saveAssetsPath,
                    generateLODs,
                    errorHandlingMode == ErrorHandlingMode.StopOnError
                );
                
                Debug.Log($"[Polyfew Batch Optimizer] 최적화 완료! {targetObjects.Count}개 오브젝트 처리됨.");
                EditorUtility.DisplayDialog("최적화 완료", "모든 오브젝트의 최적화가 완료되었습니다.", "확인");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Polyfew Batch Optimizer] 최적화 중 오류 발생: {ex.Message}");
                EditorUtility.DisplayDialog("오류", $"최적화 중 오류가 발생했습니다:\n{ex.Message}", "확인");
            }
            finally
            {
                isProcessing = false;
            }
        }
    }
}

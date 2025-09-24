using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

[DisallowMultipleComponent]
public class MeshRendererRemover : MonoBehaviour
{
    [Header("Strip Options")]
    [Tooltip("루트 자신도 포함해서 컴포넌트를 제거할지 여부")]
    public bool includeSelf = false;

    [Tooltip("플레이 중 자동 실행 (Start에서 1회)")]
    public bool runOnStart = false;

    [Tooltip("작업 완료 후 이 스크립트를 제거")]
    public bool removeThisComponentAfter = true;

    void Start()
    {
        if (runOnStart)
        {
            StripComponents();
        }
    }

    [ContextMenu("Strip: Keep only Collider & MeshFilter (children)")]
    public void StripComponents()
    {
        var allTransforms = GetComponentsInChildren<Transform>(true);

        foreach (var t in allTransforms)
        {
            if (t == null)
                continue; // 파괴된 오브젝트 건너뛰기
            
            if (!includeSelf && t == transform)
                continue;

            bool hasAnyCollider = t.GetComponent<Collider>() != null;
            bool hasMeshCollider = t.GetComponent<MeshCollider>() != null;
            bool allowMeshFilter = hasMeshCollider;

            var components = t.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null)
                    continue; // Missing script 슬롯 보호

                if (comp is Transform)
                    continue;
                if (comp is Collider)
                    continue;

                if (comp is MeshFilter)
                {
                    if (allowMeshFilter)
                        continue;
                    // MeshCollider가 없으면 MeshFilter 제거
                }

                // 나머지는 모두 제거 (또는 MeshFilter 비허용 시 제거)
                if (Application.isPlaying)
                    Destroy(comp);
                else
                {
#if UNITY_EDITOR
                    Undo.DestroyObjectImmediate(comp);
                    EditorUtility.SetDirty(t.gameObject);
#else
                    DestroyImmediate(comp);
#endif
                }
            }
        }

        // 제거 후 검증 로그 출력
        LogLeftoverNonAllowedComponents();

        // 에디터 모드에서 저장 처리
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                // 프리팹 스테이지에서 작업
                AssetDatabase.SaveAssets();
            }
            else
            {
                // 일반 씬에서 작업
                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
            }
        }
#endif

        if (removeThisComponentAfter)
        {
            if (Application.isPlaying)
                Destroy(this);
            else
            {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(this);
#else
                DestroyImmediate(this);
#endif
            }
        }
    }

    [ContextMenu("Log: Report non-allowed components (children)")]
    public void LogLeftoverNonAllowedComponents()
    {
        var allTransforms = GetComponentsInChildren<Transform>(true);
        bool anyProblem = false;
        foreach (var t in allTransforms)
        {
            if (t == null)
                continue; // 파괴된 오브젝트 건너뛰기
            
            if (!includeSelf && t == transform)
                continue;

            bool hasAnyCollider = t.GetComponent<Collider>() != null;
            bool hasMeshCollider = t.GetComponent<MeshCollider>() != null;
            bool allowMeshFilter = hasMeshCollider;

            if (!hasAnyCollider)
            {
                var pathNoCol = GetHierarchyPath(t);
                // Debug.Log($"[MeshRendererRemover] 콜라이더 없음: {pathNoCol} (Transform만 유지)", t);
            }

            var components = t.GetComponents<Component>();
            System.Collections.Generic.List<string> leftoverTypes = null;
            foreach (var comp in components)
            {
                if (comp == null)
                    continue;
                if (comp is Transform)
                    continue;
                if (comp is Collider)
                    continue;
                if (comp is MeshFilter && allowMeshFilter)
                    continue;

                if (leftoverTypes == null)
                    leftoverTypes = new System.Collections.Generic.List<string>();
                leftoverTypes.Add(comp.GetType().Name);
            }

            if (leftoverTypes != null && leftoverTypes.Count > 0)
            {
                var path = GetHierarchyPath(t);
                Debug.Log($"[MeshRendererRemover] 허용되지 않은 컴포넌트 발견: {path} → {string.Join(", ", leftoverTypes)}", t);
                anyProblem = true;
            }
        }
        if (!anyProblem)
        {
            Debug.Log("[MeshRendererRemover] 검사 완료: 문제 없음", this);
        }
    }

    // 규칙이 컨텍스트(오브젝트 상태)에 의존하므로 고정 허용 목록 메서드는 제거했습니다.

    private static string GetHierarchyPath(Transform t)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        var current = t;
        while (current != null)
        {
            if (sb.Length == 0)
                sb.Insert(0, current.name);
            else
                sb.Insert(0, current.name + "/");
            current = current.parent;
        }
        return sb.ToString();
    }
}

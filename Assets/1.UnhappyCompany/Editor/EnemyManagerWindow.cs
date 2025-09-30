using UnityEditor;
using UnityEngine;
using System.Linq;

public class EnemyManagerWindow : EditorWindow
{
    private BaseEnemyAIData[] enemyDataObjects;
    private int selectedEnemyIndex = -1;
    private string filterKeyword = "";
    private bool sortByName = false;
    private Vector2 scrollPosition;
    private Vector2 detailScrollPosition;

    [MenuItem(Structures.LAB_WATCHER + "/Enemy Manager")]
    public static void ShowWindow()
    {
        GetWindow<EnemyManagerWindow>("Enemy Manager");
    }

    private void OnEnable()
    {
        // 모든 EnemyAIData Scriptable Object를 로드합니다.
        enemyDataObjects = Resources.FindObjectsOfTypeAll<BaseEnemyAIData>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Enemy Manager", EditorStyles.boldLabel);

        // 탭 인터페이스
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("List"))
        {
            // List tab selected
        }
        if (GUILayout.Button("Presets"))
        {
            // Presets tab selected
        }
        if (GUILayout.Button("Preview"))
        {
            // Preview tab selected
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 필터링 UI
        filterKeyword = EditorGUILayout.TextField("Filter", filterKeyword);

        // 정렬 UI
        sortByName = EditorGUILayout.Toggle("Sort by Name", sortByName);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        // 스크롤 뷰 시작
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width / 2));

        var filteredEnemies = enemyDataObjects
            .Where(e => string.IsNullOrEmpty(filterKeyword) || e.enemyName.Contains(filterKeyword))
            .ToArray();

        if (sortByName)
        {
            filteredEnemies = filteredEnemies.OrderBy(e => e.enemyName).ToArray();
        }

        if (filteredEnemies.Length == 0)
        {
            GUILayout.Label("No enemies found.");
        }
        else
        {
            EditorGUILayout.BeginVertical();
            foreach (var enemyData in filteredEnemies)
            {
                if (GUILayout.Button(enemyData.enemyName))
                {
                    selectedEnemyIndex = System.Array.IndexOf(enemyDataObjects, enemyData);
                }
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        // 선택한 Enemy의 속성을 표시
        if (selectedEnemyIndex >= 0 && selectedEnemyIndex < enemyDataObjects.Length)
        {
            DisplaySelectedEnemyProperties(enemyDataObjects[selectedEnemyIndex]);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DisplaySelectedEnemyProperties(BaseEnemyAIData enemyData)
    {
        detailScrollPosition = EditorGUILayout.BeginScrollView(detailScrollPosition, GUILayout.Width(position.width / 2));
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Selected Enemy: " + enemyData.enemyName, EditorStyles.boldLabel);

        SerializedObject serializedEnemyData = new SerializedObject(enemyData);
        SerializedProperty property = serializedEnemyData.GetIterator();
        property.NextVisible(true); // Skip generic field

        while (property.NextVisible(false))
        {
            EditorGUILayout.PropertyField(property, true);
        }

        serializedEnemyData.ApplyModifiedProperties();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
} 
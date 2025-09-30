using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyAIController<>), true)]
public class EnemyAIControllerEditor : Editor
{
    private bool showAIDataSettings = true;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var controller = target as MonoBehaviour;
        var enemyData = GetEnemyData(controller);
        
        if (enemyData != null)
        {
            EditorGUILayout.Space(10);
            showAIDataSettings = EditorGUILayout.Foldout(showAIDataSettings, "AI Data Settings", true);
            
            if (showAIDataSettings)
            {
                EditorGUI.indentLevel++;
                
                SerializedObject soData = new SerializedObject(enemyData);
                SerializedProperty iterator = soData.GetIterator();
                bool enterChildren = true;
                
                EditorGUI.BeginChangeCheck();
                
                while (iterator.NextVisible(enterChildren))
                {
                    if (iterator.name == "m_Script") continue;
                    
                    EditorGUILayout.PropertyField(iterator, true);
                    enterChildren = false;
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    soData.ApplyModifiedProperties();
                    EditorUtility.SetDirty(enemyData);
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Apply Changes to Prefab"))
                {
                    var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(controller);
                    if (prefabRoot != null)
                    {
                        PrefabUtility.SavePrefabAsset(prefabRoot);
                        Debug.Log("Changes saved to prefab!");
                    }
                }
            }
        }
    }
    
    private BaseEnemyAIData GetEnemyData(MonoBehaviour controller)
    {
        var dataField = controller.GetType().GetField("enemyData", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
            
        return dataField?.GetValue(controller) as BaseEnemyAIData;
    }
} 
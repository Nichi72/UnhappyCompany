// SkinnedMeshToStaticConverter.cs
// Place this script inside any "Editor" folder.
// Usage: Select GameObjects that have SkinnedMeshRenderer → Tools ▸ Mesh Utilities ▸ Convert SkinnedMesh → Static (⌘/Ctrl+Shift+B)
// It will bake the current pose, save a new Mesh asset next to the source prefab/model, and create a new GameObject with MeshFilter + MeshRenderer.
// Written for Unity 2020.3+ but should work on newer versions as well.

using UnityEditor;
using UnityEngine;
using System.IO;

public static class SkinnedMeshToStaticConverter
{
    private const string MenuPath = "Tools/Mesh Utilities/Convert SkinnedMesh → Static %#b"; // %#b = Ctrl/Cmd+Shift+B

    // Validate: at least one selected object has a SkinnedMeshRenderer
    [MenuItem(MenuPath, true)]
    private static bool Validate()
    {
        foreach (var obj in Selection.gameObjects)
        {
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return true;
        }
        return false;
    }

    [MenuItem(MenuPath, false, 500)]
    private static void Convert()
    {
        foreach (var go in Selection.gameObjects)
        {
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) continue;

            // 1. Bake skinned mesh into a new Mesh instance
            var bakedMesh = new Mesh();
            smr.BakeMesh(bakedMesh);
            bakedMesh.name = go.name + "_Baked";

            // 2. Save the baked mesh as an asset for reuse / prefab safety
            string assetPath = GetSavePath(go, bakedMesh.name);
            AssetDatabase.CreateAsset(bakedMesh, assetPath);
            AssetDatabase.SaveAssets();

            // 3. Create a new GameObject that uses the baked mesh
            GameObject staticGO = new GameObject(go.name + "_Static");
            Undo.RegisterCreatedObjectUndo(staticGO, "Create Static Mesh");

            staticGO.transform.SetParent(go.transform.parent, false);
            staticGO.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);
            staticGO.transform.localScale = go.transform.localScale;

            var mf = staticGO.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

            var mr = staticGO.AddComponent<MeshRenderer>();
            mr.sharedMaterials = smr.sharedMaterials;

            // Copy static flags / lightmap settings if desired
            GameObjectUtility.SetStaticEditorFlags(staticGO, GameObjectUtility.GetStaticEditorFlags(go));

            // Select the new object for convenience
            Selection.activeGameObject = staticGO;
        }
    }

    // Helper ─ find a reasonable place to store the new asset (next to prefab or in Assets)
    private static string GetSavePath(GameObject source, string meshName)
    {
        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(source);
        string dir = string.IsNullOrEmpty(prefabPath) ? "Assets" : Path.GetDirectoryName(prefabPath);
        return AssetDatabase.GenerateUniqueAssetPath($"{dir}/{meshName}.asset");
    }
}

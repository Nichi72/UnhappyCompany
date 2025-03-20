#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace GSpawn_Lite
{
    public static class ScriptableObjectEx
    {
        public static bool isAsset(this ScriptableObject so)
        {
            return AssetDatabase.Contains(so);
        }

        public static void destroyImmediate(ScriptableObject so)
        {
            if (so != null && !so.isAsset()) ScriptableObject.DestroyImmediate(so);
        }

    }
}
#endif
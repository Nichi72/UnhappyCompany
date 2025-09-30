#if UNITY_EDITOR
using UnityEditor;

namespace GSpawn_Lite
{
    public class PluginAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (IntPatternDb.exists)                            IntPatternDb.instance.onPostProcessAllAssets();
            if (GridSettingsProfileDb.exists)                   GridSettingsProfileDb.instance.onPostProcessAllAssets();
            if (SegmentsObjectSpawnSettingsProfileDb.exists)    SegmentsObjectSpawnSettingsProfileDb.instance.onPostProcessAllAssets();
            if (IntRangePrefabProfileDb.exists)                 IntRangePrefabProfileDb.instance.onPostProcessAllAssets();
            if (RandomPrefabProfileDb.exists)                   RandomPrefabProfileDb.instance.onPostProcessAllAssets();
            if (PrefabLibProfileDb.exists)                      PrefabLibProfileDb.instance.onPostProcessAllAssets();
            if (ShortcutProfileDb.exists)                       ShortcutProfileDb.instance.onPostProcessAllAssets();
        }
    }
}
#endif
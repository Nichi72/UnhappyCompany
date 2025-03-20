#if UNITY_EDITOR
using System.Collections.Generic;

namespace GSpawn_Lite
{
    public static class PluginPrefabEvents
    {
        public static void onPrefabsWillBeRemoved(List<PluginPrefab> pluginPrefabs)
        {
            RandomPrefabProfileDb.instance.deletePrefabs(pluginPrefabs);
            IntRangePrefabProfileDb.instance.deletePrefabs(pluginPrefabs);
        }

        public static void onPrefabChangedName(PluginPrefab prefab)
        {
            RandomPrefabProfileDbUI.instance.refresh();
            IntRangePrefabProfileDbUI.instance.refresh();
            PluginPrefabManagerUI.instance.refresh();;
        }
    }
}
#endif
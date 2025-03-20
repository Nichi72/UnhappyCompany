#if UNITY_EDITOR
namespace GSpawn_Lite
{
    public static class UIRefresh
    {
        public static void refreshShortcutToolTips()
        {
            PluginInspectorUI.instance.refresh();
        }
    }
}
#endif
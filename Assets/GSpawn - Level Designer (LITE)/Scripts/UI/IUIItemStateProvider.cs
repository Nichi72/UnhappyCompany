#if UNITY_EDITOR
namespace GSpawn_Lite
{
    public interface IUIItemStateProvider
    {
        bool            uiSelected      { get; set; }
        CopyPasteMode   uiCopyPasteMode { get; set; }
        PluginGuid      guid            { get; }
    }
}
#endif
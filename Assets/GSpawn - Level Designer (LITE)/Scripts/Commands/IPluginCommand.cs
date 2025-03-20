#if UNITY_EDITOR
namespace GSpawn_Lite
{
    public interface IPluginCommand
    {
        void enter  ();
        void exit   ();
    }
}
#endif
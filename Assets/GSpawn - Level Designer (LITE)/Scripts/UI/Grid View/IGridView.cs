#if UNITY_EDITOR
namespace GSpawn_Lite
{
    public interface IGridView
    {
        int             dragAndDropInitiatorId  { get; }
        System.Object   dragAndDropData         { get; }
    }
}
#endif
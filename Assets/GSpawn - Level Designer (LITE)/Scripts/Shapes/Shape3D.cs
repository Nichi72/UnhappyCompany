#if UNITY_EDITOR
namespace GSpawn_Lite
{
    public abstract class Shape3D
    {
        public abstract void drawFilled();
        public abstract void drawWire();
    }
}
#endif
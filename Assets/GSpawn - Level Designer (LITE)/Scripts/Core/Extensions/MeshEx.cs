#if UNITY_EDITOR
using UnityEngine;

namespace GSpawn_Lite
{
    public static class MeshEx
    {
        public static float calcInscribedWorldSphereRadius(this Mesh mesh, Transform meshTransform)
        {
            return mesh.bounds.extents.getMaxAbsComp() * meshTransform.lossyScale.getMaxAbsComp();
        }
    }
}
#endif
#if UNITY_EDITOR
using UnityEngine;

namespace GSpawn_Lite
{
    public class TerrainObjectOverlapFilter
    {
        public bool filterObject(GameObject gameObject)
        {
            return true;
        }
    }
}
#endif
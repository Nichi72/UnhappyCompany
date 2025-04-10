using System.Collections.Generic;
using UnityEngine;

public class RoomSetting : MonoBehaviour
{
    public List<Transform> eggSpawnPoints;
    public List<Transform> itemSpawnPoints;
    public List<Light> roomLights;

    [ContextMenu("Update Room Lights")]
    private void UpdateRoomLights()
    {
        roomLights = new List<Light>(GetComponentsInChildren<Light>());
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}

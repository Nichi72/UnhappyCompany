using UnityEngine;

public class RoomTemplate : MonoBehaviour
{
    public enum DoorDirection
    {
        North,
        South,
        East,
        West
    }

    [System.Serializable]
    public class DoorSocket
    {
        public bool hasDoor;
        public DoorDirection direction;
        public Vector3 doorPosition;
    }

    public DoorSocket[] doorSockets;
    public Vector3 roomSize = new Vector3(10f, 4f, 10f);
    public string roomType = "Basic";  // Basic, Special, Boss 등

    void OnDrawGizmos()
    {
        // 방의 경계를 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, roomSize);

        // 문 위치 시각화
        if (doorSockets != null)
        {
            foreach (var door in doorSockets)
            {
                if (door.hasDoor)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(transform.position + door.doorPosition, 0.5f);
                }
            }
        }
    }
} 
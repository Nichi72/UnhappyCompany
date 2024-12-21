using UnityEngine;

public class DoorEdge : MonoBehaviour
{
    public enum DoorType
    {
        // 아직 너머에 방이 없는 문
        Empty,
        // 벽으로 매워진 문
        Wall,
        // 너머에 방이 있는 문
        Room
    }
    // 먼저 채워지는 노드
    [ReadOnly]public RoomNode formRoomNode;
    // 나중에 채워지는 노드
    [ReadOnly]public RoomNode toRoomNode;
    public DoorType currentDoorType;
    public bool isVisited => toRoomNode != null;
    void Awake()
    {
        formRoomNode = GetComponentInParent<RoomNode>();
    }
    
    // Update is called once per frame
    void Update()
    {
        // if(formRoomNode == nu)
        // {

        // }

        if(toRoomNode == formRoomNode)
        {
            Debug.Break();
        }
    }
}

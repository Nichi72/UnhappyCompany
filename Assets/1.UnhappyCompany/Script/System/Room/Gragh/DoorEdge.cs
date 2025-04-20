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
    
    // 문의 방향을 표시하는 열거형
    
    
    // 먼저 채워지는 노드
    [ReadOnly]public RoomNode formRoomNode;
    // 나중에 채워지는 노드
    [ReadOnly]public RoomNode toRoomNode;
    public DoorType currentDoorType;
    public DoorDirection direction; // 문의 방향
    public bool isVisited => toRoomNode != null;
    
    void Awake()
    {
        formRoomNode = GetComponentInParent<RoomNode>();
        // DetermineDirection();
    }
    void Update()
    {
        // DetermineDirection();
    }

    // 문의 방향을 결정하는 메서드

    public void DetermineDirection()
    {
        // 월드 좌표에서의 forward 방향을 기준으로 방향 결정
        Vector3 worldForward = transform.position + transform.forward - transform.position;
        worldForward.Normalize();

        // 각 방향과의 각도 계산
        float angleNorth = Vector3.Angle(worldForward, Vector3.forward);
        float angleSouth = Vector3.Angle(worldForward, Vector3.back);
        float angleEast = Vector3.Angle(worldForward, Vector3.right);
        float angleWest = Vector3.Angle(worldForward, Vector3.left);
        float angleUp = Vector3.Angle(worldForward, Vector3.up);
        float angleDown = Vector3.Angle(worldForward, Vector3.down);

        // 가장 작은 각도를 가진 방향 선택
        float minAngle = Mathf.Min(angleNorth, angleSouth, angleEast, angleWest, angleUp, angleDown);

        if (minAngle == angleNorth) direction = DoorDirection.North;
        else if (minAngle == angleSouth) direction = DoorDirection.South;
        else if (minAngle == angleEast) direction = DoorDirection.East;
        else if (minAngle == angleWest) direction = DoorDirection.West;
        else if (minAngle == angleUp) direction = DoorDirection.Up;
        else direction = DoorDirection.Down;

        // 게임 오브젝트의 이름을 방향값으로 설정
        gameObject.name = direction.ToString();
    }
    
    // 에디터에서 방향을 시각적으로 표시
    void OnDrawGizmos()
    {
        // Awake가 호출되기 전에 에디터에서도 방향 결정
        if (Application.isEditor && !Application.isPlaying)
        {
            DetermineDirection();
        }
        
        // 방향에 따라 색상 표시
        switch (this.direction)
        {
            case DoorDirection.North: Gizmos.color = Color.blue; break;
            case DoorDirection.South: Gizmos.color = Color.magenta; break;
            case DoorDirection.East: Gizmos.color = Color.red; break;
            case DoorDirection.West: Gizmos.color = Color.green; break;
            case DoorDirection.Up: Gizmos.color = Color.white; break;
            case DoorDirection.Down: Gizmos.color = Color.black; break;
            default: Gizmos.color = Color.cyan; break;
        }
        
        // 월드 방향으로 광선 그리기
        Vector3 worldDirection = (transform.position + transform.forward - transform.position).normalized * 4.0f;
        Gizmos.DrawRay(transform.position, worldDirection);
        
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    
}

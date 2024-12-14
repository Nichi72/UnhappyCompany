using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    public enum Direction { Up, Down, Right, Left }
    public Direction connectionDirection;

    [HideInInspector]
    public bool isConnected = false;

    private void OnDrawGizmos()
    {
        float arrowLength = 1f;
        Vector3 direction = Vector3.zero;
        
        switch(connectionDirection)
        {
            case Direction.Up:
                direction = Vector3.forward;
                break;
            case Direction.Down:
                direction = Vector3.back;
                break;
            case Direction.Right:
                direction = Vector3.right;
                break;
            case Direction.Left:
                direction = Vector3.left;
                break;
        }

        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end = start + direction * arrowLength;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.1f);
    }

    public bool CanExpand()
    {
        return !isConnected;
    }

    // 반대 방향을 반환하기 위한 헬퍼 함수
    public static Direction GetOppositeDirection(Direction dir)
    {
        switch(dir)
        {
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            case Direction.Right: return Direction.Left;
            case Direction.Left: return Direction.Right;
        }
        return Direction.Up; // 기본값
    }
}

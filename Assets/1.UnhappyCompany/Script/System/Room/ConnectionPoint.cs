using UnityEngine;
    public enum Direction {
    North,
    East,
    South,
    West,
    ForTest
}
public class ConnectionPoint : MonoBehaviour 
{

    public Direction currentDirection = Direction.North;

    // 방향 enum을 실제 세계 방향 벡터로 매핑
    public Vector3 GetDirectionVector(Direction dir) {
        switch (dir) {
            case Direction.North: return Vector3.forward; // z+
            case Direction.East:  return Vector3.right;   // x+
            case Direction.South: return Vector3.back;    // z-
            case Direction.West:  return Vector3.left;    // x-
        }
        return Vector3.forward;
    }

    // 반대 방향을 반환하는 헬퍼 함수
    public Direction GetOppositeDirection(Direction dir) {
        switch (dir) {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East:  return Direction.West;
            case Direction.West:  return Direction.East;
        }
        return Direction.North;
    }

    // 디버그용 기즈모 표시(에디터 상에서 문 방향 확인)
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, GetDirectionVector(currentDirection) * 1f);
    }
}

using UnityEngine;

public class DoorBtn : MonoBehaviour , InteractionF
{
    public Door door;
    public void HitEvent()
    {
        Debug.Log("Hit");
        // 효과음

        // 기능처리
        door.OpenCloseDoor();

    }
}

using UnityEngine;

public class DoorBtn : MonoBehaviour , InteractionF
{
    public Door door;
    public void HitEventInteractionF(Player rayOrigin)
    {
        Debug.Log("Hit");
        // 효과음

        // 기능처리
        door.OpenCloseDoor();
    }
}

using UnityEngine;

public class DoorBtn : MonoBehaviour , InteractionF
{
    public Door door;
    public void HitEventInteractionF(Player rayOrigin)
    {
        Debug.Log("Hit");
        // ȿ����

        // ���ó��
        door.OpenCloseDoor();
    }
}

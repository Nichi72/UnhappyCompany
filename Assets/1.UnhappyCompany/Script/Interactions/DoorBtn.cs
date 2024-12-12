using UnityEngine;

public class DoorBtn : MonoBehaviour , IInteractable
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

using UnityEngine;

public class DoorBtn : MonoBehaviour , InteractionF
{
    public Door door;
    public void HitEvent()
    {
        Debug.Log("Hit");
        // ȿ����

        // ���ó��
        door.OpenCloseDoor();

    }
}

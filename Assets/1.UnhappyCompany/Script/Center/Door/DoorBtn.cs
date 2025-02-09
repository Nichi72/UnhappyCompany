using UnityEngine;
using MyUtility;

public class DoorBtn : MonoBehaviour , IInteractable
{
    public Door door;

    public string InteractionText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "DoorBtn_ITR"); set => InteractionText = value; }

    public void HitEventInteractionF(Player rayOrigin)
    {
        Debug.Log("Hit");
        door.OpenCloseDoor();
    }
}

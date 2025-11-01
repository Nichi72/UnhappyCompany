using UnityEngine;
using MyUtility;

public class DoorBtn : MonoBehaviour , IInteractableF
{
    public Door door;
    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "DoorBtn_ITR"); set => InteractionTextF = value; }
    public bool IgnoreInteractionF { get; set; } = false;

    public void HitEventInteractionF(Player rayOrigin)
    {
        Debug.Log("Hit");
        AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.doorButtonPress, transform, 20f, "Door Button Press");
        door.OpenCloseDoor();
    }
}

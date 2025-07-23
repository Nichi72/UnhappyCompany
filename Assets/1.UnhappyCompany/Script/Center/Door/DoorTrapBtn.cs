using UnityEngine;
using MyUtility;

public class DoorTrapBtn : MonoBehaviour,IInteractableF
{
    [SerializeField] private DoorTrapBtnType doorTrapBtnType;
    [SerializeField] private DoorTrapFire doorTrapFire;
    [SerializeField] private DoorTrapWaterCannon doorTrapWaterCannon;
    [SerializeField] private DoorTrapHammer doorTrapHammer;
    
    

    public bool IgnoreInteractionF { get; set; } = false;
    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "DoorTrapBtn_ITR"); set => InteractionTextF = value; }

    public void HitEventInteractionF(Player rayOrigin)
    {
        if(doorTrapBtnType == DoorTrapBtnType.Fire)
        {
            FireTrap();
        }
        else if(doorTrapBtnType == DoorTrapBtnType.Water)
        {
            WaterTrap();
        }
        else if(doorTrapBtnType == DoorTrapBtnType.Hammer)
        {
            HammerTrap();
        }
        else if(doorTrapBtnType == DoorTrapBtnType.Electric)
        {
            ElectricTrap();
        }
    }


    void FireTrap()
    {
        doorTrapFire.On();
        Debug.Log("FireTrap");
    }

    void WaterTrap()
    {
        doorTrapWaterCannon.On();
        Debug.Log("WaterTrap");
    }

    void HammerTrap()
    {
        doorTrapHammer.On();
        Debug.Log("HammerTrap");
    }

    void ElectricTrap()
    {
        Debug.Log("ElectricTrap");
    }
}

using UnityEngine;
using MyUtility;

public class DoorTrapBtn : MonoBehaviour,IInteractable
{
    enum DoorTrapBtnType{
        Fire,
        Water,
        Hammer,
        Electric
    }
    [SerializeField] private DoorTrapBtnType doorTrapBtnType;

    public string InteractionText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "DoorTrapBtn_ITR"); set => InteractionText = value; }

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
        Debug.Log("FireTrap");
    }

    void WaterTrap()
    {
        Debug.Log("WaterTrap");
    }

    void HammerTrap()
    {
        Debug.Log("HammerTrap");
    }

    void ElectricTrap()
    {
        Debug.Log("ElectricTrap");
    }
}

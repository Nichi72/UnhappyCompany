using UnityEngine;
using FMODUnity;
using System.Collections.Generic;


public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance;
    
    [field: Header("Damage")]
    [field: SerializeField] public EventReference missDamage { get; private set; }
    [field: SerializeField] public EventReference damage { get; private set; }
    
    [field: Header("UI Sounds")]
    [field: SerializeField] public EventReference uiButtonClick { get; private set; }
    [field: SerializeField] public EventReference uiButtonHover { get; private set; }
    [field: SerializeField] public EventReference uiPopupOpen { get; private set; }
    [field: SerializeField] public EventReference shopItemBuy { get; private set; }
    [field: SerializeField] public EventReference shopItemDrop { get; private set; }
    [field: SerializeField] public EventReference startButtonPress { get; private set; }
    
    [field: Header("Enemy - Rampage")]
    [field: SerializeField] public EventReference rampageHitBlock { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionWall { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionPlayer { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionObject { get; private set; }
    [field: SerializeField] public EventReference rampageChargePrep { get; private set; }
    
    [field: Header("Traps")]
    [field: SerializeField] public EventReference trapWaterCannonFire { get; private set; }
    [field: SerializeField] public EventReference trapWaterCannonAlreadyActive { get; private set; }
    [field: SerializeField] public EventReference trapWaterCannonLimitExceeded { get; private set; }
    [field: SerializeField] public EventReference trapHammerStrike { get; private set; }
    [field: SerializeField] public EventReference trapHammerAlreadyActive { get; private set; }
    [field: SerializeField] public EventReference trapHammerLimitExceeded { get; private set; }
    [field: SerializeField] public EventReference trapFireIgnite { get; private set; }
    [field: SerializeField] public EventReference trapFireAlreadyActive { get; private set; }
    [field: SerializeField] public EventReference trapFireLimitExceeded { get; private set; }
    
    [field: Header("RSP Game")]
    [field: SerializeField] public EventReference rspWin { get; private set; }
    [field: SerializeField] public EventReference rspLose { get; private set; }
    [field: SerializeField] public EventReference rspWheelSpin { get; private set; }
    [field: SerializeField] public List<EventReference> rspStack { get; private set; }
    
    [field: Header("TEST")]
    [field: SerializeField] public EventReference TEST { get; private set; }
    [field: SerializeField] public EventReference TestBeep { get; private set; }



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }   
}

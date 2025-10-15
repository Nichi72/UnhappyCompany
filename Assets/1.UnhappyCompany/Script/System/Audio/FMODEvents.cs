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
    [field: SerializeField] public EventReference shopItemBuy { get; private set; }
    [field: SerializeField] public EventReference shopItemDrop { get; private set; }
    [field: SerializeField] public EventReference startButtonPress { get; private set; }
    [field: SerializeField] public EventReference doorButtonPress { get; private set; }

	[field: Header("Computer")]
	[field: SerializeField] public EventReference computerCursorClick { get; private set; }
	[field: SerializeField] public EventReference computerCursorHover { get; private set; }
	[field: SerializeField] public EventReference ComputerPopupOpen { get; private set; }
	[field: SerializeField] public EventReference computerScreenClose { get; private set; }
	
	[field: Header("Ambience")]
	[field: SerializeField] public EventReference computerAmb { get; private set; }

    
    [field: Header("Enemy - Rampage")]
    [field: SerializeField] public EventReference rampageHitBlock { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionWall { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionPlayer { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionObject { get; private set; }
    [field: SerializeField] public EventReference rampageChargePrep { get; private set; }
    
    [field: Header("Items - Cushion")]
    [field: SerializeField] public EventReference cushionDeployStart { get; private set; }      // 쿠션 설치 시작 (확정 시)
    [field: SerializeField] public EventReference cushionDeployUnfold { get; private set; }     // 쿠션 펼치는 중 (애니메이션 진행)
    [field: SerializeField] public EventReference cushionDeployComplete { get; private set; }   // 쿠션 설치 완료 (펴짐 완료)
    [field: SerializeField] public EventReference cushionRetract { get; private set; }          // 쿠션 회수 사운드 (Phase 2)
    [field: SerializeField] public EventReference cushionImpact { get; private set; }           // Rampage 충돌 사운드 (Phase 3)
    
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

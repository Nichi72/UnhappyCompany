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

    [field: Header("Door")]
    [field: SerializeField] public EventReference doorButtonPress { get; private set; }
    [field: SerializeField] public EventReference CenterDoorOpen { get; private set; }
    [field: SerializeField] public EventReference CenterDoorClose { get; private set; }
    [field: SerializeField] public EventReference CenterDoorButton { get; private set; }

    [field: Header("Item Sell")]
    [field: SerializeField] public EventReference SellLeverDown { get; private set; }
    [field: SerializeField] public EventReference SellItemLight { get; private set; }
    [field: SerializeField] public EventReference SellItemHeavy { get; private set; }
    [field: SerializeField] public EventReference SellDoorOpen { get; private set; }
    [field: SerializeField] public EventReference SellDoorClose { get; private set; }
    [field: SerializeField] public EventReference SellItemSubtract { get; private set; }
    [field: SerializeField] public EventReference SellItemComplete { get; private set; }

	[field: Header("Computer")]
	[field: SerializeField] public EventReference computerCursorClick { get; private set; }
	[field: SerializeField] public EventReference computerCursorHover { get; private set; }
	[field: SerializeField] public EventReference ComputerPopupOpen { get; private set; }
	[field: SerializeField] public EventReference computerScreenClose { get; private set; }
	
	[field: Header("Ambience")]
	[field: SerializeField] public EventReference computerAmb { get; private set; }

    
    [field: Header("Enemy - Rampage")]
    [field: Header("Rampage - Engine/Movement Sounds")]
    [field: SerializeField] public EventReference rampageIdle { get; private set; }          // 공회전 루프 사운드
    [field: SerializeField] public EventReference rampageStart { get; private set; }         // 출발 시 RPM 상승 사운드
    [field: SerializeField] public EventReference rampageMoveLoop { get; private set; }      // 바퀴/이동 루프 사운드
    [field: SerializeField] public EventReference rampageDrift { get; private set; }         // 드리프트 사운드
    
    [field: Header("Rampage - Collision Sounds")]
    [field: SerializeField] public EventReference rampageHitBlock { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionPlayer { get; private set; }
    [field: SerializeField] public EventReference rampageCollisionObject { get; private set; }
    
    [field: Header("Rampage - Panel Damage Sounds")]
    [field: SerializeField] public EventReference rampagePanelDamage { get; private set; }        // 패널 공격받을 때 소리
    
    [field: Header("Rampage - Wall Collision HP Damage Sounds")]
    [field: SerializeField] public EventReference rampageWallHitLevel1 { get; private set; }      // 벽 충돌 1단계 (HP 많음 66%+)
    [field: SerializeField] public EventReference rampageWallHitLevel2 { get; private set; }      // 벽 충돌 2단계 (HP 중간 33-66%)
    [field: SerializeField] public EventReference rampageWallHitLevel3 { get; private set; }      // 벽 충돌 3단계 (HP 적음 33%-)
    
    [field: Header("Rampage - Destruction Sounds")]
    [field: SerializeField] public EventReference rampageBreakWarning { get; private set; }  // 부서지기 직전 소리
    [field: SerializeField] public EventReference rampageBreak { get; private set; }         // 부서지는 소리
    [field: SerializeField] public EventReference rampageExplode { get; private set; }         // 폭발 소리
    
    [field: Header("Items - Cushion")]
    [field: SerializeField] public EventReference cushionDeployStart { get; private set; }      // 쿠션 설치 시작 (확정 시)
    [field: SerializeField] public EventReference cushionDeployUnfold { get; private set; }     // 쿠션 펼치는 중 (애니메이션 진행)
    [field: SerializeField] public EventReference cushionDeployComplete { get; private set; }   // 쿠션 설치 완료 (펴짐 완료)
    [field: SerializeField] public EventReference cushionRetract { get; private set; }          // 쿠션 회수 사운드 (Phase 2)
    [field: SerializeField] public EventReference cushionImpact { get; private set; }           // Rampage 충돌 사운드 (Phase 3)
    
    [field: Header("Items - Gift Box")]
    [field: SerializeField] public EventReference giftBoxOpen { get; private set; }             // 선물상자 열기 소리
    
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

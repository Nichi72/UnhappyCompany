// 중앙 배터리를 사용하는 구체적인 클래스 1: 설치된 조명
using UnityEngine;

public class CentralLight : CentralBatteryConsumer
{
    //// 초당 소모되는 배터리 양
    //protected override float BatteryDrainPerSecond => 10.0f;

    // 배터리 소모 메서드 (기본 동작을 유지하거나 수정 가능)
    public override void DrainBattery()
    {
        base.DrainBattery(); // 기본 배터리 소모 기능 사용
        // 추가적인 행동이 필요하다면 여기에 작성
    }
}
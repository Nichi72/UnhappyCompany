using UnityEngine;

public class Abstractions
{
    
}

/// <summary>
/// 오브젝트가 월드에 존재하고 있을 때 플레이어에 의해 레이캐스트를 맞았을 때 호출되는 함수
/// </summary>
/// <param name="player">레이캐스트를 쏜 플레이어</param>
/// <param name="raycastHit">맞은 개체</param>
public interface InteractionF
{
    public void HitEventInteractionF(Player rayOrigin);

}
/// <summary>
/// 센터의 배터리를 닳게하는 아이템
/// </summary>

public interface ICentralBatteryConsumer
{
    float BatteryDrainPerSecond { get; set; }
    void DrainBattery();
    string GetConsumerName();
}
/// <summary>
/// 데미지를 받을 수 있는 개체 인터페이스=
/// </summary>
public interface IDamageable
{
    public int hp { get; set; }
    void TakeDamage(int damage);
}
/// <summary>
/// 데미지를 줄 수 있는 개체 인터페이스
/// </summary>
public interface IDamager
{
    public int damage { get; set; }
    void DealDamage(IDamageable target);
}




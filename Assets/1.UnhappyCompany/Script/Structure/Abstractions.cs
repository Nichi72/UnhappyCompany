using UnityEngine;

public class Abstractions
{
    
}

/// <summary>
/// 인터렉션
/// </summary>
public interface InteractionF
{
    public void HitEventInteractionF(Player rayOrigin , RaycastHit hitObject);

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




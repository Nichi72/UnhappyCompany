using UnityEngine;

public class Abstractions
{
    
}

/// <summary>
/// ���ͷ���
/// </summary>
public interface InteractionF
{
    public void HitEventInteractionF(Player rayOrigin , RaycastHit hitObject);

}
/// <summary>
/// ������ ���͸��� ����ϴ� ������
/// </summary>

public interface ICentralBatteryConsumer
{
    float BatteryDrainPerSecond { get; set; }
    void DrainBattery();
    string GetConsumerName();
}
/// <summary>
/// �������� ���� �� �ִ� ��ü �������̽�=
/// </summary>
public interface IDamageable
{
    public int hp { get; set; }
    void TakeDamage(int damage);
}
/// <summary>
/// �������� �� �� �ִ� ��ü �������̽�
/// </summary>
public interface IDamager
{
    public int damage { get; set; }
    void DealDamage(IDamageable target);
}




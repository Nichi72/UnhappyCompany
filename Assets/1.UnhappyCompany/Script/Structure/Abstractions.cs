using UnityEngine;

/// <summary>
/// 플레이어와 상호 작용할 때 호출되는 메서드를 정의하는 인터페이스입니다.
/// </summary>
/// <param name="player">상호 작용하는 플레이어</param>
public interface IInteractableF
{
    bool IgnoreInteractionF { get; set; }
    string InteractionTextF { get; set; }
    void HitEventInteractionF(Player rayOrigin);

}

public interface IInteractableE
{
    bool IgnoreInteractionE { get; set; }
    string InteractionTextE { get; set; }
    void HitEventInteractionE(Player rayOrigin);
}

public interface IToolTip
{
    string ToolTipText { get; set; }
    string ToolTipText2 { get; set; }
    string ToolTipText3 { get; set; }
}


/// <summary>
/// 중앙 배터리를 소모하는 소비자를 정의하는 인터페이스입니다.
/// </summary>
public interface ICentralBatteryConsumer
{
    string ID { get; }
    float BatteryDrainPerSecond { get; set; }
    void DrainBattery();
    string GetConsumerName();
}

/// <summary>
/// 피해를 입을 수 있는 객체를 정의하는 인터페이스입니다.
/// </summary>
public interface IDamageable
{
    int hp { get; set; }
    void TakeDamage(int damage, DamageType damageType);
}


/// <summary>
/// 피해를 줄 수 있는 객체를 정의하는 인터페이스입니다.
/// </summary>
public interface IDamager
{
    // int damage { get; set; }
    public void DealDamage(int damage, IDamageable target);
}

/// <summary>
/// 업데이트를 오버라이드하는 인터페이스입니다.
/// </summary>
public interface IItemOverrideUpdate
{
    void OverrideUpdate();
}

public interface ISavable
{
    void SaveState();
    void LoadState();
}

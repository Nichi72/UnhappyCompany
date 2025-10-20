using UnityEngine;

public class RampagePanel : MonoBehaviour , IDamageable
{
    public RampageAIController controller;
    private int _hp = 1;
    public int hp { get => _hp; set => _hp = value; }
    // 데미지 입은 후 한번만 패널 닫기
    private bool onceDamage = false;

    public void TakeDamage(int damage, DamageType damageType)
    {
        // Debug.Break();
        Debug.Log("Panel 데미지 입음");
        hp -= damage;
        
        // 데미지 사운드 재생
        PlayDamageSound();
    }
    void Update()
    {
        if(onceDamage == true)
        {
            return;
        }
        
        if(hp <= 0)
        {
            ClosePanel();
        }
    }

    public void OpenPanel()
    {
        hp = 1;
        onceDamage = false;
    }

    private void ClosePanel()
    {
        hp = 0;
        onceDamage = true;
        controller.CurrentPanelHealth--;
        
        // 패널 비활성화 (나중에 애니메이션으로 교체 예정)
        DeactivatePanel();
    }

    /// <summary>
    /// 패널 비활성화 처리
    /// TODO: 나중에 애니메이션이나 이펙트로 교체 예정
    /// </summary>
    private void DeactivatePanel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 데미지 사운드 재생
    /// </summary>
    private void PlayDamageSound()
    {
        if (AudioManager.instance != null && FMODEvents.instance != null && 
            !FMODEvents.instance.rampagePanelDamage.IsNull)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rampagePanelDamage, transform, "RampagePanel 데미지 사운드");
        }
    }
}

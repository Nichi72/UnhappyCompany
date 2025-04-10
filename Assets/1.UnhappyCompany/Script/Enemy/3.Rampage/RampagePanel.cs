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
    }

    private void ClosePanel()
    {
        hp = 0;
        onceDamage = true;
        controller.CurrentPanelHealth--;
    }


}

using UnityEngine;

public class RampagePanel : MonoBehaviour , IDamageable
{
    public RampageAIController controller;
    private int hp = 1;
    public int Hp { get => hp; set => hp = value; }
    // 데미지 입은 후 한번만 패널 닫기
    private bool onceDamage = false;

    public void TakeDamage(int damage, DamageType damageType)
    {
        // Debug.Break();
        Debug.Log("Panel 데미지 입음");
        Hp -= damage;
    }
    void Update()
    {
        if(onceDamage == true)
        {
            return;
        }
        
        if(Hp <= 0)
        {
            ClosePanel();
        }
    }

    public void OpenPanel()
    {
        Hp = 1;
    }

    private void ClosePanel()
    {
        Hp = 0;
        onceDamage = true;
        controller.CurrentPanelHealth--;
    }


}

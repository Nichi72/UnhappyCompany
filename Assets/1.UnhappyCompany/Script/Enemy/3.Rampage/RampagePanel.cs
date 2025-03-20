using UnityEngine;

public class RampagePanel : MonoBehaviour , IDamageable
{
    public RampageAIController controller;
    private int hp = 1;
    public int Hp { get => hp; set => hp = value; }

    public void TakeDamage(int damage, DamageType damageType)
    {
        Debug.Break();
        Debug.Log("Panel 데미지 입힘");
        Hp -= damage;
    }
    void Update()
    {
        if(Hp <= 0)
        {
            controller.currentPanelHealth--;
        }
    }

    public void OpenPanel()
    {
        Hp = 1;
    }


}

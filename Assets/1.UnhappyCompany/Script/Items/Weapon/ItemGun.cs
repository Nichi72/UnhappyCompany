using UnityEngine;

/// <summary>
/// 총을 사용하는 아이템입니다.
/// 총 종류의 아이템들을 해당 클래스에서 상속받아 사용합니다.
/// 총 종류는 왼쪽 클릭 버튼을 눌러서 사용합니다. 
/// </summary>
public class ItemGun : Item, IDamager, IOverrideUpdate
{
   
    public Animator playerArmAnimator;
    private string animatorShootName;
    private string animatorCockingName;
    private string animatorReloadName;

    public int damage { get; set; } = 10;
    
    public override void Use(Player player)
    {
        // 사용하지 않음.
    }

    public void DealDamage(IDamageable target)
    {
        
    }


    
    public void OverrideUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            playerArmAnimator.Play(animatorShootName);
        }
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            playerArmAnimator.Play(animatorCockingName);
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            playerArmAnimator.Play(animatorReloadName);
        }
    }
}

using UnityEngine;

/// <summary>
/// 총을 사용하는 아이템입니다.
/// 총 종류의 아이템들을 해당 클래스에서 상속받아 사용합니다.
/// 총 종류는 왼쪽 클릭 버튼을 눌러서 사용합니다. 
/// </summary>
public class ItemGun : Item, IDamager, IOverrideUpdate
{
   
    public Animator playerAnimator;
    public int damage { get; set; } = 10;
    
    public override void Use(Player player)
    {
        // 사용하지 않음.
    }

    public void DealDamage(IDamageable target)
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    
    public void OverrideUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            
        }
    }

    public void Shoot()
    {
        // animator.SetTrigger("Shoot");
    if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
    {
        playerAnimator.Play("Shoot");
    }
    }
    public void Reload()
    {
        playerAnimator.SetTrigger("Reload");
    }
    public void Cocking()
    {
        playerAnimator.SetTrigger("Cocking");
    }

}

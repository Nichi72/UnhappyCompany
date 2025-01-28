using UnityEngine;

/// <summary>
/// 총을 사용하는 아이템입니다.
/// 총 종류의 아이템들을 해당 클래스에서 상속받아 사용합니다.
/// 총 종류는 왼쪽 클릭 버튼을 눌러서 사용합니다. 
/// </summary>
public class ItemGun : Item, IDamager, IOverrideUpdate, IAnimatorLayer
{
   
    public Animator playerArmAnimator;
    private string animatorShootName;
    private string animatorCockingName;
    private string animatorReloadName;


    public int damage { get; set; } = 10;
    public string animatorLayerName { get; set; } = "Gun";

    public override void Use(Player player)
    {
        // 사용하지 않음.
    }

    public void DealDamage(IDamageable target)
    {
        
    }
    public override void PickUp(Player player)
    {
        base.PickUp(player);
    }
    public override void Mount(Player player)
    {
        base.Mount(player);


        Vector3 gunPosition = new Vector3(-0.0459837541f,0.233345002f,0.0039123809f);
        Vector3 gunRotation = new Vector3(331.509766f,106.864601f,57.3288536f);
        Vector3 gunScale = new Vector3(0.554564357f,0.554564595f,0.554564416f);


        Rigidbody rd =  GetComponent<Rigidbody>();
        rd.isKinematic = true;
        playerArmAnimator = player.armAnimator;
        int layerIndex = playerArmAnimator.GetLayerIndex(animatorLayerName);
        playerArmAnimator.SetLayerWeight(layerIndex, 1);
        
        transform.localPosition = gunPosition;
        transform.localRotation = Quaternion.Euler(gunRotation);
        transform.localScale = gunScale;
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

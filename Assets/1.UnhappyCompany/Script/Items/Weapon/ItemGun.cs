using UnityEngine;
using MyUtility;
using UnityEditor.Rendering;

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
    public string animatorLayerName { get; set; } = "Gun";
    public override string ToolTipText { get; set; } = "[LMB]: "; // 발사
    public override string ToolTipText2 { get; set; } = "[RMB]: "; // 코킹
    public override string ToolTipText3 { get; set; } = "[R]: "; // 재장전

    public override void Use(Player player)
    {
        // 사용하지 않음.
    }

    
    public override void PickUp(Player player)
    {
        base.PickUp(player);
    }
    public override void Mount(Player player, object state = null)
    {
        base.Mount(player, state);

        Vector3 gunPosition = new Vector3(-0.0459837541f,0.233345002f,0.0039123809f);
        Vector3 gunRotation = new Vector3(331.509766f,106.864601f,57.3288536f);
        Vector3 gunScale = new Vector3(0.554564357f,0.554564595f,0.554564416f);

        ToolTipText += LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemGun_TT_LMB");
        ToolTipText2 += LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemGun_TT_RMB");
        ToolTipText3 += LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemGun_TT_R");

        ToolTipUI.instance.SetToolTip(this);

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
            Fire();
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

    private void Fire()
    {
        playerArmAnimator.Play(animatorShootName);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DealDamage(damageable);
            }
        }

    }

    public void DealDamage(IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Nomal);
        Debug.Log("Hit!");
    }

    // public void DealDamage(IDamageable target)
    // {
    //     if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f))
    //     {
    //         IDamageable damageable = hit.collider.GetComponent<IDamageable>();
    //         if (damageable != null)
    //         {
    //             damageable.TakeDamage(damage, DamageType.Nomal);
    //             Debug.Log("Hit!");
    //         }
    //     }
    // }

}

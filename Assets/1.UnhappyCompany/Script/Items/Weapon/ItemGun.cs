 using UnityEngine;
using MyUtility;
using UnityEditor.Rendering;

/// <summary>
/// 총을 사용하는 아이템입니다.
/// 총 종류의 아이템들을 해당 클래스에서 상속받아 사용합니다.
/// 총 종류는 왼쪽 클릭 버튼을 눌러서 사용합니다. 
/// </summary>
public class ItemGun : Item, IDamager, IItemOverrideUpdate
{
   
    


    public int damage { get; set; } = 10;
    public string animatorLayerName { get; set; } = "Gun";
    public override string ToolTipText { get; set; } = "[LMB]: "; // 발사
    public override string ToolTipText2 { get; set; } = "[RMB]: "; // 코킹
    public override string ToolTipText3 { get; set; } = "[R]: "; // 재장전
    public LayerMask DamageLayer { get => damageLayer; set => damageLayer = value; }
    public float Distance { get => distance; set => distance = value; }

    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private float distance;
    // 리볼버 장탄 수
    [ReadOnly] public int maxAmmo = 6;
    [ReadOnly] public int currentAmmo;
    public bool isCocking = false;
    


    #region 애니메이션 프로퍼티
    [Header("Animation")]
    public Animator playerArmAnimator;
    public Animator playerItemAnimator;
    // 아이템 애니메이션 이름
    private string animatonIdleName;
    private string animatorShootName;
    private string animatorCockingName;
    private string animatorReloadName;

    // 팔 애니메이션 이름
    private string animatorArmIdleName;
    private string animatorArmShootName;
    private string animatorArmCockingName;
    private string animatorArmReloadName;
#endregion

    public override void Use(Player player)
    {
        // 사용하지 않음. OverrideUpdate로 대체함.
    }

    private void Awake()
    {
        
        
    }
    public override void PickUp(Player player)
    {
        base.PickUp(player);
    }
    public override void Mount(Player player, object state = null)
    {
        isModelHandAnimation = true;
        base.Mount(player, state);
        
        animatonIdleName = "Gun_Idle_Item";
        animatorShootName = "Gun_Shot_Item";
        animatorCockingName = "Gun_Cocking_Item"; // 이거 아직 안됨.
        animatorReloadName = "Gun_Reload_Item";

        animatorArmIdleName = "Gun_Idle_Arm";
        animatorArmShootName = "Gun_Shot_Arm";
        animatorArmCockingName = "Gun_Cocking_Arm";// 이거 아직 안됨.
        animatorArmReloadName = "Gun_Reload_Arm";

        Vector3 gunPosition = new Vector3(-0.0459837541f,0.233345002f,0.0039123809f);
        Vector3 gunRotation = new Vector3(331.509766f,106.864601f,57.3288536f);
        Vector3 gunScale = new Vector3(0.554564357f,0.554564595f,0.554564416f);

        ToolTipText += LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemGun_TT_LMB");
        ToolTipText2 += LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemGun_TT_RMB");
        ToolTipText3 += LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemGun_TT_R");

        isModelHandAnimation = true;

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
            Cock();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    private void Fire()
    {
        if(isCocking == false)
        {
            Debug.Log("Cocking이 안되어있어서 쏘지 못함");
            return;
        }
        if(currentAmmo <= 0)
        {
            Debug.Log("장탄이 없어서 쏘지 못함");
            return;
        }

        playerArmAnimator.Play(animatorArmShootName);
        playerItemAnimator.Play(animatorShootName);
        DamageSystem.RaycastDamage(damage, 100f, damageLayer, DealDamage);
        isCocking = false;
        currentAmmo--;
    }
    private void Cock()
    {
        if(isCocking == true)
        {
            Debug.Log("Cocking이 되어있어서 코킹 못함");
            return;
        }
        playerArmAnimator.Play(animatorArmCockingName);
        playerItemAnimator.Play(animatorCockingName);
        isCocking = true;
    }
    private void Reload()
    {
        playerArmAnimator.Play(animatorArmReloadName);
        playerItemAnimator.Play(animatorReloadName);
        currentAmmo = maxAmmo;
    }

    public void DealDamage(int damage, IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Nomal);
        Debug.Log("Hit!");
    }

    
}

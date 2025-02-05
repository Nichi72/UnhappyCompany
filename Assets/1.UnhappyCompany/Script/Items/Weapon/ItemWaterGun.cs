using System.Collections;
using UnityEngine;

public class ItemWaterGun : Item, IDamager, IOverrideUpdate , IAnimatorLayer
{
    public readonly string animatorStartName = "Shot_Water_Start";
    public readonly string animatorLoopName = "Shot_Water_Loop";
    public readonly string animatorFinishName = "Shot_Water_Finish";
    public readonly string animatorLoadWaterName = "Load_Water";
    public readonly string animatorLoadAirName = "Load_Air";

    private Animator playerArmAnimator;
    
    public int damage { get; set; } = 0; // 물총의 데미지 설정
    public string animatorLayerName { get; set; } = "WaterGun";
    public Vector3 waterGunPosition = new Vector3(-0.010254181f,0.0214034468f,0.206581041f);
    public Vector3 waterGunRotation = new Vector3(306.530121f,243.974197f,216.739578f);
    public Vector3 waterGunScale = new Vector3(0.554564357f,0.554564595f,0.554564416f);

    void Start()
    {
    }
    public override void Use(Player player)
    {
        base.Use(player);
    }
   

    private void Update()
    {
        // 물총의 업데이트 로직이 필요하다면 여기에 추가
    }
    
    public override void PickUp(Player player)
    {
        base.PickUp(player);
    }

    public override void Mount(Player player)
    {
        base.Mount(player);

        waterGunPosition = new Vector3(-0.010254181f,0.0214034468f,0.206581041f);
        waterGunRotation = new Vector3(306.530121f,243.974197f,216.739578f);
        waterGunScale = new Vector3(0.554564357f,0.554564595f,0.554564416f);


        Rigidbody rd =  GetComponent<Rigidbody>();
        rd.isKinematic = true;
        playerArmAnimator = player.armAnimator;
        int layerIndex = playerArmAnimator.GetLayerIndex(animatorLayerName);
        playerArmAnimator.SetLayerWeight(layerIndex, 1);
        
        transform.localPosition = waterGunPosition;
        transform.localRotation = Quaternion.Euler(waterGunRotation);
        transform.localScale = waterGunScale;
    }

    public void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        var interactionSystemTemp = MyUtility.ComponentUtils.GetAllComponentsInParents<InteractionSystem>(gameObject, true)[0];

        if (Physics.Raycast(ray, out hit, interactionSystemTemp.raycastMaxDistance, interactionSystemTemp.interactionLayer))
        {
            var damageAbleTemp = hit.transform.GetComponent<IDamageable>();
            if (damageAbleTemp == null)
            {
                return;
            }
            DealDamage(damageAbleTemp);
        }
    }

    public void DealDamage(IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Water); // 물 데미지 타입 사용
        Debug.Log($"{target.ToString()} Water Damage! _ Left HP { target.Hp}");
    }

    

    public void OverrideUpdate()
    {
        // 물총 발사
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            playerArmAnimator.Play(animatorStartName);
        }
        // 물총 발사 중
        if(Input.GetKey(KeyCode.Mouse0))
        {
            playerArmAnimator.Play(animatorLoopName);
        }
        // 물총 발사 완료
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            playerArmAnimator.Play(animatorFinishName);
        }
    }
} 
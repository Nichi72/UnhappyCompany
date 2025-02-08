using System.Collections;
using UnityEngine;

public class ItemWaterGun : Item, IDamager, IOverrideUpdate , IAnimatorLayer
{
    [ReadOnly] [SerializeField] private Animator playerArmAnimator;
    [SerializeField] private Animator itemAnimator;
    [SerializeField] private Transform handPivot;
    [SerializeField] private ParticleSystem vfxWater;

    [ReadOnly] [SerializeField] float airValue = 0;
    [ReadOnly] [SerializeField] float waterValue = 0;
    [ReadOnly] [SerializeField] float airMaxValue = 100;
    [ReadOnly] [SerializeField] float waterMaxValue = 100;
    [SerializeField] float airDecreaseValue = 1;
    [SerializeField] float waterDecreaseValue = 1;

    



    
    public int damage { get; set; } = 0; // 물총의 데미지 설정
    public string animatorLayerName { get; set; } = "WaterGun";


    public Vector3 waterGunPosition;
    public Vector3 waterGunRotation;
    public Vector3 waterGunScale;

    // 애니메이션 상태 이름
    public readonly string animatorItemStartName = "Shot_Water_Start";
    public readonly string animatorItemLoopName = "Shot_Water_Loop";
    public readonly string animatorItemFinishName = "Shot_Water_Finish";
    public readonly string animatorItemLoadWaterName = "Load_Water";
    public readonly string animatorItemLoadAirName = "Load_Air";



    // public readonly string animatorArmLayerName = "Shot_Start_Arm";
    public readonly string animatorArmStartName = "Arm_Shot_Water_Start";
    public readonly string animatorArmLoopName = "Arm_Shot_Water_Loop";
    public readonly string animatorArmFinishName = "Arm_Shot_Water_Finish";
    public readonly string animatorArmLoadWaterName = "Arm_Load_Water";
    public readonly string animatorArmLoadAirName = "Arm_Load_Air";






    void Awake()
    {
 
    }
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
        itemAnimator.enabled = true;
        // itemAnimator.applyRootMotion = true;
        handPivot = player.rightHandPos;
        transform.position = handPivot.position;
        // transform.parent = handPivot;
        

        // hand pivot 기준
        // waterGunPosition = new Vector3(0.714165092f,0.312598348f,0.881453872f);
        // waterGunRotation = new Vector3(4.73381901f,133.387222f,8.63204288f);    
        // waterGunScale = new Vector3(0.554564357f,0.554564595f,0.554564416f);

        // right hand 기준
        waterGunPosition = new Vector3(-0.0383468196f,0.165113255f,0.0990908518f);
        waterGunRotation = new Vector3(2.06121516f,245.157364f,281.128632f);  
        waterGunScale = new Vector3(0.554564357f,0.554564595f,0.554564416f);
        

        Rigidbody rd =  GetComponent<Rigidbody>();
        rd.isKinematic = true;
        playerArmAnimator = player.armAnimator;
        int layerIndex = playerArmAnimator.GetLayerIndex(animatorLayerName);
        playerArmAnimator.SetLayerWeight(layerIndex, 1);

        
        transform.localPosition = waterGunPosition;
        transform.localRotation = Quaternion.Euler(waterGunRotation);
        transform.localScale = waterGunScale;
        // 
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
        // handPivot.position = transform.position;
        // 물총 발사
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Fire();
        }
        // 물총 발사 중
        if(Input.GetKey(KeyCode.Mouse0))
        {
            airValue -= airDecreaseValue;
            waterValue -= waterDecreaseValue;
        }
        // 물총 발사 완료
        if(Input.GetKeyUp(KeyCode.Mouse0))

        {
            // itemAnimator.CrossFade(animatorItemFinishName,0.1f);
            playerArmAnimator.CrossFade(animatorArmFinishName,0.1f);
        }
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            // itemAnimator.applyRootMotion = false;
            itemAnimator.CrossFade(animatorItemLoadAirName,0.1f);
            playerArmAnimator.CrossFade(animatorArmLoadAirName,0.1f);
        }


    }

    private void Fire()
    {
        // itemAnimator.CrossFade(animatorItemStartName,0.1f);
        // itemAnimator.enabled = true;
        playerArmAnimator.CrossFade(animatorArmStartName,0.1f);
        vfxWater.Play();
    }





} 
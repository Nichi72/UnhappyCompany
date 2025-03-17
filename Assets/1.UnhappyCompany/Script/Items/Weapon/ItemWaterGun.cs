using System.Collections;
using MyUtility;
using Unity.Mathematics;
using UnityEngine;

public class ItemWaterGun : Item, IDamager, IOverrideUpdate
{
    [ReadOnly] [SerializeField] private Animator playerArmAnimator;
    [SerializeField] private Animator itemAnimator;
    [SerializeField] private Transform handPivot;
    [SerializeField] private ParticleSystem vfxWater;
    [SerializeField] private ParticleSystem vfxWaterSmoke;

    [ReadOnly] [SerializeField] float currentAirValue = 100;
    [ReadOnly] [SerializeField] float currentWaterValue = 100;
    [ReadOnly] [SerializeField] float airMaxValue = 100;
    [ReadOnly] [SerializeField] float waterMaxValue = 100;
    [SerializeField] float airDecreaseValue = 0.8f;
    [SerializeField] float waterDecreaseValue = 0.01f;


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

    override public string ToolTipText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_TT_WaterGun_0"); set => ToolTipText = value; } // [LM]발사
    override public string ToolTipText2 { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_TT_WaterGun_1"); set => ToolTipText2 = value; } // [RM]공기 충전.
    override public string ToolTipText3 { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_TT_WaterGun_2"); set => ToolTipText3 = value; } // [R]물 충전. 

    private bool isLoadWaterStateChecked = false;


    void Awake()
    {
        ToggleAnimator();
        if (string.IsNullOrEmpty(uniqueInstanceID))
        {
            uniqueInstanceID = System.Guid.NewGuid().ToString();
        }
    }
    
    void Start()
    {
        
    }
    public override void Use(Player player)
    {
        base.Use(player);
    }
    public override void HitEventInteractionF(Player player)
    {
        base.HitEventInteractionF(player);
    }

    private void Update()
    {
        AnimatorStateInfo stateInfo = itemAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(animatorItemLoadWaterName))
        {
            isLoadWaterStateChecked = true;
        }
        else if (isLoadWaterStateChecked)
        {
            // "LoadWater" 상태가 끝났을 때 실행
            AniEvtLoadWaterEnd();
            isLoadWaterStateChecked = false;
        }
    }
    public override void OnDrop()
    {
        base.OnDrop();
        UnMount();
    }
    public override void PickUp(Player player)
    {
        base.PickUp(player);
    }

    public override void Mount(Player player, object state)
    {
        base.Mount(player, state);
        if(state != null)
        {
            DeserializeState(state);
        }
        else
        {
           Debug.Log("Mount ItemWaterGun state NULL");
        }
        ToggleAnimator();
        handPivot = player.rightHandPos;
        transform.position = handPivot.position;
        // 새로운 RH 기준
        waterGunPosition = new Vector3(0.162f,-0.164000005f,0.428000003f);
        waterGunRotation = new Vector3(306.530121f,243.974197f,216.739578f);
        waterGunScale = new Vector3(0.371532321f,0.37153247f,0.371532351f);

        Rigidbody rd =  GetComponent<Rigidbody>();
        rd.isKinematic = true;
        playerArmAnimator = player.armAnimator;
        int layerIndex = playerArmAnimator.GetLayerIndex(animatorLayerName);
        playerArmAnimator.SetLayerWeight(layerIndex, 1);
        
        transform.localPosition = waterGunPosition;
        transform.localRotation = Quaternion.Euler(waterGunRotation);
        transform.localScale = waterGunScale;

        

    }
    public override void UnMount()
    {
        base.UnMount();
        Debug.Log("ItemWaterGun UnMount");
        int layerIndex = playerArmAnimator.GetLayerIndex(animatorLayerName);
        playerArmAnimator.SetLayerWeight(layerIndex, 0);
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
            DealDamage(damage, damageAbleTemp);
        }
    }

    public void DealDamage(int damage, IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Water); // 물 데미지 타입 사용
        Debug.Log($"{target.ToString()} Water Damage! _ Left HP { target.Hp}");
    }

    private void CheckValuesAndStop()
    {
        if (currentAirValue <= 0)
        {
            // 물총 발사 중지
            vfxWater.Stop();
            vfxWaterSmoke.Stop();
            
            // 추가적인 동작이 필요하다면 여기에 추가
            Debug.Log("Air is depleted. Stopping effects.");
        }

        if (currentWaterValue <= 0)
        {
            // 물총 발사 중지
            vfxWater.Stop();
            vfxWaterSmoke.Stop();
            
            // 추가적인 동작이 필요하다면 여기에 추가
            Debug.Log("Water is depleted. Stopping effects.");
        }
    }

    public void OverrideUpdate()
    {
        WaterPower();
        if(Input.GetKeyDown(KeyCode.Mouse0) && itemAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            Fire();
        }
        // 물총 발사 중
        if(Input.GetKey(KeyCode.Mouse0) && itemAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            currentAirValue -= airDecreaseValue;
            currentWaterValue -= waterDecreaseValue;
            CheckValuesAndStop(); // 값이 0이 되었는지 확인
        }
        // 물총 발사 완료
        if(Input.GetKeyUp(KeyCode.Mouse0) && itemAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            playerArmAnimator.CrossFade(animatorArmFinishName,0.1f);
            vfxWater.Stop();
            vfxWaterSmoke.Stop();
        }
        if(Input.GetKeyDown(KeyCode.Mouse1) && itemAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            itemAnimator.CrossFade(animatorItemLoadAirName,0.1f);
            playerArmAnimator.CrossFade(animatorArmLoadAirName,0.1f);
        }
        if(Input.GetKeyDown(KeyCode.R) && itemAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            itemAnimator.CrossFade(animatorItemLoadWaterName,0.1f);
            playerArmAnimator.CrossFade(animatorArmLoadWaterName,0.1f);
        }

    }

    private void Fire()
    {
        playerArmAnimator.CrossFade(animatorArmStartName,0.1f);
        vfxWater.Play();
        WaterPower();
    }

    float currentWaterGravity = 10f;
    float minWaterGravity = 2.6f;
    float maxWaterGravity = 50f;

    float currentWaterStartLifeTime = 1.5f;
    float minWaterStartLifeTime = 0f;
    float maxWaterStartLifeTime = 1.5f;


    private void WaterPower()
    {
        UpdateWaterGravity();
    }

    private void UpdateWaterGravity()
    {
        float airRatio = currentAirValue / airMaxValue;
        // Debug.Log($"airRatio: {airRatio} currentAirValue: {currentAirValue} airMaxValue: {airMaxValue}");
        currentWaterGravity = Mathf.Lerp(maxWaterGravity, minWaterGravity, airRatio);
        currentWaterStartLifeTime = Mathf.Lerp(minWaterStartLifeTime, maxWaterStartLifeTime, airRatio);
        // Debug.Log($"currentWaterGravity: {currentWaterGravity} currentWaterStartLifeTime: {currentWaterStartLifeTime}");
        var vfxWaterSmokeMain = vfxWaterSmoke.main;
        var vfxWaterMain = vfxWater.main;

        vfxWaterSmokeMain.gravityModifier = currentWaterGravity;
        vfxWaterMain.gravityModifier = currentWaterGravity;

        if(airRatio <= 0f)
        {
            vfxWaterSmoke.Stop();
            vfxWater.Stop();
        }
    }

    private void ToggleAnimator()
    {
        if(transform.parent == null)
        {
            itemAnimator.enabled = false;
        }
        else
        {
            itemAnimator.enabled = true;
        }
    }

    public void AniEvtLoadAirEnd()
    {
        Debug.Log("Do AniEvtLoadAirEnd");
        RefillAir();
    }
    
    public void AniEvtLoadWaterEnd()
    {
        Debug.Log("Do AniEvtLoadWaterEnd");
        RefillWater();
    }

    private void RefillAir()
    {
        currentAirValue = airMaxValue;
    }

    private void RefillWater()
    {
        currentWaterValue = waterMaxValue;
    }

    public void SaveWaterGunState()
    {
        ES3.Save("currentAirValue", currentAirValue);
        ES3.Save("currentWaterValue", currentWaterValue);
        Debug.Log($"WaterGunState Saved {currentAirValue} {currentWaterValue}");
    }

    public void LoadWaterGunState()
    {
        currentAirValue = ES3.Load<float>("currentAirValue", airMaxValue);
        currentWaterValue = ES3.Load<float>("currentWaterValue", waterMaxValue);
        Debug.Log($"WaterGunState Loaded {currentAirValue} {currentWaterValue}");
    }
    

    // public override void SaveState()
    // {
    //     // base.SaveState();
    //     SaveWaterGunState();
    // }

    // public override void LoadState()
    // {
    //     // base.LoadState();
    //     LoadWaterGunState();
    // }
   
    public override object SerializeState()
    {
        WaterGunState waterGunState = new WaterGunState();
        waterGunState.currentAirValue = this.currentAirValue;
        waterGunState.currentWaterValue = this.currentWaterValue;
        Debug.Log($"SerializeState {waterGunState.Print()}");
        return waterGunState;
    }
    public override void DeserializeState(object state)
    {
        if (state is WaterGunState s)
        {
            this.currentAirValue = s.currentAirValue;
            this.currentWaterValue = s.currentWaterValue;
            Debug.Log($"DeserializeState {s.Print()}");
        }
    }

   

}


public class WaterGunState
{
    public float currentAirValue;
    public float currentWaterValue;
    public string Print()
    {
        return $"WaterGunState : currentAirValue {currentAirValue} , currentWaterValue {currentWaterValue}";
    }
}
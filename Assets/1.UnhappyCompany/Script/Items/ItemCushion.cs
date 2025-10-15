using UnityEngine;
using System.Collections;

/// <summary>
/// Collider 상태 Enum
/// </summary>
public enum ColliderState
{
    Item,       // 아이템 상태 (작은 Collider, 트리거 꺼짐)
    Deployed    // 펴진 상태 (큰 Collider, 트리거 켜짐)
}

public class ItemCushion : Item
{
    [Header("Cushion Models")]
    [SerializeField] private GameObject boxModel;        // 설치 전 Box 오브젝트
    [SerializeField] private GameObject cushionModel;    // 설치 후 Cushion 오브젝트
    
    [Header("Colliders")]
    [SerializeField] private Collider itemCollider;      // 아이템 상태 Collider (작고, 트리거 꺼짐)
    [SerializeField] private Collider deployedCollider;  // 펴진 상태 Collider (크고, 트리거 켜짐)
    
    [Header("Animation Settings")]
    [SerializeField] private float deployStartDelay = 0.3f;  // 설치 선딜 시간 (쿠션이 펴지기 전 준비)
    [SerializeField] private float deployDuration = 0.5f;    // 설치 애니메이션 시간 (쿠션이 펴지는 시간)
    [SerializeField] private AnimationCurve deployCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 설치 애니메이션 커브
    
    [Header("Impact Settings (Phase 3)")]
    [SerializeField] private float impactSquashAmount = 0.7f;  // 찌그러짐 정도 (0.7 = 30% 찌그러짐)
    [SerializeField] private float impactDuration = 0.3f;      // 찌그러짐 애니메이션 시간
    [SerializeField] private AnimationCurve impactCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 찌그러짐 커브
    [SerializeField] private ParticleSystem impactParticle;    // 충격 파티클 (선택)
    
    [Header("State")]
    private bool isDeployed = false;    // 설치 완료 상태
    private Vector3 originalCushionScale;  // 쿠션 원본 크기
    private bool isImpacting = false;   // 충격 애니메이션 중 (Phase 3)
    
    void Start()
    {
        InitializeCushion();
    }

    /// <summary>
    /// 쿠션 초기 상태 설정
    /// </summary>
    private void InitializeCushion()
    {
        // 자식 오브젝트에서 자동으로 찾기 (Inspector에서 설정 안했을 경우)
        if (boxModel == null)
            boxModel = transform.Find("BoxModel")?.gameObject;
        
        if (cushionModel == null)
            cushionModel = transform.Find("CushionModel")?.gameObject;
        
        if (cushionModel != null)
        {
            originalCushionScale = cushionModel.transform.localScale;
            
            // 초기 상태: Box 활성, Cushion 비활성
            if (!isDeployed)
            {
                if (boxModel != null)
                    boxModel.SetActive(true);
                
                cushionModel.SetActive(true);
                cushionModel.transform.localScale = Vector3.zero;
            }
        }
        else
        {
            Debug.LogWarning($"[ItemCushion] CushionModel이 설정되지 않았습니다: {gameObject.name}");
        }
        
        // Collider 자동 탐색 (Inspector에서 설정 안했을 경우)
        if (itemCollider == null || deployedCollider == null)
        {
            AutoDetectColliders();
        }
    }
    
    /// <summary>
    /// Collider 자동 탐색 (Inspector에서 설정하지 않았을 때)
    /// </summary>
    private void AutoDetectColliders()
    {
        Collider[] colliders = GetComponents<Collider>();
        
        if (colliders.Length >= 2)
        {
            // 크기가 작은 것을 itemCollider, 큰 것을 deployedCollider로 추정
            if (itemCollider == null)
                itemCollider = colliders[0];
            if (deployedCollider == null)
                deployedCollider = colliders[1];
                
            Debug.Log($"[ItemCushion] Collider 자동 탐색 완료: Item({itemCollider.GetType().Name}), Deployed({deployedCollider.GetType().Name})");
        }
        else if (colliders.Length == 1)
        {
            Debug.LogWarning($"[ItemCushion] Collider가 1개만 있습니다. 2개가 필요합니다!");
        }
        else
        {
            Debug.LogError($"[ItemCushion] Collider가 없습니다! Root에 2개의 Collider를 추가해주세요.");
        }
    }

    public override void Use(Player player)
    {
        // BuildSystem에 설치 시작 (벽에만 설치 가능, 콜백으로 프리뷰 초기화)
        player.buildSystem.StartPlacing(
            itemData.prefab.gameObject, 
            this.gameObject, 
            true, 
            OnPlacingStarted
        );
    }
    
    /// <summary>
    /// BuildSystem에서 프리뷰 생성 시 호출되는 콜백
    /// </summary>
    private void OnPlacingStarted(GameObject previewObject)
    {
        var cushion = previewObject.GetComponent<ItemCushion>();
        if (cushion != null)
        {
            // 프리뷰 상태에서는 모든 Collider 비활성화 (Raycast 방해 방지)
            if (cushion.itemCollider != null)
                cushion.itemCollider.enabled = false;
            
            if (cushion.deployedCollider != null)
                cushion.deployedCollider.enabled = false;
            
            Debug.Log($"[ItemCushion] 프리뷰 생성 완료: {previewObject.name}");
        }
    }

    /// <summary>
    /// BuildSystem에서 설치 확정 시 호출 (Phase 1)
    /// </summary>
    public void OnPlaced()
    {
        if (isDeployed) return;
        
        // 쿠션 모델 초기화 (BuildSystem으로 생성된 경우 Start()가 안 불릴 수 있음)
        EnsureInitialized();
        
        // Rigidbody만 먼저 설정 (Collider는 애니메이션 중에 전환)
        SetupRigidbody();
        
        StartCoroutine(DeployAnimation());
    }
    
    /// <summary>
    /// 초기화 보장 (Start()가 호출되지 않았을 경우 대비)
    /// </summary>
    private void EnsureInitialized()
    {
        // originalCushionScale이 초기화되지 않았다면 초기화
        if (originalCushionScale == Vector3.zero)
        {
            if (cushionModel != null)
            {
                originalCushionScale = cushionModel.transform.localScale;
                Debug.Log($"[ItemCushion] EnsureInitialized - originalCushionScale: {originalCushionScale}");
            }
        }
    }
    
    /// <summary>
    /// Rigidbody 설정 (Kinematic으로 고정)
    /// </summary>
    private void SetupRigidbody()
    {
        // Root Tag 확인
        if (gameObject.tag != ETag.Item.ToString())
        {
            gameObject.tag = ETag.Item.ToString();
            Debug.LogWarning($"[ItemCushion] Tag를 Item으로 수정했습니다.");
        }
        
        // Rigidbody를 Kinematic으로 설정
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log($"[ItemCushion] Rigidbody를 Kinematic으로 설정");
        }
        else
        {
            Debug.LogError($"[ItemCushion] Rigidbody가 없습니다! Prefab에 추가해주세요.");
        }
    }

    /// <summary>
    /// Collider 상태 전환 (아이템 ↔ 펴진 상태)
    /// </summary>
    /// <param name="state">전환할 Collider 상태</param>
    private void SetColliderState(ColliderState state)
    {
        if (itemCollider == null || deployedCollider == null)
        {
            Debug.LogError($"[ItemCushion] Collider가 설정되지 않았습니다!");
            return;
        }
        
        switch (state)
        {
            case ColliderState.Item:
                // 아이템 상태: 작은 Collider 활성화, 트리거 꺼짐
                itemCollider.enabled = true;
                itemCollider.isTrigger = false;
                
                deployedCollider.enabled = false;
                
                Debug.Log($"[ItemCushion] Collider 상태 → 아이템 (작은 크기, 트리거 꺼짐)");
                break;
                
            case ColliderState.Deployed:
                // 펴진 상태: 큰 Collider 활성화, 트리거 켜짐
                itemCollider.enabled = false;
                
                deployedCollider.enabled = true;
                deployedCollider.isTrigger = true;
                
                Debug.Log($"[ItemCushion] Collider 상태 → 펴진 상태 (큰 크기, 트리거 켜짐)");
                break;
        }
    }
    

    /// <summary>
    /// 설치 애니메이션 (Box → Cushion) - 3단계 사운드 포함
    /// </summary>
    private IEnumerator DeployAnimation()
    {
        isDeployed = true;
        
        // ============================
        // 1단계: 설치 시작 (선딜)
        // ============================
        
        // 설치 시작 사운드 재생 (즉시)
        PlayDeployStartSound();
        
        // Cushion 모델 준비 (Scale 0 상태, Box는 아직 보임)
        if (cushionModel != null)
        {
            cushionModel.SetActive(true);
            cushionModel.transform.localScale = Vector3.zero;
        }
        
        // 선딜 대기 (Box가 보이는 상태로 대기)
        yield return new WaitForSeconds(deployStartDelay);
        
        // ============================
        // 2단계: 쿠션 펼치기 (애니메이션)
        // ============================
        
        // Box 모델 비활성화 (이제 쿠션이 나옴)
        if (boxModel != null)
        {
            boxModel.SetActive(false);
        }
        
        // 쿠션이 펴지기 시작할 때 Collider를 Deployed 상태로 전환
        SetColliderState(ColliderState.Deployed);
        
        // 펼치는 사운드 재생
        PlayDeployUnfoldSound();
        
        float elapsed = 0f;
        
        // Cushion 모델 Scale 애니메이션 (0 → 1)
        if (cushionModel != null)
        {
            while (elapsed < deployDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / deployDuration);
                float curveValue = deployCurve.Evaluate(t);
                
                cushionModel.transform.localScale = originalCushionScale * curveValue;
                
                yield return null;
            }
            
            // 최종 크기 보정
            cushionModel.transform.localScale = originalCushionScale;
        }
        
        // ============================
        // 3단계: 설치 완료
        // ============================
        
        // 완료 사운드 재생
        PlayDeployCompleteSound();
        
        Debug.Log("[ItemCushion] 설치 완료!");
    }

    /// <summary>
    /// 1단계: 설치 시작 사운드 (설치 확정 시)
    /// </summary>
    private void PlayDeployStartSound()
    {
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.cushionDeployStart, 
                transform, 
                "쿠션 설치 시작 - 설치 확정 소리 (예: 딸깍)"
            );
        }
    }

    /// <summary>
    /// 2단계: 쿠션 펼치는 사운드 (애니메이션 진행 중)
    /// </summary>
    private void PlayDeployUnfoldSound()
    {
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.cushionDeployUnfold, 
                transform, 
                "쿠션 펼치는 중 - 공기 빠지는 소리/천 펼치는 소리"
            );
        }
    }

    /// <summary>
    /// 3단계: 설치 완료 사운드 (펴짐 완료)
    /// </summary>
    private void PlayDeployCompleteSound()
    {
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.cushionDeployComplete, 
                transform, 
                "쿠션 설치 완료 - 펴짐 완료 소리 (예: 퐁!)"
            );
        }
    }

    // ========================
    // Phase 2: 회수 시스템
    // ========================
    
    /// <summary>
    /// 설치된 쿠션과 상호작용하여 회수 (F키)
    /// </summary>
    public override void HitEventInteractionF(Player player)
    {
        // 펴진 상태에서는 접기만 가능 (줍기 불가)
        if (isDeployed)
        {
            Debug.Log("[ItemCushion] 펴진 상태 - 접기 시작 (줍기 불가)");
            StartCoroutine(RetractAnimation(player));
            return;
        }
        
        // 아이템 상태에서는 일반 줍기
        Debug.Log("[ItemCushion] 아이템 상태 - 일반 줍기");
        base.HitEventInteractionF(player);
    }
    
    /// <summary>
    /// 접기 애니메이션 (Cushion → Box 역재생)
    /// 아이템 상태로 되돌리고 바닥에 떨어뜨림 (인벤토리 추가 X)
    /// </summary>
    private IEnumerator RetractAnimation(Player player)
    {
        Debug.Log("[ItemCushion] 접기 시작!");
        
        // 1. 접기 사운드 재생 (시작)
        PlayRetractSound();
        
        // 2. Cushion 모델 Scale 애니메이션 (1 → 0) - 역재생
        float elapsed = 0f;
        while (elapsed < deployDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / deployDuration);
            float curveValue = deployCurve.Evaluate(t);
            
            if (cushionModel != null)
                cushionModel.transform.localScale = originalCushionScale * curveValue;
            
            yield return null;
        }
        
        // 3. Cushion 비활성화, Box 활성화
        if (cushionModel != null)
        {
            cushionModel.SetActive(false);
            cushionModel.transform.localScale = Vector3.zero;
        }
        
        if (boxModel != null)
            boxModel.SetActive(true);
        
        // 4. 상태를 아이템으로 복귀
        isDeployed = false;
        
        // 5. Collider를 아이템 상태로 복귀 (작은 Collider)
        SetColliderState(ColliderState.Item);
        
        // 6. Rigidbody를 Dynamic으로 복귀 (떨어질 수 있게)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        
        Debug.Log("[ItemCushion] 접기 완료! 이제 바닥에 떨어지며 다시 F키로 습득 가능");
    }
    
    /// <summary>
    /// 회수 사운드 재생
    /// </summary>
    private void PlayRetractSound()
    {
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.cushionRetract, 
                transform, 
                "쿠션 회수 사운드 - 접히는 소리"
            );
        }
    }

    // ========================
    // Phase 3: 충격 흡수 연출
    // ========================
    
    /// <summary>
    /// Rampage 충돌 시 호출되는 충격 이벤트
    /// </summary>
    /// <param name="impactPosition">충돌 위치</param>
    public void OnImpact(Vector3 impactPosition)
    {
        if (!isDeployed)
        {
            Debug.LogWarning("[ItemCushion] 펴지지 않은 상태에서 충격 받음");
            return;
        }
        
        if (isImpacting)
        {
            Debug.Log("[ItemCushion] 이미 충격 애니메이션 중");
            return;
        }
        
        // 초기화 보장 (안전장치)
        EnsureInitialized();
        
        Debug.Log($"[ItemCushion] 충격 받음! 위치: {impactPosition}");
        
        // 충격 흡수 연출 시작
        StartCoroutine(ImpactAnimation());
        
        // 충격 사운드 재생
        PlayImpactSound();
        
        // 파티클 재생 (있다면)
        PlayImpactParticle(impactPosition);
    }
    
    /// <summary>
    /// 충격 흡수 애니메이션 (찌그러짐)
    /// </summary>
    private IEnumerator ImpactAnimation()
    {
        isImpacting = true;
        float elapsed = 0f;
        
        // 1단계: 찌그러짐 (0 → 1)
        while (elapsed < impactDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (impactDuration / 2f));
            float curveValue = impactCurve.Evaluate(t);
            
            // Z축 방향으로 찌그러짐 (충격 방향)
            float squash = Mathf.Lerp(1f, impactSquashAmount, curveValue);
            Vector3 squashedScale = new Vector3(
                originalCushionScale.x,
                originalCushionScale.y,
                originalCushionScale.z * squash
            );
            
            if (cushionModel != null)
                cushionModel.transform.localScale = squashedScale;
            
            yield return null;
        }
        
        elapsed = 0f;
        
        // 2단계: 복원 (1 → 0)
        while (elapsed < impactDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (impactDuration / 2f));
            float curveValue = impactCurve.Evaluate(t);
            
            // 원래 크기로 복원
            float squash = Mathf.Lerp(impactSquashAmount, 1f, curveValue);
            Vector3 squashedScale = new Vector3(
                originalCushionScale.x,
                originalCushionScale.y,
                originalCushionScale.z * squash
            );
            
            if (cushionModel != null)
                cushionModel.transform.localScale = squashedScale;
            
            yield return null;
        }
        
        // 최종 크기 보정
        if (cushionModel != null)
            cushionModel.transform.localScale = originalCushionScale;
        
        isImpacting = false;
        Debug.Log("[ItemCushion] 충격 흡수 완료!");
    }
    
    /// <summary>
    /// 충격 사운드 재생
    /// </summary>
    private void PlayImpactSound()
    {
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.cushionImpact, 
                transform, 
                "쿠션 충격 흡수 사운드 - 둔탁한 소리"
            );
        }
    }
    
    /// <summary>
    /// 충격 파티클 재생 (Phase 3 - 나중에 Unity에서 설정)
    /// </summary>
    private void PlayImpactParticle(Vector3 impactPosition)
    {
        // if (impactParticle != null)
        // {
        //     // 파티클 위치를 충격 위치로 설정
        //     impactParticle.transform.position = impactPosition;
        //     impactParticle.Play();
        //     Debug.Log($"[ItemCushion] 충격 파티클 재생: {impactPosition}");
        // }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 디버그: 강제 설치 테스트
    /// </summary>
    [ContextMenu("Test Deploy Animation")]
    private void TestDeploy()
    {
        if (Application.isPlaying)
        {
            OnPlaced();
        }
    }
    
    /// <summary>
    /// 디버그: 충격 흡수 테스트
    /// </summary>
    [ContextMenu("Test Impact Animation")]
    private void TestImpact()
    {
        if (Application.isPlaying)
        {
            // 테스트를 위해 강제로 설치된 상태로 설정
            if (!isDeployed)
            {
                Debug.Log("[ItemCushion] 테스트를 위해 강제로 설치 상태로 전환");
                isDeployed = true;
                
                // 쿠션 모델 활성화
                if (cushionModel != null)
                {
                    cushionModel.SetActive(true);
                    cushionModel.transform.localScale = originalCushionScale;
                }
                if (boxModel != null)
                {
                    boxModel.SetActive(false);
                }
            }
            
            // 충격 테스트
            OnImpact(transform.position);
        }
        else
        {
            Debug.LogWarning("[ItemCushion] Play Mode에서만 테스트 가능합니다.");
        }
    }
    
    /// <summary>
    /// 디버그: 즉시 찌그러짐만 테스트 (상태 체크 무시)
    /// </summary>
    [ContextMenu("Test Impact (Force)")]
    private void TestImpactForce()
    {
        if (Application.isPlaying)
        {
            Debug.Log("[ItemCushion] 강제 충격 테스트 - 상태 체크 무시");
            StartCoroutine(ImpactAnimation());
            PlayImpactSound();
        }
        else
        {
            Debug.LogWarning("[ItemCushion] Play Mode에서만 테스트 가능합니다.");
        }
    }
#endif
}

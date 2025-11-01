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
    
    [Header("Impact Settings")]
    [SerializeField] private float impactSquashAmount = 0.7f;  // 찌그러짐 정도 (0.7 = 30% 찌그러짐)
    [SerializeField] private float impactDuration = 0.3f;      // 찌그러짐 애니메이션 시간
    [SerializeField] private AnimationCurve impactCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 찌그러짐 커브
    [SerializeField] private ParticleSystem impactParticle;    // 충격 파티클 (선택)
    
    [Header("State")]
    private bool isDeployed = false;    // 설치 완료 상태
    private Vector3 originalCushionScale;  // 쿠션 원본 크기
    private bool isImpacting = false;   // 충격 애니메이션 중
    private Vector3 lastImpactDirection; // 마지막 충격 방향 (로컬 공간)
    
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
    /// BuildSystem에서 설치 확정 시 호출
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
    /// Rigidbody 설정 (Kinematic으로 고정) 및 Layer 변경
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
        
        // Layer를 DeployedItem으로 변경 (설치된 아이템 전용 레이어)
        // - 플레이어와 충돌 가능
        // - InteractionSystem에서 감지 가능 (F키)
        // - 모든 설치형 아이템이 사용
        int deployedItemLayer = LayerMask.NameToLayer("DeployedItem");
        gameObject.layer = deployedItemLayer;
        Debug.Log($"[ItemCushion] Layer를 DeployedItem으로 변경 (플레이어 충돌 가능, F키 상호작용 가능)");
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
            AudioManager.instance.Play3DSoundByTransform(
                FMODEvents.instance.cushionDeployStart, 
                transform, 
                40f,
                "Cushion Deploy Start"
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
            AudioManager.instance.Play3DSoundByTransform(
                FMODEvents.instance.cushionDeployUnfold, 
                transform, 
                40f,
                "Cushion Deploy Unfold"
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
            AudioManager.instance.Play3DSoundByTransform(
                FMODEvents.instance.cushionDeployComplete, 
                transform, 
                40f,
                "Cushion Deploy Complete"
            );
        }
    }

    // ========================
    // 회수 시스템
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
        
        // 7. Layer를 Item으로 복원 (일반 아이템 상태)
        int itemLayer = LayerMask.NameToLayer(ETag.Item.ToString());
        gameObject.layer = itemLayer;
        Debug.Log($"[ItemCushion] Layer를 Item으로 복원");
        
        Debug.Log("[ItemCushion] 접기 완료! 이제 바닥에 떨어지며 다시 F키로 습득 가능");
    }
    
    /// <summary>
    /// 회수 사운드 재생
    /// </summary>
    private void PlayRetractSound()
    {
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            AudioManager.instance.Play3DSoundByTransform(
                FMODEvents.instance.cushionRetract, 
                transform, 
                40f,
                "Cushion Retract"
            );
        }
    }

    // ========================
    // 충격 흡수 연출
    // ========================
    
    /// <summary>
    /// Rampage 충돌 시 호출되는 충격 이벤트 (충돌 지점 포함)
    /// </summary>
    /// <param name="attackerPosition">공격자의 위치</param>
    /// <param name="contactPoint">충돌 지점</param>
    public void OnImpactWithContact(Vector3 attackerPosition, Vector3 contactPoint)
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
        
        // 충돌 지점에서 쿠션 중심으로의 방향 (쿠션 안쪽으로 들어가는 방향)
        Vector3 impactDirectionWorld = (transform.position - contactPoint).normalized;
        
        // 월드 방향을 로컬 방향으로 변환
        Vector3 localDirection = transform.InverseTransformDirection(impactDirectionWorld);
        
        // 각 축별 영향도 계산 (절댓값)
        lastImpactDirection = new Vector3(
            Mathf.Abs(localDirection.x),
            Mathf.Abs(localDirection.y),
            Mathf.Abs(localDirection.z)
        );
        
        Debug.Log($"[ItemCushion] 충격 받음!\n" +
                  $"  공격자: {attackerPosition}\n" +
                  $"  충돌 지점: {contactPoint}\n" +
                  $"  충격 방향(월드): {impactDirectionWorld}\n" +
                  $"  충격 방향(로컬): {localDirection}\n" +
                  $"  영향도(abs): {lastImpactDirection}");
        
        // 충격 흡수 연출 시작
        StartCoroutine(ImpactAnimation());
        
        // 충격 사운드 재생
        PlayImpactSound();
        
        // 파티클 재생 (충돌 지점에)
        PlayImpactParticle(contactPoint);
    }
    
    /// <summary>
    /// (Deprecated) 이전 버전 - OnImpactWithContact 사용 권장
    /// </summary>
    public void OnImpact(Vector3 attackerPosition)
    {
        // 충돌 지점을 알 수 없으면 중심으로 가정
        OnImpactWithContact(attackerPosition, transform.position);
    }
    
    /// <summary>
    /// 충격 흡수 애니메이션 (찌그러짐 - 방향성 있음)
    /// </summary>
    private IEnumerator ImpactAnimation()
    {
        isImpacting = true;
        float elapsed = 0f;
        
        // 충돌 방향의 주 축 찾기 (X, Y, Z 중 가장 큰 값)
        Vector3 absDirection = new Vector3(
            Mathf.Abs(lastImpactDirection.x),
            Mathf.Abs(lastImpactDirection.y),
            Mathf.Abs(lastImpactDirection.z)
        );
        
        // 1단계: 찌그러짐 (0 → 1)
        while (elapsed < impactDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (impactDuration / 2f));
            float curveValue = impactCurve.Evaluate(t);
            
            // 충돌 방향으로 찌그러짐, 다른 축은 약간 확대 (부피 보존)
            float squashFactor = Mathf.Lerp(1f, impactSquashAmount, curveValue);
            float stretchFactor = Mathf.Lerp(1f, 1f + (1f - impactSquashAmount) * 0.5f, curveValue);
            
            Vector3 squashedScale = CalculateDirectionalSquash(squashFactor, stretchFactor, absDirection);
            
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
            float squashFactor = Mathf.Lerp(impactSquashAmount, 1f, curveValue);
            float stretchFactor = Mathf.Lerp(1f + (1f - impactSquashAmount) * 0.5f, 1f, curveValue);
            
            Vector3 squashedScale = CalculateDirectionalSquash(squashFactor, stretchFactor, absDirection);
            
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
    /// 충돌 방향에 따른 찌그러진 Scale 계산
    /// </summary>
    /// <param name="squashFactor">찌그러짐 비율 (0.7 = 30% 감소)</param>
    /// <param name="stretchFactor">늘어남 비율 (부피 보존용)</param>
    /// <param name="absDirection">충돌 방향 (절댓값)</param>
    /// <returns>찌그러진 Scale</returns>
    private Vector3 CalculateDirectionalSquash(float squashFactor, float stretchFactor, Vector3 absDirection)
    {
        // 각 축별 가중치 계산 (충돌 방향이 강할수록 더 찌그러짐)
        float xWeight = absDirection.x;
        float zWeight = absDirection.z;
        // Y축은 항상 고정 (위아래 찌그러짐은 부자연스러움)
        
        // X, Z축만 찌그러짐 계산
        float xScale = Mathf.Lerp(stretchFactor, squashFactor, xWeight);
        float zScale = Mathf.Lerp(stretchFactor, squashFactor, zWeight);
        
        return new Vector3(
            originalCushionScale.x * xScale,
            originalCushionScale.y,  // Y축은 항상 원본 크기 유지
            originalCushionScale.z * zScale
        );
    }
    
    /// <summary>
    /// 충격 사운드 재생
    /// </summary>
    private void PlayImpactSound()
    {
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            AudioManager.instance.Play3DSoundByTransform(
                FMODEvents.instance.cushionImpact, 
                transform, 
                40f,
                "Cushion Impact"
            );
        }
    }
    
    /// <summary>
    /// 충격 파티클 재생
    /// </summary>
    private void PlayImpactParticle(Vector3 impactPosition)
    {
        if (impactParticle != null)
        {
            // 파티클 위치를 충격 위치로 설정
            impactParticle.transform.position = impactPosition;
            impactParticle.Play();
            Debug.Log($"[ItemCushion] 충격 파티클 재생: {impactPosition}");
        }
    }
    
    // ========================
    // 플레이어 충돌
    // ========================
    
    /// <summary>
    /// 플레이어와 충돌 감지 (달리기 상태에서만 충격 연출)
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // 펴진 상태가 아니면 무시
        if (!isDeployed) return;
        
        // 플레이어와 충돌했는지 확인
        if (other.CompareTag(ETag.Player.ToString()))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.playerStatus != null)
            {
                // 달리기 중일 때만 충격 연출
                if (player.playerStatus.IsCurrentRun)
                {
                    Debug.Log("[ItemCushion] 플레이어가 달려서 충돌 - 충격 연출 시작");
                    
                    // 플레이어와 쿠션의 가장 가까운 지점을 충돌 지점으로 사용
                    Collider cushionCollider = GetComponent<Collider>();
                    Vector3 contactPoint = cushionCollider != null 
                        ? cushionCollider.ClosestPoint(other.transform.position)
                        : transform.position;
                    
                    OnImpactWithContact(other.transform.position, contactPoint);
                }
                else
                {
                    Debug.Log("[ItemCushion] 플레이어가 걸어서 충돌 - 충격 연출 없음");
                }
            }
        }
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

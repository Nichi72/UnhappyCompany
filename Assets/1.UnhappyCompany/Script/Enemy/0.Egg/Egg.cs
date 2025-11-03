using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



public enum EggStage
{
    Stage1,     // 초기 단계 (무적)
    Stage2,     // 중간 단계 (데미지 받음)
    Stage3      // 최종 단계 (부화)
}

public class Egg : MonoBehaviour, IDamageable, IScannable
{
    [Header("DEBUG")]
    public string currentTimeText;
    [Tooltip("생성 시간")]
    public string createTimeText;
    [Tooltip("Stage1 종료 시간")]
    public string stage1EndTimeText;
    [Tooltip("Stage2 종료 시간")]
    public string stage2EndTimeText;
    [Tooltip("Stage1 지속 시간(초)")]
    public string stage1DurationText;
    [Tooltip("Stage2 지속 시간(초)")]
    public string stage2DurationText;
    [Tooltip("남은 시간")]
    public string remainingTimeText;
    [Tooltip("알 생성 실제 시간")]
    public float createdRealTime;
    [Tooltip("현실 기준 경과 시간(분)")]
    public float realElapsedMinutes;

    [Header("Basic Settings")]
    [Tooltip("알이 부화할 적 데이터")] 
    public BaseEnemyAIData enemyAIData;
    [Tooltip("알이 부서졌을 때 생성할 파편 프리팹")] 
    public GameObject eggShatterItemPrefab;
    
    [Header("Egg Type Settings")]
    [Tooltip("알의 타입 (기계형/인간형/동물형)")] 
    public EnemyType eggType; // 현재는 안씀
    
    [Header("Stage Settings (현실 시간)")]
    [Tooltip("현실 시간으로 Stage 1의 기본 지속 시간(분)")]
    public float stage1RealTimeMinutes = 10f; // 현실 10분 = 게임 내 1일
    [Tooltip("현실 시간으로 Stage 1의 지속 시간 랜덤 범위(분)")]
    public float stage1RandomRangeMinutes = 2f; // 현실 2분
    [Tooltip("현실 시간으로 Stage 2의 기본 지속 시간(분)")]
    public float stage2RealTimeMinutes = 5.8f; // 현실 5.8분 = 게임 내 약 350초
    [Tooltip("현실 시간으로 Stage 2의 지속 시간 랜덤 범위(분)")]
    public float stage2RandomRangeMinutes = 2.5f; // 현실 2.5분 = 게임 내 약 150초
    
    [Header("Stage Timing (게임 내 시간 - 수정 금지)")]
    [ReadOnly] public float stage1Duration; // 게임 내 시간(초)
    [ReadOnly] public float stage2Duration; // 게임 내 시간(초)

    [Header("Stage Visuals")]
    [Tooltip("Stage 1의 외형")] 
    public GameObject stage1Visual;
    [Tooltip("Stage 2의 외형")] 
    public GameObject stage2Visual;
    
    [Tooltip("단계 변화시 재생할 파티클 이펙트")]
    public ParticleSystem stageTransitionEffect;
    
    [ReadOnly] [SerializeField] private EggStage currentStage = EggStage.Stage1;
    public int hp { get; set; } = 100;
    public int id;
    public bool isScanning = false;
    public bool isScanningOver = false;
    
    [HideInInspector] public bool isLoadedFromSave = false; // 로드된 알인지 여부

    /// <summary>
    /// 현재 알의 스테이지 반환 (세이브 시스템용)
    /// </summary>
    public EggStage GetCurrentStage() => currentStage;
    
    /// <summary>
    /// 알의 스테이지를 설정 (로드 시스템용)
    /// </summary>
    public void SetCurrentStage(EggStage stage)
    {
        currentStage = stage;
        UpdateVisuals(stage);
    }

    // 게임 시간 기반 타이머
    private TimeSpan createTime; // 생성 시간
    private TimeSpan stage1EndTime; // Stage1 종료 시간
    private TimeSpan stage2EndTime; // Stage2 종료 시간
    [SerializeField] private float stageElapsedTime; // 현재 단계에서 경과한 시간(초)


    private void Start()
    {
        // 로드된 알인 경우 초기화를 건너뜀
        if (isLoadedFromSave)
        {
            Debug.Log($"Egg {id} 로드 완료. 현재 단계: {currentStage}, 부화 진행도: {realElapsedMinutes:F2}분");
            return;
        }
        
        // 새로 생성된 알인 경우에만 초기화
        id = EnemyManager.instance.EggID;
        EnemyManager.instance.EggID++;
        
        // 알이 생성된 실제 시간 저장 (초 단위)
        createdRealTime = Time.time;
        
        // 먼저 stage1Duration을 설정
        InitializeVisuals();
        
        // 그 다음 타이머 초기화 (stage1Duration 사용)
        InitializeEggTimer();
        
        Debug.Log($"Egg {id} 생성 완료. 현재 단계: {currentStage}, 종료 시간: {stage1EndTime}, 현실 생성 시간: {createdRealTime}초");
    }

    private void Update()
    {
        // 현실 시간 기준으로 경과 시간 계산 (분 단위)
        realElapsedMinutes = (Time.time - createdRealTime) / 60f;
        
        CheckStageProgressionBasedOnRealTime();
        UpdateDebugTexts();
    }

    private void InitializeVisuals()
    {
        // 게임 내 하루(24시간)에 해당하는 초 값 설정
        float baseStage1Duration = TimeManager.instance.ConvertRealMinutesToGameSeconds(stage1RealTimeMinutes);
        float stage1RandomRange = TimeManager.instance.ConvertRealMinutesToGameSeconds(
            UnityEngine.Random.Range(0, stage1RandomRangeMinutes)); // 항상 양수 범위만 사용
        stage1Duration = baseStage1Duration + stage1RandomRange; // 항상 기본값보다 크거나 같게 설정
        
        // Stage 2 지속 시간에 랜덤 범위 적용 (양수 범위만 사용)
        float baseGameDuration = TimeManager.instance.ConvertRealMinutesToGameSeconds(stage2RealTimeMinutes);
        float randomRange = TimeManager.instance.ConvertRealMinutesToGameSeconds(
            UnityEngine.Random.Range(0, stage2RandomRangeMinutes)); // 항상 양수 범위만 사용
        stage2Duration = baseGameDuration + randomRange;
        
        // 모든 비주얼을 비활성화
        stage1Visual.SetActive(false);
        stage2Visual.SetActive(false);
        // Stage 1 비주얼만 활성화
        UpdateVisuals(EggStage.Stage1);
        Debug.Log($"Egg entered Stage 1 - Invincible (Stage1 지속 시간: {stage1Duration}초)");
    }

    private void InitializeEggTimer()
    {
        // 현재 게임 시간을 생성 시간으로 저장
        createTime = TimeManager.instance.GameTime;
        
        // Stage1 종료 시간 설정 (현재 시간 + Stage1 지속 시간)
        stage1EndTime = createTime.Add(TimeSpan.FromSeconds(stage1Duration));
        
        // 하루가 넘어가는 경우 계산
        TimeSpan oneDay = TimeSpan.FromHours(24);
        if (stage1EndTime >= oneDay)
        {
            stage1EndTime = stage1EndTime.Subtract(oneDay);
        }
        
        stageElapsedTime = 0f;
        
        Debug.Log($"알 {id} 타이머 초기화: 생성 시간 = {createTime}, Stage1 종료 시간 = {stage1EndTime}, Stage1 지속 시간 = {stage1Duration}초");
    }

    private void UpdateDebugTexts()
    {
        // 현재 게임 시간
        TimeSpan currentGameTime = TimeManager.instance.GameTime;
        
        // 시간 텍스트 업데이트
        currentTimeText = $"현재 시간: {currentGameTime:hh\\:mm\\:ss} (Stage: {currentStage})";
        createTimeText = $"생성: {createTime:hh\\:mm\\:ss} (현실 시간: {createdRealTime:F1}초)";
        stage1EndTimeText = $"Stage1 종료: {stage1EndTime:hh\\:mm\\:ss}";
        stage2EndTimeText = $"Stage2 종료: {stage2EndTime:hh\\:mm\\:ss}";
        stage1DurationText = $"Stage1 지속: {stage1Duration:F1}초 (현실 {stage1Duration * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds):F2}분)";
        stage2DurationText = $"Stage2 지속: {stage2Duration:F1}초 (현실 {stage2Duration * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds):F2}분)";
        
        // 남은 시간 계산
        string remainingTime = "";
        switch (currentStage)
        {
            case EggStage.Stage1:
                // Stage 1 지속 시간을 현실 시간(분)으로 변환
                float stage1RealTimeMinutesToEnd = stage1Duration * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
                float remainingRealMinutes = stage1RealTimeMinutesToEnd - realElapsedMinutes;
                
                remainingTime = $"Stage1 남은 시간: {remainingRealMinutes:F2}분 (경과 {realElapsedMinutes:F2}분 / 목표 {stage1RealTimeMinutesToEnd:F2}분)";
                break;
                
            case EggStage.Stage2:
                // Stage 1+2 지속 시간을 현실 시간(분)으로 변환
                float stage1And2RealTimeMinutesToEnd = (stage1Duration + stage2Duration) * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
                float stage2RealTimeMinutesToEnd = stage2Duration * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
                float stage2RemainingRealMinutes = stage1And2RealTimeMinutesToEnd - realElapsedMinutes;
                
                remainingTime = $"Stage2 남은 시간: {stage2RemainingRealMinutes:F2}분 (경과 {realElapsedMinutes:F2}분 / 목표 {stage1And2RealTimeMinutesToEnd:F2}분)";
                break;
                
            case EggStage.Stage3:
                remainingTime = "부화 중";
                break;
        }
        remainingTimeText = remainingTime;
    }

    private void CheckStageProgressionBasedOnRealTime()
    {
        // 현재 게임 시간 (디버그용)
        TimeSpan currentGameTime = TimeManager.instance.GameTime;
        
        // 현재 스테이지에 따라 다음 단계로 전환할 시간인지 확인
        switch (currentStage)
        {
            case EggStage.Stage1:
                // Stage 1 지속 시간을 현실 시간(분)으로 변환
                float stage1RealTimeMinutesToEnd = stage1Duration * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
                
                // 현실 경과 시간이 목표 시간을 넘었는지 확인
                if (realElapsedMinutes >= stage1RealTimeMinutesToEnd)
                {
                    // 스테이지 2로 전환
                    currentStage = EggStage.Stage2;
                    UpdateVisuals(EggStage.Stage2);
                    Debug.Log($"Egg entered Stage 2 - Vulnerable (현실 경과 시간: {realElapsedMinutes:F2}분, 게임 시간: {currentGameTime})");
                    
                    // EggLevel2Breaking: Stage2로 전환될 때 (깨질 수 있는 상태로 변경) 사운드
                    // AudioManager.instance.PlayOneShot(FMODEvents.instance.EggLevel2Breaking, transform);
                    
                    // Stage2 종료 시간 설정 (종료 시간은 게임 내 시간이 아닌 현실 경과 시간 기준으로 설정)
                    float stage2RealTimeToEnd = stage2Duration * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
                    stage2EndTime = currentGameTime.Add(TimeSpan.FromSeconds(stage2Duration));
                    
                    // 하루가 넘어가는 경우 계산
                    TimeSpan oneDay = TimeSpan.FromHours(24);
                    if (stage2EndTime >= oneDay)
                    {
                        stage2EndTime = stage2EndTime.Subtract(oneDay);
                    }
                    
                    // 디버그 정보 즉시 업데이트
                    UpdateDebugTexts();
                }
                break;
                
            case EggStage.Stage2:
                // Stage 2 지속 시간을 현실 시간(분)으로 변환
                float stage1And2RealTimeMinutesToEnd = (stage1Duration + stage2Duration) * (TimeManager.instance.realTimeMinutesPerGameDay / (float)TimeSpan.FromHours(24).TotalSeconds);
                
                // 현실 경과 시간이 목표 시간(Stage1 + Stage2 지속시간)을 넘었는지 확인
                if (realElapsedMinutes >= stage1And2RealTimeMinutesToEnd)
                {
                    // 스테이지 3으로 전환 (부화)
                    currentStage = EggStage.Stage3;
                    Debug.Log($"Egg entered Stage 3 - Hatching (현실 경과 시간: {realElapsedMinutes:F2}분, 게임 시간: {currentGameTime})");
                    
                    // 디버그 정보 즉시 업데이트
                    UpdateDebugTexts();
                    
                    HatchIntoEnemy();
                }
                break;
        }
    }
    
    private void UpdateVisuals(EggStage newStage)
    {
        // 이전 단계의 비주얼을 비활성화
        switch (currentStage)
        {
            case EggStage.Stage1:
                if (stage1Visual != null)
                    stage1Visual.SetActive(false);
                break;
            case EggStage.Stage2:
                if (stage2Visual != null)
                    stage2Visual.SetActive(false);
                break;
        }

        // 새로운 단계의 비주얼을 활성화
        GameObject targetVisual = null;
        switch (newStage)
        {
            case EggStage.Stage1:
                if (stage1Visual != null)
                {
                    stage1Visual.SetActive(true);
                    targetVisual = stage1Visual;
                }
                break;
            case EggStage.Stage2:
                if (stage2Visual != null)
                {
                    stage2Visual.SetActive(true);
                    targetVisual = stage2Visual;
                }
                break;
        }

        // 렌더러 상태 디버깅
        if (targetVisual != null)
        {
            Renderer[] renderers = targetVisual.GetComponentsInChildren<Renderer>();
            Debug.Log($"[Egg {id}] UpdateVisuals - Stage: {newStage}, Visual Active: {targetVisual.activeSelf}, Renderers: {renderers.Length}");
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    Debug.Log($"  - {renderer.gameObject.name}: enabled={renderer.enabled}, isVisible={renderer.isVisible}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[Egg {id}] UpdateVisuals - Stage {newStage}의 비주얼이 null입니다!");
        }

        // 단계 변화 이펙트 재생
        if (stageTransitionEffect != null)
        {
            stageTransitionEffect.Play();
        }
    }

    public void TakeDamage(int damage, DamageType damageType)
    {
        // Stage 1에서는 데미지를 받지 않음
        if (currentStage == EggStage.Stage1)
        {
            Debug.Log($"{gameObject.name}은(는) 아직 무적 상태입니다!");
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.missDamage, transform, 40f, "Egg Miss Damage");
            return;
        }

        // Stage 2에서만 데미지를 받음
        if (currentStage == EggStage.Stage2)
        {
            // 일단 속성에 
            // if (!IsWeakTo(damageType))
            // {
            //     Debug.Log($"{gameObject.name}({eggType})은(는) {damageType} 속성에 면역입니다!");
            //     AudioManager.instance.PlayOneShot(FMODEvents.instance.missDamage, transform);
            //     return;
            // }

            hp -= damage;
            Debug.Log($"{gameObject.name}({eggType}) Take Damage {damage} from {damageType} _ Left HP :{hp}");
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.damage, transform, 40f, "Egg Damage");
            
            if(hp <= 0)
            {
                DestroyEgg();
            }
        }
    }

    public void HatchIntoEnemy()
    {
        Vector3 eggPosition = transform.position;
        Debug.Log("Egg 부화!");

        // 성체 생성
        GameObject adult = Instantiate(enemyAIData.prefab, eggPosition, Quaternion.identity);
        EnemyManager.instance.activeEnemies.Add(adult);
        
        // AI 상태 설정
        var enemyBehavior = adult.GetComponent<EnemyAIController<BaseEnemyAIData>>();
        EnemyManager.instance.activeEggs.Remove(gameObject);
        Destroy(gameObject);
    }

    private bool IsWeakTo(DamageType damageType)
    {
        return (eggType, damageType) switch
        {
            (EnemyType.Machine, DamageType.Water) => true,     // 기계형은 물 속성에만 취약
            (EnemyType.Human, DamageType.Fire) => true,        // 인간형은 불 속성에만 취약
            (EnemyType.Animal, DamageType.Physical) => true,   // 동물형은 물리 속성에만 취약
            _ => false                                       // 그 외의 모든 조합은 면역
        };
    }

    void DestroyEgg()
    {
        // EggLevel2Break: 알이 파괴될 때 사운드
        // AudioManager.instance.PlayOneShot(FMODEvents.instance.EggLevel2Break, transform);
        
        // 알 파괴 후 파편을 생성
        Instantiate(eggShatterItemPrefab, transform.position, Quaternion.identity);
        
        // 파괴 시 타입에 따른 특수 효과 발생
        switch (eggType)
        {
            case EnemyType.Machine:
                Debug.Log("기계형 알 파괴 - 전기 스파크 발생!");
                break;
            case EnemyType.Human:
                Debug.Log("인간형 알 파괴 - 화염 폭발 발생!");
                break;
            case EnemyType.Animal:
                Debug.Log("동물형 알 파괴 - 물리 파편 발생!");
                break;
        }

        Destroy(gameObject);
    }

    public void AutoCompleteScanAfterDay()
    {
        // TimeManager.instance.OnDayPassed += () =>   
        // {
        //     isScanningOver = true;
        // };

        // 테스트용 코드
        isScanningOver = true;
    }

    /// <summary>
    /// 시간을 무시하고 즉시 부화시킵니다 (에디터 전용)
    /// </summary>
    public void ForceHatch()
    {
        Debug.Log($"[Egg {id}] 강제 부화 실행!");
        
        // Stage를 3으로 변경하고 즉시 부화
        currentStage = EggStage.Stage3;
        HatchIntoEnemy();
    }

    #region IScannable Implementation
    
    public string GetScanName()
    {
        if (isScanningOver)
        {
            // 스캔 완료 시 적의 이름 표시
            return enemyAIData != null ? enemyAIData.enemyName : "Unknown Enemy";
        }
        else
        {
            // 스캔 중일 때
            return "Unknown Egg";
        }
    }

    public string GetScanDescription()
    {
        if (isScanningOver)
        {
            // 스캔 완료 시 위험도와 타입 정보 표시
            string dangerLevel = enemyAIData != null ? enemyAIData.dangerLevel.ToString() : "Unknown";
            return $"Type: {eggType} | Danger: {dangerLevel} | Stage: {currentStage}";
        }
        else if (isScanning)
        {
            return "Scanning in progress...";
        }
        else
        {
            return "Not scanned yet";
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public EObjectTrackerUIType GetUIType()
    {
        return EObjectTrackerUIType.Egg;
    }

    public void OnScanned()
    {
        if (!isScanning)
        {
            isScanning = true;
            AutoCompleteScanAfterDay();
            Debug.Log($"[Egg {id}] 스캔 시작됨");
        }
    }
    
    #endregion
}
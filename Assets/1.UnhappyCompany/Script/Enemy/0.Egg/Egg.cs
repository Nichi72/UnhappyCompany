using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public enum EggStage
{
    Stage1,     // 초기 단계 (무적)
    Stage2,     // 중간 단계 (데미지 받음)
    Stage3      // 최종 단계 (부화)
}

public class Egg : MonoBehaviour, IDamageable
{
    public BaseEnemyAIData enemyAIData;
    [Header("Basic Settings")]
    [Tooltip("알이 부서졌을 때 생성할 파편 프리팹")] 
    public GameObject eggShatterItemPrefab;
    
    [Header("Egg Type Settings")]
    [Tooltip("알의 타입 (기계형/인간형/동물형)")]
    public EnemyType eggType;
    [Header("Stage Settings")]
    [Tooltip("각 단계별 지속 시간(초)")]
    [ReadOnly] public float stage1Duration = 60 * 10f;
    public float stage2Duration = 350f;
    public float randomRange = 150f;

    [Header("Stage Visuals")]
    [Tooltip("Stage 1의 외형")] 
    public GameObject stage1Visual;
    [Tooltip("Stage 2의 외형")] 
    public GameObject stage2Visual;
    
    [Tooltip("단계 변화시 재생할 파티클 이펙트")]
    public ParticleSystem stageTransitionEffect;
    
    private EggStage currentStage = EggStage.Stage1;
    public int hp { get; set; } = 100;
    public int id;
    public bool isScanning = false;
    public bool isScanningOver = false;


    private void Start()
    {
        id = EnemyManager.instance.EggID;
        EnemyManager.instance.EggID++;
        InitializeVisuals();
        StartCoroutine(EggStageProgression());
    }

    private void InitializeVisuals()
    {
        stage1Duration = TimeManager.instance.realTimeMinutesPerGameDay * 60;
        stage2Duration = Random.Range(stage2Duration-randomRange, stage2Duration+randomRange);
        // 모든 비주얼을 비활성화
        stage1Visual.SetActive(false);
        stage2Visual.SetActive(false);
        // Stage 1 비주얼만 활성화
        UpdateVisuals(EggStage.Stage1);
    }

    private void UpdateVisuals(EggStage newStage)
    {
        // 이전 단계의 비주얼을 비활성화
        switch (currentStage)
        {
            case EggStage.Stage1:
                stage1Visual.SetActive(false);
                break;
            case EggStage.Stage2:
                stage2Visual.SetActive(false);
                break;
        }

        // 새로운 단계의 비주얼을 활성화
        switch (newStage)
        {
            case EggStage.Stage1:
                stage1Visual.SetActive(true);
                break;
            case EggStage.Stage2:
                stage2Visual.SetActive(true);
                break;
        }

        // 단계 변화 이펙트 재생
        if (stageTransitionEffect != null)
        {
            stageTransitionEffect.Play();
        }
    }

    private IEnumerator EggStageProgression()
    {
        // Stage 1 (무적 상태)
        currentStage = EggStage.Stage1;
        UpdateVisuals(EggStage.Stage1);
        Debug.Log($"Egg entered Stage 1 - Invincible");
        yield return new WaitForSeconds(stage1Duration);

        // Stage 2 (데미지 받는 상태)
        currentStage = EggStage.Stage2;
        UpdateVisuals(EggStage.Stage2);
        Debug.Log($"Egg entered Stage 2 - Vulnerable");
        yield return new WaitForSeconds(stage2Duration);

        // Stage 3 (부화)
        currentStage = EggStage.Stage3;
        Debug.Log($"Egg entered Stage 3 - Hatching");
        HatchIntoEnemy();
    }

    public void TakeDamage(int damage, DamageType damageType)
    {
        // Stage 1에서는 데미지를 받지 않음
        if (currentStage == EggStage.Stage1)
        {
            Debug.Log($"{gameObject.name}은(는) 아직 무적 상태입니다!");
            AudioManager.instance.PlayOneShot(FMODEvents.instance.missDamage, transform);
            return;
        }

        // Stage 2에서만 데미지를 받음
        if (currentStage == EggStage.Stage2)
        {
            if (!IsWeakTo(damageType))
            {
                Debug.Log($"{gameObject.name}({eggType})은(는) {damageType} 속성에 면역입니다!");
                AudioManager.instance.PlayOneShot(FMODEvents.instance.missDamage, transform);
                return;
            }

            hp -= damage;
            Debug.Log($"{gameObject.name}({eggType}) Take Damage {damage} from {damageType} _ Left HP :{hp}");
            AudioManager.instance.PlayOneShot(FMODEvents.instance.damage, transform);
            
            if(hp <= 0)
            {
                DestroyEgg();
            }
        }
    }

    private void HatchIntoEnemy()
    {
        Vector3 eggPosition = transform.position;
        Debug.Log("Egg 부화!");
        
        // 랜덤한 적 타입 선택
        var soEnemies = EnemyManager.instance.soEnemies;
        int randomIndex = Random.Range(0, soEnemies.Count);
        var adultEnemyPrefab = soEnemies[randomIndex];
        
        // 성체 생성
        GameObject adult = Instantiate(adultEnemyPrefab.prefab, eggPosition, Quaternion.identity);
        EnemyManager.instance.activeEnemies.Add(adult);
        
        // AI 상태 설정
        var enemyBehavior = adult.GetComponent<EnemyAIController<BaseEnemyAIData>>();
        // if (enemyBehavior != null)
        // {
        //     enemyBehavior.ChangeState(new CubePatrolState(enemyBehavior.GetComponent<EnemyAICube>(), enemyBehavior.UtilityCalculator ));
        // }

        // 알 제거
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

}
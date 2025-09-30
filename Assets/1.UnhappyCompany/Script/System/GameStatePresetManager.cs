using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameStatePresetManager : MonoBehaviour
{
    public static GameStatePresetManager Instance { get; private set; }
    
    [Header("프리셋 목록")]
    [SerializeField] private List<GameStatePreset> availablePresets = new List<GameStatePreset>();
    
    [Header("시스템 참조")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private EnemyManager enemyManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 시스템 참조 자동 찾기
        if (timeManager == null)
            timeManager = TimeManager.instance;
        if (enemyManager == null)
            enemyManager = EnemyManager.instance;
    }
    
    /// <summary>
    /// 프리셋을 적용합니다
    /// </summary>
    public bool ApplyPreset(string presetName)
    {
        var preset = GetPresetByName(presetName);
        if (preset == null)
        {
            Debug.LogError($"[PresetManager] 프리셋을 찾을 수 없습니다: {presetName}");
            return false;
        }
        
        return ApplyPreset(preset);
    }
    
    /// <summary>
    /// 프리셋을 적용합니다
    /// </summary>
    public bool ApplyPreset(GameStatePreset preset)
    {
        if (preset == null)
        {
            Debug.LogError("[PresetManager] 프리셋이 null입니다");
            return false;
        }
        
        Debug.Log($"[PresetManager] 프리셋 적용 시작: {preset.presetName}");
        
        try
        {
            // 1. 기존 상태 정리
            ClearCurrentState(preset);
            
            // 2. 시간 설정
            ApplyTimeSettings(preset);
            
            // 3. 알 생성
            ApplyEggSettings(preset);
            
            // 4. 적 생성  
            ApplyEnemySettings(preset);
            
            // 5. 기타 설정
            ApplyMiscSettings(preset);
            
            Debug.Log($"[PresetManager] 프리셋 적용 완료: {preset.presetName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PresetManager] 프리셋 적용 중 오류: {ex.Message}");
            return false;
        }
    }
    
    private void ClearCurrentState(GameStatePreset preset)
    {
        if (preset.clearExistingEggs)
        {
            ClearAllEggs();
        }
        
        if (preset.clearExistingEnemies)
        {
            ClearAllEnemies();
        }
    }
    
    private void ApplyTimeSettings(GameStatePreset preset)
    {
        if (timeManager == null)
        {
            Debug.LogWarning("[PresetManager] TimeManager를 찾을 수 없습니다");
            return;
        }
        
        // 일수 설정
        timeManager.days = preset.day;
        
        // 시간 설정
        var newTime = TimeSpan.FromHours(preset.hour).Add(TimeSpan.FromMinutes(preset.minute));
        timeManager.GetType().GetProperty("GameTime").SetValue(timeManager, newTime);
        
        // 시간 배율 설정
        timeManager.timeMultiplier = preset.timeMultiplier;
        
        Debug.Log($"[PresetManager] 시간 설정: Day {preset.day}, {preset.hour:00}:{preset.minute:00}, 배율 {preset.timeMultiplier}x");
    }
    
    private void ApplyEggSettings(GameStatePreset preset)
    {
        if (enemyManager == null)
        {
            Debug.LogWarning("[PresetManager] EnemyManager를 찾을 수 없습니다");
            return;
        }
        
        var settings = preset.eggSettings;
        
        for (int i = 0; i < settings.eggCount; i++)
        {
            CreateRandomizedEgg(settings);
        }
        
        Debug.Log($"[PresetManager] {settings.eggCount}개의 알 생성 완료");
    }
    
    private void CreateRandomizedEgg(EggSpawnSettings settings)
    {
        // 랜덤 스폰 포인트 선택
        if (enemyManager.roomSettings.Count == 0)
        {
            Debug.LogWarning("[PresetManager] 사용 가능한 스폰 포인트가 없습니다");
            return;
        }
        
        var randomRoom = enemyManager.roomSettings[UnityEngine.Random.Range(0, enemyManager.roomSettings.Count)];
        if (randomRoom.eggSpawnPoints.Count == 0)
        {
            Debug.LogWarning($"[PresetManager] {randomRoom.name}에 스폰 포인트가 없습니다");
            return;
        }
        
        var randomSpawnPoint = randomRoom.eggSpawnPoints[UnityEngine.Random.Range(0, randomRoom.eggSpawnPoints.Count)];
        
        // 알 생성
        GameObject eggObj = Instantiate(enemyManager.eggPrefab, randomSpawnPoint.transform.position, Quaternion.identity);
        var egg = eggObj.GetComponent<Egg>();
        
        if (egg == null)
        {
            Debug.LogError("[PresetManager] 생성된 알에 Egg 컴포넌트가 없습니다");
            Destroy(eggObj);
            return;
        }
        
        // 랜덤 특성 적용
        ApplyRandomizedEggProperties(egg, settings);
        
        // EnemyManager에 등록
        enemyManager.AddEgg(eggObj);
    }
    
    private void ApplyRandomizedEggProperties(Egg egg, EggSpawnSettings settings)
    {
        // 1. 알 타입 설정
        if (settings.randomizeEggType)
        {
            egg.eggType = GetRandomEnemyType(settings);
        }
        else
        {
            egg.eggType = settings.fixedEggType;
        }
        
        // 2. 초기 단계 설정
        EggStage initialStage = EggStage.Stage1;
        if (settings.randomizeInitialStage)
        {
            initialStage = UnityEngine.Random.value < settings.stage1StartChance ? EggStage.Stage1 : EggStage.Stage2;
        }
        else
        {
            initialStage = settings.fixedInitialStage;
        }
        
        // 3. 부화할 적 데이터 설정
        if (settings.randomizeEnemyData)
        {
            if (enemyManager.soEnemies.Count > 0)
            {
                egg.enemyAIData = enemyManager.soEnemies[UnityEngine.Random.Range(0, enemyManager.soEnemies.Count)];
            }
        }
        else if (!string.IsNullOrEmpty(settings.fixedEnemyDataName))
        {
            var enemyData = enemyManager.soEnemies.FirstOrDefault(e => e.enemyName == settings.fixedEnemyDataName);
            if (enemyData != null)
            {
                egg.enemyAIData = enemyData;
            }
        }
        
        // 4. 단계 지속시간 랜덤 설정
        if (settings.randomizeStageDuration)
        {
            egg.stage1RealTimeMinutes = UnityEngine.Random.Range(settings.stage1DurationRange.x, settings.stage1DurationRange.y);
            egg.stage2RealTimeMinutes = UnityEngine.Random.Range(settings.stage2DurationRange.x, settings.stage2DurationRange.y);
        }
        
        // 초기 단계 강제 설정 (Stage2로 시작하는 경우)
        if (initialStage == EggStage.Stage2)
        {
            // Stage1을 즉시 완료시키고 Stage2로 전환
            var currentStageField = typeof(Egg).GetField("currentStage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentStageField?.SetValue(egg, EggStage.Stage2);
        }
        
        Debug.Log($"[PresetManager] 알 설정 완료 - 타입: {egg.eggType}, 단계: {initialStage}, 적: {egg.enemyAIData?.enemyName}");
    }
    
    private EnemyType GetRandomEnemyType(EggSpawnSettings settings)
    {
        float rand = UnityEngine.Random.value;
        
        if (rand < settings.machineTypeChance)
            return EnemyType.Machine;
        else if (rand < settings.machineTypeChance + settings.humanTypeChance)
            return EnemyType.Human;
        else
            return EnemyType.Animal;
    }
    
    private void ApplyEnemySettings(GameStatePreset preset)
    {
        if (enemyManager == null)
        {
            Debug.LogWarning("[PresetManager] EnemyManager를 찾을 수 없습니다");
            return;
        }
        
        var settings = preset.enemySettings;
        
        if (settings.enemiesToSpawn.Count == 0)
        {
            Debug.Log("[PresetManager] 생성할 적이 지정되지 않았습니다");
            return;
        }
        
        int successCount = 0;
        for (int i = 0; i < settings.enemiesToSpawn.Count; i++)
        {
            var enemyData = settings.enemiesToSpawn[i];
            if (CreateSpecificEnemy(enemyData))
            {
                successCount++;
            }
        }
        
        Debug.Log($"[PresetManager] {successCount}마리의 적 생성 완료 (총 시도: {settings.enemiesToSpawn.Count})");
    }
    
    private bool CreateSpecificEnemy(BaseEnemyAIData enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogWarning("[PresetManager] 적 데이터가 null입니다");
            return false;
        }
        
        if (enemyData.prefab == null)
        {
            Debug.LogError($"[PresetManager] {enemyData.enemyName}의 프리팹이 null입니다");
            return false;
        }
        
        // 랜덤 스폰 포인트 선택
        if (enemyManager.roomSettings.Count == 0)
        {
            Debug.LogWarning("[PresetManager] 사용 가능한 스폰 포인트가 없습니다");
            return false;
        }
        
        var randomRoom = enemyManager.roomSettings[UnityEngine.Random.Range(0, enemyManager.roomSettings.Count)];
        if (randomRoom.eggSpawnPoints.Count == 0)
        {
            Debug.LogWarning($"[PresetManager] {randomRoom.name}에 스폰 포인트가 없습니다");
            return false;
        }
        
        var randomSpawnPoint = randomRoom.eggSpawnPoints[UnityEngine.Random.Range(0, randomRoom.eggSpawnPoints.Count)];
        
        // 적 생성
        GameObject enemy = Instantiate(enemyData.prefab, randomSpawnPoint.transform.position, Quaternion.identity);
        
        // EnemyAIController 컴포넌트 확인
        var controller = enemy.GetComponent<EnemyAIController>();
        if (controller == null)
        {
            Debug.LogError($"[PresetManager] 생성된 적 {enemy.name}에 EnemyAIController 컴포넌트가 없습니다!");
            Destroy(enemy);
            return false;
        }
        
        // 적 이름 설정
        enemy.name = $"{enemyData.enemyName}_{enemyManager.activeEnemies.Count}";
        
        // EnemyManager에 등록
        enemyManager.activeEnemies.Add(enemy);
        
        Debug.Log($"[PresetManager] 적 생성 완료 - 타입: {enemyData.enemyName}, 위치: {randomSpawnPoint.name}");
        return true;
    }
    
    private void ApplyMiscSettings(GameStatePreset preset)
    {
        if (preset.pauseTimeOnApply && timeManager != null)
        {
            timeManager.IsStop = true;
            Debug.Log("[PresetManager] 시간 일시정지됨");
        }
    }
    
    private void ClearAllEggs()
    {
        if (enemyManager == null) return;
        
        var eggs = new List<GameObject>(enemyManager.activeEggs);
        foreach (var egg in eggs)
        {
            if (egg != null)
            {
                enemyManager.RemoveEgg(egg);
                Destroy(egg);
            }
        }
        Debug.Log($"[PresetManager] {eggs.Count}개의 알 제거됨");
    }
    
    private void ClearAllEnemies()
    {
        if (enemyManager == null) return;
        
        var enemies = new List<GameObject>(enemyManager.activeEnemies);
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        enemyManager.activeEnemies.Clear();
        Debug.Log($"[PresetManager] {enemies.Count}마리의 적 제거됨");
    }
    
    public GameStatePreset GetPresetByName(string name)
    {
        return availablePresets.FirstOrDefault(p => p.presetName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public List<string> GetAvailablePresetNames()
    {
        return availablePresets.Select(p => p.presetName).ToList();
    }
    
    public List<GameStatePreset> GetAvailablePresets()
    {
        return new List<GameStatePreset>(availablePresets);
    }
    
    public void AddPreset(GameStatePreset preset)
    {
        if (preset != null && !availablePresets.Contains(preset))
        {
            availablePresets.Add(preset);
        }
    }
    
    public void RemovePreset(GameStatePreset preset)
    {
        availablePresets.Remove(preset);
    }
    
    /// <summary>
    /// 치트 명령어: 프리셋 목록 출력
    /// </summary>
    [CheatCommand("preset_list", "사용 가능한 프리셋 목록을 출력합니다", "Test")]
    public void ListPresets()
    {
        if (availablePresets.Count == 0)
        {
            Debug.Log("[PresetManager] 사용 가능한 프리셋이 없습니다");
            return;
        }
        
        Debug.Log($"[PresetManager] 사용 가능한 프리셋 ({availablePresets.Count}개):");
        for (int i = 0; i < availablePresets.Count; i++)
        {
            var preset = availablePresets[i];
            Debug.Log($"  {i + 1}. {preset.presetName} - {preset.description}");
        }
    }
    
    /// <summary>
    /// 치트 명령어: 프리셋 적용
    /// </summary>
    [CheatCommand("preset_load", "지정된 프리셋을 적용합니다", "Test")]
    public void LoadPresetCommand(string presetName)
    {
        if (string.IsNullOrEmpty(presetName))
        {
            Debug.LogError("[PresetManager] 프리셋 이름을 입력해주세요");
            return;
        }
        
        ApplyPreset(presetName);
    }
    
    /// <summary>
    /// 치트 명령어: 프리셋 정보 출력
    /// </summary>
    [CheatCommand("preset_info", "지정된 프리셋의 상세 정보를 출력합니다", "Test")]
    public void ShowPresetInfo(string presetName)
    {
        var preset = GetPresetByName(presetName);
        if (preset == null)
        {
            Debug.LogError($"[PresetManager] 프리셋을 찾을 수 없습니다: {presetName}");
            return;
        }
        
        Debug.Log($"=== 프리셋 정보: {preset.presetName} ===");
        Debug.Log($"설명: {preset.description}");
        Debug.Log($"시간: Day {preset.day}, {preset.hour:00}:{preset.minute:00} (배율 {preset.timeMultiplier}x)");
        Debug.Log($"알: {preset.eggSettings.eggCount}개 (랜덤 타입: {preset.eggSettings.randomizeEggType})");
        Debug.Log($"적: {preset.enemySettings.enemiesToSpawn.Count}마리 ({string.Join(", ", preset.enemySettings.enemiesToSpawn.Where(e => e != null).Select(e => e.enemyName))})");
        Debug.Log($"기존 정리: 알 {preset.clearExistingEggs}, 적 {preset.clearExistingEnemies}");
    }
} 
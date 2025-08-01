using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class EggSpawnSettings
{
    [Header("기본 설정")]
    public int eggCount = 5;
    
    [Header("랜덤 설정")]
    public bool randomizeEggType = true;
    public bool randomizeInitialStage = true;
    public bool randomizeEnemyData = true;
    public bool randomizeStageDuration = true;
    
    [Header("수동 설정 (랜덤 비활성화시 사용)")]
    public EnemyType fixedEggType = EnemyType.Machine;
    public EggStage fixedInitialStage = EggStage.Stage1;
    public string fixedEnemyDataName = ""; // 비어있으면 랜덤
    
    [Header("랜덤 범위 설정")]
    [Range(0f, 1f)] public float machineTypeChance = 0.33f;
    [Range(0f, 1f)] public float humanTypeChance = 0.33f;
    // Animal은 나머지 확률 (1 - machine - human)
    
    [Range(0f, 1f)] public float stage1StartChance = 0.7f; // Stage1으로 시작할 확률
    
    [Header("시간 랜덤 범위")]
    public Vector2 stage1DurationRange = new Vector2(8f, 12f); // 분 단위
    public Vector2 stage2DurationRange = new Vector2(3f, 8f); // 분 단위
}

[System.Serializable]
public class EnemySpawnSettings
{
    [Header("생성할 적 목록")]
    [Tooltip("리스트에 추가된 적들이 각각 하나씩 생성됩니다")]
    public List<BaseEnemyAIData> enemiesToSpawn = new List<BaseEnemyAIData>();
    
    [Header("추가 설정")]
    [Tooltip("같은 적을 여러 마리 생성하고 싶다면 리스트에 여러 번 추가하세요")]
    public bool showSpawnCount = true; // Inspector에서 개수를 보여주기 위한 변수
}

[CreateAssetMenu(fileName = "GameStatePreset", menuName = "UnhappyCompany/Test/GameStatePreset")]
public class GameStatePreset : ScriptableObject
{
    [Header("프리셋 정보")]
    public string presetName = "새 프리셋";
    [TextArea(2, 4)]
    public string description = "프리셋 설명을 입력하세요";
    
    [Header("시간 설정")]
    public int day = 1;
    [Range(0, 23)] public int hour = 6;
    [Range(0, 59)] public int minute = 0;
    [Range(0.5f, 4f)] public float timeMultiplier = 1f;
    
    [Header("알 설정")]
    public EggSpawnSettings eggSettings = new EggSpawnSettings();
    
    [Header("적 설정")]
    public EnemySpawnSettings enemySettings = new EnemySpawnSettings();
    
    [Header("정리 설정")]
    public bool clearExistingEggs = true;
    public bool clearExistingEnemies = true;
    
    [Header("기타 설정")]
    public bool pauseTimeOnApply = false; // 프리셋 적용 후 시간 일시정지
}
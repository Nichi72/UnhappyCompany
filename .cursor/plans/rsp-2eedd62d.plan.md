<!-- 2eedd62d-f66e-4fc4-87ad-b12800ad819b 6bc4eecf-6261-4044-aebb-5f0708659775 -->
# RSP 메달 게임 개선 플랜

## 1. 코인 잭팟 시스템 개선 (RSPSystem.cs)

### 현재 상황

- `SpawnRewardCoins()` 메서드가 coinPrefab을 사용해 코인을 즉시 생성
- 모든 코인이 한번에 생성되어 물리 효과로 흩어짐

### 변경 사항

- ItemData_17_Coin을 Resources.Load로 로드
- 코인을 순차적으로 하나씩 떨어뜨리는 코루틴 구현
- `coinSpawnTransform` 추가 (Inspector에서 설정)
- ItemData.InstantiateItem() 메서드 사용
- 코인 생성 간격과 물리 힘을 설정 가능하게

**파일**: `RSPSystem.cs`

- coinPrefab 필드 제거, ItemData 사용
- [SerializeField] Transform coinSpawnTransform 추가
- [SerializeField] float coinSpawnInterval = 0.15f
- [SerializeField] Vector2 coinForceRange = new Vector2(3f, 5f)
- SpawnRewardCoinsSequentially() 코루틴 생성

## 2. 스택별 에미션 시스템 (EnemyAIRSP.cs)

### 구조 설계

```csharp
[Serializable]
public struct StackEmissionData
{
    public int stackLevel;
    public Color emissionColor;
    public float emissionIntensity;
    public float blinkInterval;
    
    public StackEmissionData(int level, Color color, float intensity, float blink)
    {
        stackLevel = level;
        emissionColor = color;
        emissionIntensity = intensity;
        blinkInterval = blink;
    }
}
```

### 필드 선언 시 기본값 설정 (Inspector에 미리 표시됨)

```csharp
[SerializeField] private StackEmissionData[] stackEmissionProfiles = new StackEmissionData[]
{
    new StackEmissionData(0, new Color(0f, 1f, 0f), 1000f, 0f),
    new StackEmissionData(1, new Color(1f, 0.92f, 0.016f), 1500f, 2f),
    new StackEmissionData(2, new Color(1f, 0.5f, 0f), 2000f, 1f),
    new StackEmissionData(3, new Color(1f, 0.2f, 0f), 2500f, 0.5f),
    new StackEmissionData(4, new Color(1f, 0f, 0f), 3000f, 0.25f)
};
```

**파일**: `EnemyAIRSP.cs`

- StackEmissionData 구조체 추가 (생성자 포함)
- [SerializeField] Renderer[] stackEmissionRenderers 추가
- [SerializeField] StackEmissionData[] 기본값으로 초기화
- UpdateStackEmission(int stackLevel) 메서드
- BlinkStackEmission() 코루틴
- Coroutine blinkCoroutine 필드로 관리
- IncrementStack/DecrementStack에서 UpdateStackEmission 호출
- OnDestroy에서 코루틴 정리

## 3. 코인 부족 시 플레이어 즉사 (RSPHoldingState.cs)

### 변경 사항

`OnRSPMedalGameComplete` 메서드의 hasNoCoin 처리 부분 수정:

- 코인 부족 시 스택이 4 이상이면 플레이어 즉사
- 스택이 4 미만이면 기존처럼 속박 해제
- `player.playerStatus.TakeDamage(9999, DamageType.Other)` 호출

**파일**: `RSPHoldingState.cs`

- OnRSPMedalGameComplete 메서드 수정

## 4. 메달 결과에 따른 코인 지급 (RSPSystem.cs, RSPMedalGame.cs)

### 변경 사항

- RSPMedalGame에 lastResult 필드와 GetLastResult() 메서드 추가
- RSPSystem에서 실제 메달 결과 사용
- 순차 코인 생성 코루틴 호출

**파일**:

- `RSPMedalGame.cs`: int lastResult, GetLastResult() 추가
- `RSPSystem.cs`: medalGame.GetLastResult()로 결과 가져오기

## 주의사항

1. stackEmissionRenderers는 emissionRenderers와 완전 분리
2. 깜박임 코루틴은 스택 변경/OnDestroy 시 정리
3. Resources.Load("ScriptableObj/Item/ItemData_17_Coin") 경로 확인
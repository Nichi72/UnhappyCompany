# 확장성 시스템

## 무한 진행 구조 (리썰컴퍼니 방식)

### 진행 방식

#### 사이클 1-3: 튜토리얼 단계
- **맵 크기**: 소규모 (센터 반경 50m)
- **크리처**: 1-2종, 낮은 공격성
- **필요 자원**: 적음
- **학습 목표**: 기본 메커니즘 숙련

#### 사이클 4-10: 성장 단계
- **맵 크기**: 중간 (센터 반경 100m)
- **크리처**: 3-4종, 중간 공격성
- **필요 자원**: 증가
- **도전 목표**: 효율적 전략 개발

#### 사이클 11+: 생존 단계
- **맵 크기**: 대규모 (센터 반경 200m+)
- **크리처**: 5+종, 높은 공격성
- **필요 자원**: 많음
- **CCTV 한계**: 커버리지 부족
- **목표**: 한계까지 버티기

---

## 자연스러운 난이도 증가

### 맵 확장으로 인한 효과

#### 물리적 제약 증가
- **탐험 시간 증가**: 더 멀리 가야 함
- **복귀 시간 증가**: 안개 시 위험
- **이동 피로도**: 더 많은 체력 소모
- **길 잃기 위험**: 복잡한 맵 구조

#### 전략적 복잡성 증가
- **CCTV 커버리지 한계**: 20개로 부족
- **크리처 접근 경로 다양화**: 예측 어려움
- **자원 관리 복잡성**: 더 많은 변수
- **시간 압박 강화**: 더 넓은 영역 관리

### 플레이어 부담감 증가

#### 심리적 압박
- **정보 부족**: 모든 구역 감시 불가
- **선택 부담**: 어디를 우선할지 고민
- **실패 두려움**: 한 번의 실수가 치명적
- **팀 의존도**: 혼자서는 불가능

#### 물리적 압박
- **더 많은 이동**: 체력과 시간 소모
- **빠른 판단**: 순간적 의사결정 필요
- **정확한 조작**: 실수 허용 범위 감소
- **지속적 집중**: 긴 시간 동안 고도 집중

---

## 엔딩 없는 설계 철학

### 리썰컴퍼니 참조 분석
- **리썰컴퍼니**: 할당량 점진적 증가 → **우리**: 맵 범위 확장
- **리썰컴퍼니**: 시간 압박 강화 → **우리**: CCTV 커버리지 한계
- **리썰컴퍼니**: 크리처 위험도 증가 → **우리**: 복잡성 증가로 실수 증가
- **리썰컴퍼니**: 사회적 압박감 → **우리**: 팀 협조 한계점 도달

### 자연스러운 한계 설정

#### 기술적 한계
- **CCTV 20개**: 하드웨어적 제한
- **전력 용량**: 물리적 배터리 한계
- **플레이어 반응속도**: 인간의 한계
- **팀 협조 한계**: 의사소통 복잡성

#### 게임플레이 한계
- **맵 크기**: 무한정 확장하면서 관리 불가능
- **크리처 수**: 동시 처리 가능한 위협 수
- **정보 처리**: 한 번에 파악 가능한 정보량
- **스트레스 한계**: 지속 가능한 긴장감

---

## 확장 메커니즘

### 맵 확장 알고리즘

#### 방사형 확장
```csharp
public class MapScaler : MonoBehaviour
{
    [Header("Scaling Parameters")]
    public float baseRadius = 50f;
    public float radiusIncrement = 25f;
    public int roomDensity = 2; // 반지름 10m당 방 개수
    
    public void ScaleMapForCycle(int cycle)
    {
        float currentRadius = baseRadius + (cycle * radiusIncrement);
        int targetRoomCount = Mathf.RoundToInt(currentRadius / 10f * roomDensity);
        
        // 새로운 구역 생성
        GenerateNewSector(currentRadius, targetRoomCount);
    }
}
```

#### 섹터별 확장
```
사이클 1: [센터] (반지름 50m)
사이클 4: [센터][1구역] (반지름 100m)
사이클 7: [센터][1구역][2구역] (반지름 150m)
사이클 10: [센터][1구역][2구역][3구역] (반지름 200m)
```

### 크리처 스케일링

#### 종류 증가
- **사이클 1-3**: 렘페이지만
- **사이클 4-6**: 렘페이지 + RSP
- **사이클 7-9**: 렘페이지 + RSP + Moo
- **사이클 10+**: 전체 + 새로운 크리처

#### 능력치 증가
```csharp
public class CreatureScaler : MonoBehaviour
{
    public float baseHealth = 100f;
    public float baseDamage = 20f;
    public float baseSpeed = 5f;
    
    public CreatureStats ScaleForCycle(int cycle)
    {
        float multiplier = 1f + (cycle * 0.1f); // 사이클당 10% 증가
        
        return new CreatureStats
        {
            health = baseHealth * multiplier,
            damage = baseDamage * multiplier,
            speed = baseSpeed * Mathf.Min(multiplier, 2f) // 속도는 2배까지만
        };
    }
}
```

### 자원 요구량 증가

#### 전력 소모 증가
- **기본 소모**: 고정 (컴퓨터, 조명)
- **CCTV 필요량**: 맵 크기에 비례 증가
- **방어 빈도**: 크리처 수 증가로 문 사용 빈도 증가

#### 아이템 필요량 증가
```csharp
public class ResourceScaler : MonoBehaviour
{
    public int CalculateRequiredBatteries(int cycle)
    {
        // 사이클이 증가할수록 더 많은 배터리 필요
        return Mathf.RoundToInt(2f + (cycle * 0.5f));
    }
    
    public int CalculateRequiredTraps(int cycle)
    {
        // 크리처 증가에 따른 트랩 필요량
        return Mathf.RoundToInt(1f + (cycle * 0.3f));
    }
}
```

---

## 플레이어 적응 곡선

### 학습 단계별 설계

#### 1단계: 기본 학습 (사이클 1-3)
- **목표**: 기본 조작법 익히기
- **허용**: 실수해도 회복 가능
- **제공**: 충분한 자원과 시간

#### 2단계: 전략 개발 (사이클 4-7)
- **목표**: 효율적 전략 찾기
- **요구**: 선택적 사고와 계획
- **제한**: 자원 제약으로 선택 강요

#### 3단계: 완성도 추구 (사이클 8-12)
- **목표**: 팀워크 최적화
- **요구**: 정밀한 협조와 실행
- **압박**: 실수 시 즉시 위험

#### 4단계: 한계 도전 (사이클 13+)
- **목표**: 개인/팀 한계 시험
- **요구**: 완벽에 가까운 플레이
- **결과**: 자연스러운 실패와 재도전

### 심리적 압박 곡선

```
긴장도
  ↑
  |     ╱╲
  |    ╱  ╲
  |   ╱    ╲_____ 
  |  ╱           ╲____
  | ╱                 ╲___
  |╱                      ╲____
  +━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━→ 사이클
  1  3   7    12   20   30   40
```

---

## 기술적 확장성

### 모듈화 설계

#### 크리처 모듈
```csharp
[CreateAssetMenu(fileName = "New Creature", menuName = "Lab Watcher/Creature")]
public class CreatureData : ScriptableObject
{
    public string creatureName;
    public CreatureAttribute attribute;
    public GameObject prefab;
    public CreatureBehavior morningBehavior;
    public CreatureStats baseStats;
    
    // 확장시 새로운 크리처 쉽게 추가
}
```

#### 맵 템플릿 시스템
```csharp
[CreateAssetMenu(fileName = "New Room Template", menuName = "Lab Watcher/Room Template")]
public class RoomTemplate : ScriptableObject
{
    public GameObject roomPrefab;
    public ItemSpawnRule[] spawnRules;
    public float riskLevel;
    public RoomType roomType;
    
    // 새로운 방 타입 쉽게 추가
}
```

### 데이터 기반 확장

#### 밸런싱 데이터
```json
{
  "cycles": [
    {
      "cycleNumber": 1,
      "mapRadius": 50,
      "creatures": ["Rampage"],
      "difficultyMultiplier": 1.0
    },
    {
      "cycleNumber": 4,
      "mapRadius": 100,
      "creatures": ["Rampage", "RSP"],
      "difficultyMultiplier": 1.3
    }
  ]
}
```

#### 업데이트 용이성
- **크리처 추가**: ScriptableObject로 즉시 추가
- **맵 요소**: 프리팹 기반으로 빠른 제작
- **밸런싱**: JSON 파일로 실시간 조정
- **새 메커니즘**: 모듈화된 시스템에 플러그인 방식

---

## 장기 확장 계획

### 콘텐츠 확장
- **새로운 테마**: 병원 → 연구소 → 지하시설
- **특수 이벤트**: 전력 정전, 크리처 무리 출현
- **보스 크리처**: 특별한 메커니즘의 대형 위협
- **환경 위험**: 화재, 독가스, 구조물 붕괴

### 시스템 확장
- **업그레이드 트리**: 영구적 발전 요소
- **캐릭터 특성**: 플레이어별 특화 능력
- **이야기 모드**: 캠페인 형식의 스토리
- **경쟁 모드**: 팀 vs 팀 생존 경쟁

### 커뮤니티 확장
- **맵 에디터**: 사용자 제작 맵
- **크리처 워크샵**: 커뮤니티 크리처 제작
- **리더보드**: 최고 생존 기록 경쟁
- **스트리밍 모드**: 관전자 시점 제공

# Enemy AI 시스템 개발 문서

> **최종 업데이트**: 2025-10-13
> **개발 완료**: Moo, Rampage 디버그 시스템 및 게임플레이 피드백

---

## 목차
1. [공통 시스템 (Base)](#공통-시스템-base)
2. [Moo 크리처](#moo-크리처)
3. [Rampage 크리처](#rampage-크리처)
4. [RSP 크리처](#rsp-크리처)

---

## 공통 시스템 (Base)

### EnemyAIController - 게임뷰 디버그 UI 시스템

모든 Enemy가 상속받아 사용하는 통합 디버그 시스템입니다.

#### 주요 기능

**1. 게임뷰 실시간 정보 표시**
- HP 바 (색상 코드: 초록 > 노랑 > 빨강)
- 상태 텍스트 (현재 State 이름)
- Enemy 이름 표시

**2. 범위 시각화**
- Patrol 범위 (초록색, Min/Max 그라데이션)
- Flee 범위 (빨간색, Min/Max 그라데이션)
- 게임뷰에서 실시간 확인 가능

**3. 목표 지점 시각화**
- 현재 이동 목표 지점 (주황색 선 + 마커)
- 목표 지점 라벨 (Enemy 이름 + 목표 타입)

**4. 헬퍼 메서드 제공**
```csharp
// 월드 → GUI 좌표 변환
protected Vector2 WorldToGUIPoint(Vector3 worldPoint)

// GUI에 선 그리기
protected void DrawGUILine(Vector2 start, Vector2 end, Color color, float thickness)

// 원형 범위 그리기
protected void DrawWorldCircleGUI(Vector3 center, float radius, Color color, int segments)

// 시야각 부채꼴 그리기
protected void DrawWorldVisionCone(Vector3 center, Vector3 forward, float range, float angle, Color color, int segments)

// 외곽선 텍스트 그리기
protected void DrawTextWithOutline(float x, float y, float width, float height, string text, GUIStyle style)
```

#### 상속받는 클래스에서 오버라이드 가능한 메서드

```csharp
// 기본 HP/상태 바 그리기
protected virtual void DrawDebugBars()

// 상태 텍스트 표시
protected virtual void DrawStateText(float x, float y, float width)

// Enemy별 특수 디버그 정보
protected virtual void DrawCustomDebugInfo()

// Enemy 표시 이름
protected virtual string GetEnemyDisplayName()
```

#### Inspector 설정

**Debug UI Settings**
- `isShowDebug`: 디버그 정보 표시 여부 (bool)
- `debugUIScale`: UI 크기 배율 (float, 기본값: 1.4)
- `currentTargetPosition`: 목표 지점 (Vector3?, HideInInspector)
- `currentTargetLabel`: 목표 라벨 (string, HideInInspector)

**Base Range Settings**
- `patrolRadius`: 순찰 반경 (float)
- `patrolDistanceMinRatio`: 순찰 최소 거리 비율 (0~2)
- `patrolDistanceMaxRatio`: 순찰 최대 거리 비율 (0~2)
- `fleeDistanceMinRatio`: 도망 최소 거리 비율 (0~2)
- `fleeDistanceMaxRatio`: 도망 최대 거리 비율 (0~2)
- `patrolRangeColor`: 순찰 범위 색상 (Color)
- `fleeRangeColor`: 도망 범위 색상 (Color)
- `showRangesInGame`: 게임뷰 범위 표시 (bool)

---

## Moo 크리처

### 개요
- **타입**: 노말형 🔨
- **컨셉**: 귀찮지만 쉬운 파훼법이 있는 초보자용 크리처
- **주요 메커니즘**: 기력 시스템 + 시야/청각 감지 + NavMesh 도망

### 핵심 시스템

#### 1. 기력(Stamina) 시스템

**변수**
```csharp
public float maxStamina = 100f;              // 최대 기력
private float currentStamina = 100f;         // 현재 기력
public float staminaDrainRate = 20f;         // 도망 시 초당 소모량
public float staminaRecoveryRate = 5f;       // 배회 시 초당 회복량
public float staminaLossOnHit = 30f;         // 피격 시 즉시 소모량
public float exhaustedThreshold = 10f;       // 지침 상태 기준값
```

**상태 전환**
- **배회 → 도망**: 기력 소모 (초당 20)
- **도망 → 지침**: 기력 <= 10
- **지침 → 배회**: 기력 >= 30% (또는 5초 경과)
- **피격**: 즉시 기력 -30

#### 2. 감지 시스템

**시각 감지 (Visual)**
- 시야 거리: `vision.sightRange` (5m)
- 시야각: `vision.sightAngle` (120°)
- 장애물 체크: `vision.obstacleLayer`
- 게임뷰 시각화: 노란색 부채꼴

**청각 감지 (Sound)**
- 감지 범위: `soundDetectionRange` (5m)
- 조건: 플레이어가 달리기 중 (`PlayerStatus.IsCurrentRun`)
- 게임뷰 시각화: 청록색 원

**통합 감지**
```csharp
public bool DetectPlayerThreat(out string detectionType)
{
    // Visual 또는 Sound 감지 시 true 반환
    // detectionType: "Visual", "Sound", "Both"
}
```

#### 3. 도망 시스템

**도망 경로 결정 (3단계)**

1. **Direct Away**: 플레이어 반대 방향으로 도망
   - 거리: `FleeDistanceMin ~ FleeDistanceMax` 랜덤
   - 조건: 플레이어와 최소 거리 이상 유지

2. **Adjusted Angle**: 반대 방향 실패 시 각도 조정
   - 각도: ±45° 랜덤 조정
   - 시도: 5회

3. **Far Random**: 모두 실패 시 먼 랜덤 위치
   - 거리: `FleeDistanceMax × 1.5 ~ 2.0` 랜덤
   - 방향: 완전 랜덤

**도망 중 슬라임 배출**
- 확률: 30%
- 타이밍: 도망 시작 후 2~5초 (랜덤)
- 조건: 여전히 Flee 상태일 때만

#### 4. State 구조

**MooWanderState** (배회)
- 5초마다 랜덤 위치로 이동
- 플레이어 감지 시 → MooFleeState
- 기력 회복: 초당 5

**MooFleeState** (도망)
- 플레이어 반대 방향으로 도망
- 기력 소진 시 → MooExhaustedState
- 목적지 도달/시간 초과 시 → MooWanderState
- 기력 소모: 초당 20

**MooExhaustedState** (지침)
- 움직임 정지, 울음 애니메이션
- 기력 회복: 초당 2.5 (느림)
- 회복 조건: 기력 >= 30% 또는 5초 경과

**MooSlimeEmitState** (슬라임 배출)
- 10초마다 자동 발동
- 즉시 슬라임 배출 후 → MooWanderState

**MooCenterAttackState** (센터 공격)
- 오후 페이즈 전용
- 센터로 이동 후 공격

### 디버그 UI (게임뷰)

**표시 정보**
```
━━━━━━━━━━━━━━━━━━━━━━━━━
HP        ████████░░ 40/50
Stamina   ████░░░░░░ 35/100
State: MooFleeState [EXHAUSTED]
Detection: Visual
━━━━━━━━━━━━━━━━━━━━━━━━━

🔵 청각 범위 (5m, 청록색 원)
🟡 시야 범위 (5m, 120°, 노란 부채꼴)
🟠 도망 목표 지점 (주황 선 + 마커)
```

**오버라이드된 메서드**
```csharp
protected override void DrawDebugBars()
{
    // HP + Stamina 바 표시
}

protected override void DrawStateText(float x, float y, float width)
{
    // State + [EXHAUSTED] + Detection 정보
}

protected override void DrawCustomDebugInfo()
{
    // 청각/시야 범위 시각화
}

protected override string GetEnemyDisplayName()
{
    return "Moo";
}
```

### 게임플레이 파훼법

**1. 스니킹 (가장 효율적)** ⭐
- 걷기로 접근 (뒤에서도 OK)
- 소리 감지 회피
- 시야각 밖에서 접근

**2. 원거리 공격 (안전)** 🎯
- 멀리서 총으로 공격
- 탄약 소모 필요

**3. 추격 (시간 소모)** 🏃
- 기력 소진까지 쫓아가기
- 지침 상태에서 처치
- 가장 비효율적

---

## Rampage 크리처

### 개요
- **타입**: 기계형 💧
- **컨셉**: 돌진형 고위력 크리처
- **주요 메커니즘**: 패널 시스템 + 2단계 돌진 + 충돌 처리

### 핵심 시스템

#### 1. 돌진 메커니즘 (2단계)

**Stage 1: NavMesh 추격**
- 플레이어를 실시간 추적
- 속도: `moveSpeed` (기본값)
- 종료 조건: 플레이어와 거리 <= `attackRadius` (2m)

**Stage 2: 물리 돌진**
- 고정된 방향으로 직선 돌진
- 속도: `rushSpeed` (빠름)
- 방향: 전환 시점의 속도 벡터 (고정)
- 종료 조건: 충돌 또는 시간 초과 (5초)

**전환 시점 정보 저장**
```csharp
controller.chargeStartPosition  // 돌진 시작 지점 (초록)
controller.chargeDirection      // 고정된 방향
controller.chargeTargetPoint    // 계산된 목표 지점 (빨강)
controller.hasChargeTarget      // 돌진 목표 설정 여부
```

#### 2. 패널 시스템

**패널 체력**
- 최대 패널 체력: `maxPanelHealth` (6)
- 현재 패널 체력: `CurrentPanelHealth`
- 패널 0 → Disabled 상태 (처치 가능)

**패널 노출 방식**
- **쿠션 충돌**: 패널 3개 노출, HP 유지
- **벽 충돌 (쿠션 없음)**: 패널 1개 노출, HP -10

#### 3. 충돌 처리 (RampageTrigger)

**Pushable 오브젝트** 📦
- 소리: `rampageCollisionObject`
- 효과: 밀어냄 (pushStrength = 10)
- 돌진 계속

**Wall 충돌** 🧱
- 소리: ~~`rampageCollisionWall`~~ **(제거됨)**
- 효과: HP -10 (쿠션 없을 때)
- 돌진 중단

**Player 충돌** 🏃
- 소리: `rampageCollisionPlayer`
- 효과: `rushDamage` 데미지
- 2초 쿨다운
- 돌진 중단

**쿠션 특수 처리** 🛡️
- 벽 충돌 무시
- 소리 없음
- HP 감소 없음
- 돌진 계속

#### 4. State 구조

**RampageIdleState** (대기)
- 초기 상태

**RampagePatrolState** (순찰)
- 순찰 경로 이동
- 플레이어 감지 시 → RampageChargeState

**RampageChargeState** (돌진)
- 3단계 코루틴:
  1. `RotateTowardsPlayerCoroutine()`: 플레이어 방향으로 회전 (1초)
  2. `MoveToPlayerCoroutine()`: NavMesh 추격 (attackRadius까지)
  3. `ChargePhysicsCoroutine()`: 물리 돌진 (충돌/시간 초과까지)
- 충돌 시 → RampagePanelOpenState
- 돌진 횟수 소진 시 → RampageIdleState

**RampagePanelOpenState** (패널 노출)
- 패널 열림 (공격 가능)
- 시간 경과 시 → RampageIdleState 또는 RampageChargeState

**RampageDisabledState** (무력화)
- 패널 체력 0 시
- 처치 가능 상태

### 디버그 UI (게임뷰)

**표시 정보**
```
━━━━━━━━━━━━━━━━━━━━━━━━━
HP        ████████░░ 40/50
Panel     ██████░░░░  4/6
Charge    ████████░░  2/3
State: RampageChargeState [COLLIDED]
━━━━━━━━━━━━━━━━━━━━━━━━━

🏃 이동 방향 (색상: 초록→빨강, 속도별)
   └─ "5.2 m/s"

🎯 돌진 시각화 (ChargeState일 때만)
   🟢 START (초록 원) - 돌진 시작 지점
   🔴 ╋ (빨간 십자가) - 고정 목표 지점
   🟡 ━━━ (노란 선) - 계획된 경로
   🔴 ━━━ (빨간 선) - 현재 진행 경로
   
   정보: "Total: 20.0m / Remain: 12.5m"
```

**오버라이드된 메서드**
```csharp
protected override void DrawDebugBars()
{
    // HP + Panel + Charge 바 표시
}

protected override void DrawStateText(float x, float y, float width)
{
    // State + [COLLIDED] 정보
}

protected override void DrawCustomDebugInfo()
{
    // 이동 방향 + 돌진 목표 시각화
}

protected override string GetEnemyDisplayName()
{
    return "Rampage";
}
```

**색상 시스템**
- HP: 초록 > 노랑 > 빨강
- Panel: 파랑 > 주황 > 빨강
- Charge: 빨강 → 청록 (선형)
- 이동 속도: 초록 (느림) → 빨강 (빠름)

### 게임플레이 피드백 시스템

#### 추적 → 돌진 전환 경고

**발동 시점**
- attackRadius (2m) 도달 시
- NavMesh 추격 → 물리 돌진 전환

**구현된 기능**
```csharp
public void TriggerChargeWarningFeedback()
{
    // FMOD 사운드 재생
    if (!string.IsNullOrEmpty(chargeStartSound.Path))
    {
        AudioManager.instance.PlayOneShot(chargeStartSound, transform, "Rampage: 추적 → 돌진 전환 경고음");
    }

    // TODO: 추가 피드백 구현
    // - VFX 파티클 생성
    // - 색상 변경
    // - 카메라 쉐이크
    // - 애니메이션 트리거
}
```

**리셋 기능**
```csharp
public void ResetChargeWarningFeedback()
{
    // TODO: 피드백 리셋 구현
    // - 색상 복구
    // - 코루틴 정지
    // - 파티클 정지
}
```

**Inspector 설정**
```
Gameplay Feedback (플레이어 피드백)
└─ Charge Start Sound: FMOD Event Reference
```

**구현 예시 (주석)**
```csharp
// VFX 추가:
// Instantiate(파티클프리팹, transform.position, Quaternion.identity);

// 색상 변경:
// GetComponent<Renderer>().material.color = 경고색;

// 애니메이션:
// animator.SetTrigger("ChargeWarning");

// 카메라 쉐이크:
// CameraShake.Instance?.Shake(강도, 지속시간);
```

### 게임플레이 파훼법

**1. 쿠션 활용 (가장 효율적)** ⭐
- 벽 앞에 쿠션 배치
- 돌진 유도
- 패널 3개 노출 (HP 무손실)

**2. 벽으로 유도 (기본)** 🧱
- 벽 근처로 유도
- 돌진 시 충돌
- 패널 1개 노출 + HP -10

**3. 패널 공격 (처치)** 🔨
- 패널 노출 시 집중 공격
- 패널 0 → Disabled
- 완전 처치

**4. 회피 (생존)** 🏃
- 2m 경고 범위 확인
- 고정된 돌진 경로 회피
- 옆으로 피하기

---

## 개발 타임라인

### 2025-10-13
- ✅ Moo 크리처 완전 구현
  - 기력 시스템
  - 시야/청각 감지
  - NavMesh 도망
  - 도망 중 슬라임 배출 (30% 확률)
  - 디버그 UI 완성

- ✅ Rampage 크리처 개선
  - 2단계 돌진 시스템 명확화
  - 돌진 목표 시각화 (고정 지점)
  - 벽 충돌 소리 제거
  - 게임플레이 피드백 시스템 추가
  - 디버그 UI 완성

- ✅ EnemyAIController 통합 디버그 UI
  - 모든 Enemy 공통 사용
  - 게임뷰 실시간 정보 표시
  - 범위 시각화 (Patrol/Flee)
  - 목표 지점 시각화
  - 오버라이드 가능한 구조

- ✅ Base Range Settings 개선
  - 퍼센트 기반 범위 설정
  - Min/Max 그라데이션 시각화
  - 게임뷰 표시 지원

### 이전 개발
- PlayerStatus 속성 추가 (IsCurrentRun, IsCurrentJump, IsCurrentWalk, IsMoving)
- BaseEnemyAIData attackRadius 제거 (Enemy별 개별 설정)
- creature-system.md 업데이트 (Moo 기획 반영)

---

## 참고 자료

### 관련 문서
- [크리처 시스템 기획서](../game-design/systems/creature-system.md)
- [Moo 개발 User Story](../NotionTask/개인 페이지 & 공유된 페이지/User Story DB/무우(Moo) 개발.md)
- [Rampage 개발 User Story](../NotionTask/개인 페이지 & 공유된 페이지/User Story DB/Rampage 개발.md)

### 주요 파일 위치
```
Assets/1.UnhappyCompany/Script/Enemy/
├─ 0.Base/
│  ├─ EnemyAIController.cs          # 공통 디버그 UI 시스템
│  └─ BaseEnemyAIData.cs            # 공통 데이터 (Range Settings)
├─ 3.Moo/
│  ├─ MooAIController.cs            # Moo 메인 컨트롤러
│  ├─ Data/MooAIData.cs             # Moo 데이터
│  └─ States/                       # Moo State 클래스들
│     ├─ MooWanderState.cs
│     ├─ MooFleeState.cs
│     ├─ MooExhaustedState.cs
│     ├─ MooSlimeEmitState.cs
│     └─ MooCenterAttackState.cs
└─ 3.Rampage/
   ├─ RampageAIController.cs        # Rampage 메인 컨트롤러
   ├─ RampageTrigger.cs             # 충돌 처리
   ├─ Data/RampageAIData.cs         # Rampage 데이터
   └─ States/                       # Rampage State 클래스들
      ├─ RampageIdleState.cs
      ├─ RampagePatrolState.cs
      ├─ RampageChargeState.cs
      ├─ RampagePanelOpenState.cs
      └─ RampageDisabledState.cs
```

---

## 개발 노트

### 설계 철학

**Moo: 초보자 친화적**
- 명확한 회피 방법 (걷기/원거리)
- 시각적 피드백 (감지 범위)
- 기력 시스템으로 예측 가능

**Rampage: 중급자용**
- 2단계 돌진으로 긴장감
- 명확한 경고 시스템
- 쿠션 활용으로 전략성

### 공통 디버그 철학
- 게임뷰에서 모든 정보 확인
- 개발자와 플레이어 모두 혜택
- 쉬운 밸런싱과 QA

### 확장 가능성
- 새 Enemy는 EnemyAIController 상속
- DrawCustomDebugInfo() 오버라이드로 특수 정보 추가
- 게임플레이 피드백 시스템 활용 (Rampage 예시 참고)

---

## RSP 크리처

### 개요
- **타입**: 기계형 💧
- **컨셉**: 가위바위보 미니게임 + 스택 시스템
- **주요 메커니즘**: 시간 압박 + 연속 게임 강제 + 쿨다운

### 핵심 시스템

#### 1. 스택 시스템

**스택 증가**
- 플레이어 발견 후 30초마다 자동 +1
- 최대 스택: 4

**스택 감소**
- 가위바위보 게임 승리 시 -1
- 스택 0 도달 시 → 2분 쿨다운

#### 2. 가위바위보 게임

**게임 시작 조건**
- F키 상호작용
- 스택 > 0일 때만 가능
- 쿨다운 중에는 불가능

**게임 진행**
- 스택이 0이 될 때까지 연속 진행 강제
- 1회 게임 종료 시 자동으로 다음 게임 시작
- 중간에 이탈 불가능

**게임 종료**
- 스택 0 도달 → 2분 쿨다운
- 쿨다운 중에는 순찰만

#### 3. State 구조

**RSPPatrolState** (순찰)
- 순찰 경로 이동
- 30초마다 스택 증가

**RSPHoldingState** (게임 진행)
- 가위바위보 게임 중
- 움직임 정지
- 스택 소진까지 강제

**RSPRageState** (광란)
- 스택 4 도달 시
- 공격적 행동

**RSPCenterAttackState** (센터 공격)
- 오후 페이즈 전용
- 센터로 이동 후 공격

### 디버그 UI (게임뷰)

**표시 정보**
```
━━━━━━━━━━━━━━━━━━━━━━━━━
HP        ████████░░ 40/50
Stack     ████████░░  3/4
State: RSPPatrolState [COOLDOWN]
Interaction Available
━━━━━━━━━━━━━━━━━━━━━━━━━
```

**오버라이드된 메서드**
```csharp
protected override void DrawDebugBars()
{
    // HP + Stack 바 표시
}

protected override void DrawStateText(float x, float y, float width)
{
    // State + [COOLDOWN] + [AIRBORNE] 정보
    // 스택 0일 때 "Interaction Available" 표시
}

protected override string GetEnemyDisplayName()
{
    return "RSP";
}
```

**색상 시스템**
- HP: 초록 > 노랑 > 빨강
- Stack: 
  - 0 (안전): 초록
  - 1~2 (주의): 청록
  - 3 (경고): 노랑
  - 4 (위험): 빨강

### 게임플레이 파훼법

**1. 즉시 대응 (가장 효율적)** ⭐
- 스택 발견 즉시 게임 시작
- 스택 적을 때 빠른 해결
- 시간 압박 최소화

**2. 방치 → 광란 (위험)** 🔴
- 스택 4 도달 허용
- 광란 상태 진입
- 처리 난이도 상승

---

## 공통 개발 사항

### 디버그 시스템 구조

**상속 구조**
```
EnemyAIController (Base)
├─ 공통 디버그 UI 메서드
├─ 범위 시각화
└─ Virtual 메서드 제공

MooAIController : EnemyAIController<MooAIData>
├─ DrawDebugBars() override → HP + Stamina
├─ DrawStateText() override → Detection
└─ DrawCustomDebugInfo() override → 감지 범위

RampageAIController : EnemyAIController<RampageAIData>
├─ DrawDebugBars() override → HP + Panel + Charge
├─ DrawStateText() override → Collided
└─ DrawCustomDebugInfo() override → 이동/돌진

EnemyAIRSP : EnemyAIController<RSPEnemyAIData>
├─ DrawDebugBars() override → HP + Stack
├─ DrawStateText() override → Cooldown + Airborne
└─ GetEnemyDisplayName() override → "RSP"
```

### 게임뷰 디버그 활성화 방법

**Unity Inspector**
```
1. Enemy 오브젝트 선택
2. [Enemy]AIController 컴포넌트
3. Debug UI Settings 섹션
   └─ Is Show Debug ✅
   └─ Debug UI Scale: 1.4 (조절 가능)
   
4. Base Range Settings 섹션
   └─ Show Ranges In Game ✅ (범위 보기)
```

### 범위 설정 시스템

**퍼센트 기반**
```
Patrol Distance Min Ratio: 0.5  (50% of patrolRadius)
Patrol Distance Max Ratio: 1.0  (100% of patrolRadius)
Flee Distance Min Ratio: 0.8    (80% of patrolRadius)
Flee Distance Max Ratio: 1.5    (150% of patrolRadius)
```

**시각화 색상**
- Min: 옅은 색 (알파 0.2)
- Max: 진한 색 (알파 0.5)
- 그라데이션으로 범위 명확하게 표시

---

## 개발 완료 체크리스트

### Moo 크리처
- [x] NavMesh 기반 이동
- [x] 기력 시스템 (소모/회복/지침)
- [x] 시야 감지 (120°, 5m)
- [x] 청각 감지 (5m, 달리기)
- [x] 3단계 도망 로직 (Direct/Adjusted/Far Random)
- [x] 도망 중 슬라임 배출 (30% 확률)
- [x] 게임뷰 디버그 UI
- [x] 감지 범위 시각화
- [x] 도망 목표 시각화
- [ ] 점액 디버프 밸런싱

### Rampage 크리처
- [x] 2단계 돌진 (NavMesh → 물리)
- [x] 패널 시스템 (쿠션/벽 충돌)
- [x] 충돌 처리 (Pushable/Wall/Player)
- [x] 게임플레이 피드백 시스템 (FMOD)
- [x] 게임뷰 디버그 UI
- [x] 이동 방향 시각화
- [x] 돌진 목표 시각화 (고정 지점)
- [x] 벽 충돌 소리 제거
- [ ] 피드백 구체화 (VFX/색상)

### RSP 크리처
- [x] 스택 시스템
- [x] 가위바위보 게임
- [x] 쿨다운 시스템
- [x] 게임뷰 디버그 UI
- [x] 스택 바 시각화
- [x] 상호작용 알림
- [ ] 게임 UI 개선

### 공통 시스템
- [x] EnemyAIController 디버그 UI 통합
- [x] Base Range Settings 개선
- [x] 퍼센트 기반 범위 설정
- [x] 게임뷰 범위 시각화
- [x] 목표 지점 시각화 시스템
- [x] 헬퍼 메서드 제공
- [ ] 다른 Enemy 디버그 UI 적용

---

**문서 작성**: AI Assistant
**검수 필요**: 실제 게임 테스트 데이터 반영


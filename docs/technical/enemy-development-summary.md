# Enemy AI 개발 요약 (2025-10-13)

> **빠른 참조용 요약 문서**
> 상세 내용은 [enemy-ai-system.md](./enemy-ai-system.md) 참고

---

## 🎯 주요 개발 성과

### ✅ 공통 시스템 (EnemyAIController)
**통합 게임뷰 디버그 UI 시스템**
- 모든 Enemy가 상속받아 사용
- HP 바, 상태 텍스트, 범위 시각화
- Virtual 메서드로 확장 가능
- 게임뷰에서 실시간 확인 (디버깅 + 개발 편의)

### ✅ Moo 크리처 (노말형 🔨)
**기력 기반 도망 시스템**
- 시야/청각 듀얼 감지
- 플레이어 반대 방향 도망 (3단계 로직)
- 기력 소진 시 지침 상태
- 도망 중 슬라임 배출 (30%)

**디버그 UI**
- HP + Stamina 바
- 시야(120°) + 청각(5m) 범위 시각화
- 도망 목표 지점 시각화

### ✅ Rampage 크리처 (기계형 💧)
**2단계 돌진 시스템**
- NavMesh 추격 → 물리 돌진 (attackRadius: 2m)
- 고정 방향 돌진 (예측 가능)
- 게임플레이 피드백 (FMOD 경고음)

**디버그 UI**
- HP + Panel + Charge 바
- 이동 방향 시각화 (속도별 색상)
- 돌진 목표 시각화 (시작점 + 고정 목표)

### ✅ RSP 크리처 (기계형 💧)
**스택 + 가위바위보 시스템**
- 30초마다 스택 증가
- 연속 게임 강제
- 2분 쿨다운

**디버그 UI**
- HP + Stack 바
- 쿨다운 상태 표시
- 상호작용 가능 알림

---

## 📊 Enemy 비교표

| Enemy | 타입 | 주요 메커니즘 | 디버그 바 | 특수 시각화 |
|-------|------|--------------|----------|------------|
| **Moo** | 노말형 🔨 | 기력 도망 | HP + Stamina | 감지범위 + 도망목표 |
| **Rampage** | 기계형 💧 | 2단계 돌진 | HP + Panel + Charge | 이동방향 + 돌진목표 |
| **RSP** | 기계형 💧 | 스택 게임 | HP + Stack | 상호작용 알림 |

---

## 🎮 디버그 UI 사용법

### Unity Inspector 설정
```
1. Enemy 오브젝트 선택
2. [Enemy]AIController 컴포넌트
3. "Is Show Debug" ✅ 체크
4. "Debug UI Scale" 조정 (기본: 1.4)
5. "Show Ranges In Game" ✅ (범위 보기)
```

### 각 Enemy별 표시 정보

**Moo**
- HP (초록→노랑→빨강)
- Stamina (초록→노랑→빨강)
- State + [EXHAUSTED]
- Detection: Visual/Sound
- 감지 범위 (시야각 + 청각)
- 도망 목표 지점

**Rampage**
- HP (초록→노랑→빨강)
- Panel (파랑→주황→빨강)
- Charge (빨강→청록)
- State + [COLLIDED]
- 이동 방향 (속도별 색상)
- 돌진 목표 (시작점 + 고정 목표)

**RSP**
- HP (초록→노랑→빨강)
- Stack (초록→청록→노랑→빨강)
- State + [COOLDOWN] + [AIRBORNE]
- Interaction Available (스택 0일 때)

---

## 🔧 확장 가이드

### 새 Enemy 추가 시

**1. AIController 상속**
```csharp
public class NewEnemyAI : EnemyAIController<NewEnemyData>
{
    // 자동으로 기본 디버그 UI 제공됨
}
```

**2. 디버그 UI 커스터마이즈**
```csharp
protected override void DrawDebugBars()
{
    // 기본 HP 바
    base.DrawDebugBars();
    
    // 추가 바 (예: Energy, Shield 등)
    DrawDebugBar(x, y, width, height, "Energy", current, max, percent, color);
}

protected override void DrawCustomDebugInfo()
{
    // Enemy만의 특수 정보
    // 예: 감지 범위, 공격 범위, 특수 상태 등
}

protected override string GetEnemyDisplayName()
{
    return "NewEnemy";
}
```

### 게임플레이 피드백 추가 (Rampage 예시)

**1. 피드백 메서드 작성**
```csharp
public void TriggerSomeFeedback()
{
    // FMOD 사운드
    if (!string.IsNullOrEmpty(someFMODEvent.Path))
    {
        AudioManager.instance.PlayOneShot(someFMODEvent, transform, "설명");
    }

    // TODO: VFX, 색상, 애니메이션 등
}

public void ResetSomeFeedback()
{
    // TODO: 정리 작업
}
```

**2. 적절한 시점에 호출**
```csharp
// State의 Enter() 또는 특정 조건에서:
controller.TriggerSomeFeedback();

// State의 Exit()에서:
controller.ResetSomeFeedback();
```

---

## 📁 주요 파일 위치

### Base (공통)
```
Assets/1.UnhappyCompany/Script/Enemy/0.Base/
├─ EnemyAIController.cs       # 디버그 UI 시스템
└─ BaseEnemyAIData.cs          # Range Settings
```

### Moo
```
Assets/1.UnhappyCompany/Script/Enemy/3.Moo/
├─ MooAIController.cs          # 기력 + 감지 시스템
├─ Data/MooAIData.cs
└─ States/
   ├─ MooWanderState.cs        # 배회 + 감지
   ├─ MooFleeState.cs          # 도망 + 슬라임 배출
   ├─ MooExhaustedState.cs     # 지침 상태
   ├─ MooSlimeEmitState.cs     # 슬라임 배출
   └─ MooCenterAttackState.cs  # 센터 공격
```

### Rampage
```
Assets/1.UnhappyCompany/Script/Enemy/3.Rampage/
├─ RampageAIController.cs      # 돌진 + 패널 + 피드백
├─ RampageTrigger.cs           # 충돌 처리
├─ Data/RampageAIData.cs
└─ States/
   ├─ RampageIdleState.cs
   ├─ RampagePatrolState.cs
   ├─ RampageChargeState.cs    # 2단계 돌진
   ├─ RampagePanelOpenState.cs
   └─ RampageDisabledState.cs
```

### RSP
```
Assets/1.UnhappyCompany/Script/Enemy/2.RSP/
├─ EnemyAIRSP.cs               # 스택 + 게임 시스템
├─ RSPSystem.cs
├─ Data/RSPEnemyAIData.cs
└─ States/
   ├─ RSPPatrolState.cs
   ├─ RSPHoldingState.cs       # 게임 진행
   ├─ RSPRageState.cs
   └─ RSPCenterAttackState.cs
```

---

## 🎨 디버그 UI 색상 가이드

### HP (공통)
- 🟢 60% 이상: Green
- 🟡 30~60%: Yellow
- 🔴 30% 미만: Red

### 특수 바
**Moo - Stamina**
- 50% 이상: Yellow → Green
- 25~50%: Red → Yellow
- 25% 미만: Red

**Rampage - Panel**
- 60% 이상: Blue
- 30~60%: Orange
- 30% 미만: Red

**Rampage - Charge**
- 선형: Red → Cyan

**RSP - Stack**
- 0: Green (안전)
- 1~2: Cyan (주의)
- 3: Yellow (경고)
- 4: Red (위험)

---

## 🚀 다음 개발 계획

### 즉시 진행
- [ ] Rampage 피드백 구체화 (VFX, 색상)
- [ ] Moo 점액 디버프 밸런싱
- [ ] 3크리처 통합 테스트

### 단기 목표
- [ ] 오후 페이즈 밸런싱
- [ ] 크리처별 난이도 조정
- [ ] 자원 관리 밸런싱

### 중기 목표
- [ ] 4번째 크리처 (인간형 🔥) 기획
- [ ] 크리처 학습 시스템
- [ ] 동적 난이도 조절

---

**작성 일자**: 2025-10-13
**작성자**: AI Assistant
**검수 필요**: 실제 플레이 테스트 후 수치 조정



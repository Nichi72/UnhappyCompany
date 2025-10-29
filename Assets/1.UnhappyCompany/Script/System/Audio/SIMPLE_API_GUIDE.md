# AudioManager 간단 API 가이드 (최종 단순화)

## 🎯 4개 함수로 모든 것 해결!

복잡한 오버로드는 다 제거했습니다. 이제 **4개 함수**만 알면 됩니다!

---

## 📋 전체 함수 목록

1. `PlayUISound` - UI 사운드 (2D)
2. `Play3DSoundAtPosition` - 위치 기반 (한 번)
3. `Play3DSoundByTransform` - Transform 추적 (한 번)
4. `PlayLoopSoundByTransform` - Transform 추적 (루프)

---

## 1️⃣ PlayUISound

**UI 사운드 (2D)**

```csharp
AudioManager.instance.PlayUISound(FMODEvents.instance.computerCursorClick);
```

---

## 2️⃣ Play3DSoundAtPosition

**위치 기반 3D 사운드 (한 번 재생)**

### 기본 (FMOD 기본 거리)
```csharp
AudioManager.instance.Play3DSoundAtPosition(
    FMODEvents.instance.rampageExplode,
    explosionPosition
);
```

### MaxDistance 설정
```csharp
AudioManager.instance.Play3DSoundAtPosition(
    FMODEvents.instance.rampageExplode,
    explosionPosition,
    100f   // maxDistance = 100m
);
```

---

## 3️⃣ Play3DSoundByTransform

**Transform 추적 3D 사운드 (한 번 재생)**

### 기본 (FMOD 기본 거리)
```csharp
AudioManager.instance.Play3DSoundByTransform(
    FMODEvents.instance.damage,
    player.transform
);
```

### MaxDistance 설정
```csharp
AudioManager.instance.Play3DSoundByTransform(
    FMODEvents.instance.cushionImpact,
    enemy.transform,
    40f   // maxDistance = 40m
);
```

---

## 4️⃣ PlayLoopSoundByTransform

**루프 사운드 (Transform 추적, 수동 정지 필요)**

### 기본 (FMOD 기본 거리)
```csharp
// 시작
PooledEmitter emitter = AudioManager.instance.PlayLoopSoundByTransform(
    FMODEvents.instance.rampageIdle,
    vehicle.transform
);

// 정지
AudioManager.instance.StopEmitter(emitter);
```

### MaxDistance 설정
```csharp
PooledEmitter emitter = AudioManager.instance.PlayLoopSoundByTransform(
    FMODEvents.instance.rampageMoveLoop,
    vehicle.transform,
    40f   // maxDistance = 40m
);
```

---

## 💡 파라미터 규칙

모든 함수에서 **동일한 규칙** 적용:

```csharp
float maxDistance = -1f  // -1 = FMOD 기본값, 0보다 크면 = 해당 값 사용
string debugName = null  // Scene 뷰에 표시될 이름
```

**참고**: MinDistance는 항상 0으로 고정됩니다 (소스 위치부터 최대 볼륨)

### 예시
```csharp
// 기본값 사용 (FMOD Studio 설정)
Play3DSoundByTransform(eventRef, transform);

// MaxDistance 설정
Play3DSoundByTransform(eventRef, transform, 50f);

// Debug 이름 추가
Play3DSoundByTransform(eventRef, transform, 50f, "My Sound");
```

---

## 🔧 RampageAIController 예시

```csharp
// MoveLoop 사운드 (MaxDistance 40m)
moveLoopEmitter = AudioManager.instance.PlayLoopSoundByTransform(
    FMODEvents.instance.rampageMoveLoop,
    transform,
    40f,  // maxDistance = 40m
    "Rampage MoveLoop"
);

// 정지
AudioManager.instance.StopEmitter(moveLoopEmitter);
```

---

## 📊 언제 뭘 쓸까?

| 상황 | 함수 | 예시 |
|------|------|------|
| UI 버튼 | `PlayUISound` | 클릭 소리 |
| 폭발 (고정 위치) | `Play3DSoundAtPosition` | 벽 파괴 |
| 발소리 (캐릭터) | `Play3DSoundByTransform` | 플레이어 발소리 |
| 엔진음 (지속) | `PlayLoopSoundByTransform` | 차량 엔진 |

---

## ⚠️ 주의사항

### Distance 파라미터

- **minDistance**: 항상 0으로 고정 (소스 위치부터 최대 볼륨)
- **maxDistance**: 
  - **-1** = FMOD 기본값 사용 (권장)
  - **0보다 큰 값** = 해당 거리까지 사운드 감쇠

### 파라미터 순서

```csharp
Play3DSoundByTransform(
    eventRef,      // EventReference (필수)
    transform,     // Transform (필수)
    maxDistance,   // float (선택, 기본값 -1f)
    debugName      // string (선택, 기본값 null)
)
```

---

## ✅ Before vs After

### Before (복잡함 💀)
```csharp
// 오버로드 10개...
Play3DSoundFollowing()
Play3DSoundFollowingWithDistance()
Play3DSoundAtPosition()
Play3DSoundAtPositionWithDistance()
PlayLoopSound()
PlayLoopSoundWithDistance()
// 뭐가 뭔지 모르겠음...
```

### After (단순함 ✅)
```csharp
// 함수 4개만!
PlayUISound()
Play3DSoundAtPosition()
Play3DSoundByTransform()
PlayLoopSoundByTransform()

// Distance는 파라미터로 해결!
Play3DSoundByTransform(eventRef, transform, 40f)  // 간단!
```

---

## 🚀 실전 활용

### 충돌음
```csharp
AudioManager.instance.Play3DSoundAtPosition(
    FMODEvents.instance.rampageCollisionPlayer,
    hitPoint,
    30f   // 30미터까지 들림
);
```

### 적 발소리
```csharp
AudioManager.instance.Play3DSoundByTransform(
    FMODEvents.instance.damage,
    enemy.transform,
    15f   // 15미터까지만
);
```

### 차량 엔진음
```csharp
engineEmitter = AudioManager.instance.PlayLoopSoundByTransform(
    FMODEvents.instance.rampageIdle,
    vehicle.transform,
    50f   // maxDistance = 50m
);
```

---

## 🎨 Scene 뷰 시각화

- 🟢 **녹색 구** - Emitter 위치
- 🟢 **녹색 원** - Min Distance
- 🔴 **빨간색 원** - Max Distance
- **라벨** - Debug 이름, 재생 시간, Min/Max 값

---

**작성일**: 2025-10-29  
**버전**: 3.0 (최종 단순화)  
**업데이트**: 오버로드 제거, 4개 함수로 단순화


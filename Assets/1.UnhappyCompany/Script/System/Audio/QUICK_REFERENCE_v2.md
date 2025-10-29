# AudioManager 빠른 참조 v2 (명확한 함수명)

## 🎵 사운드 재생 치트시트

### 1️⃣ UI 사운드 (2D)
```csharp
AudioManager.instance.PlayUISound(FMODEvents.instance.computerCursorClick);
```

---

## 📍 위치 기반 3D 사운드 (Position)

### 기본 (FMOD 기본 거리)
```csharp
AudioManager.instance.Play3DSoundAtPosition(
    FMODEvents.instance.rampageCollisionPlayer,
    hitPosition
);
```

### MaxDistance 설정
```csharp
AudioManager.instance.Play3DSoundAtPositionWithDistance(
    FMODEvents.instance.rampageExplode,
    explosionPosition,
    100f  // MaxDistance = 100m
);
```

### Min + Max 설정
```csharp
AudioManager.instance.Play3DSoundAtPositionWithDistance(
    FMODEvents.instance.rampageExplode,
    explosionPosition,
    10f,   // MinDistance = 10m
    100f   // MaxDistance = 100m
);
```

---

## 🎯 Transform 추적 3D 사운드 (Following)

### 기본 (FMOD 기본 거리)
```csharp
AudioManager.instance.Play3DSoundFollowing(
    FMODEvents.instance.damage,
    player.transform
);
```

### MaxDistance 설정
```csharp
AudioManager.instance.Play3DSoundFollowingWithDistance(
    FMODEvents.instance.cushionImpact,
    enemy.transform,
    40f  // MaxDistance = 40m
);
```

### Min + Max 설정
```csharp
AudioManager.instance.Play3DSoundFollowingWithDistance(
    FMODEvents.instance.rampageIdle,
    vehicle.transform,
    5f,   // MinDistance = 5m
    40f   // MaxDistance = 40m
);
```

---

## 🔄 루프 사운드 (Loop)

### 기본 (FMOD 기본 거리)
```csharp
// 시작
PooledEmitter emitter = AudioManager.instance.PlayLoopSound(
    FMODEvents.instance.rampageIdle,
    vehicle.transform
);

// 정지
AudioManager.instance.StopEmitter(emitter);
```

### MaxDistance 설정
```csharp
PooledEmitter emitter = AudioManager.instance.PlayLoopSoundWithDistance(
    FMODEvents.instance.rampageMoveLoop,
    vehicle.transform,
    50f  // MaxDistance = 50m
);
```

### Min + Max 설정
```csharp
PooledEmitter emitter = AudioManager.instance.PlayLoopSoundWithDistance(
    FMODEvents.instance.rampageIdle,
    vehicle.transform,
    3f,   // MinDistance = 3m
    30f   // MaxDistance = 30m
);
```

---

## 📋 함수명 정리

| 목적 | 함수명 | 오버로드 |
|------|--------|----------|
| **UI 사운드** | `PlayUISound` | - |
| **위치 기반** | `Play3DSoundAtPosition` | 기본 |
| | `Play3DSoundAtPositionWithDistance` | Max만 |
| | `Play3DSoundAtPositionWithDistance` | Min+Max |
| **Transform 추적** | `Play3DSoundFollowing` | 기본 |
| | `Play3DSoundFollowingWithDistance` | Max만 |
| | `Play3DSoundFollowingWithDistance` | Min+Max |
| **루프** | `PlayLoopSound` | 기본 |
| | `PlayLoopSoundWithDistance` | Max만 |
| | `PlayLoopSoundWithDistance` | Min+Max |

---

## 💡 언제 뭘 쓸까?

| 상황 | 사용 함수 |
|------|----------|
| UI 버튼 클릭 | `PlayUISound` |
| 충돌음 (고정 위치) | `Play3DSoundAtPosition` |
| 발소리 (캐릭터 추적) | `Play3DSoundFollowing` |
| 폭발음 (넓은 범위) | `Play3DSoundAtPositionWithDistance` (Max=100) |
| 엔진음 (지속) | `PlayLoopSoundWithDistance` |

---

## 🔧 실전 예시

### RampageAIController 적용
```csharp
// MoveLoop 사운드 (MaxDistance 40m)
moveLoopEmitter = AudioManager.instance.PlayLoopSoundWithDistance(
    FMODEvents.instance.rampageMoveLoop,
    transform,
    40f,
    "Rampage MoveLoop"
);
```

### 폭발 효과
```csharp
// 100미터까지 들리는 폭발
AudioManager.instance.Play3DSoundAtPositionWithDistance(
    FMODEvents.instance.rampageExplode,
    explosionPos,
    100f
);
```

### 플레이어 발소리
```csharp
// 15미터까지만 들리는 발소리
AudioManager.instance.Play3DSoundFollowingWithDistance(
    FMODEvents.instance.damage,
    player.transform,
    15f
);
```

---

## ⚠️ 주의사항

1. **Distance가 필요 없으면 기본 함수 사용**
   - `Play3DSoundAtPosition` (거리 설정 없음)
   - FMOD Studio의 기본값 사용

2. **Distance가 필요하면 WithDistance 함수 사용**
   - `Play3DSoundAtPositionWithDistance` (거리 설정)
   - 명확하게 구분됨!

3. **WithDistance 함수는 항상 Override Attenuation = true**
   - MinDistance, MaxDistance가 자동 적용됨

---

## 🎨 Scene 뷰 확인

1. Play 모드 실행
2. 함수 호출
3. Scene 뷰에서 **녹색 Gizmo** 확인
4. GameObject 선택 → Inspector 확인
   - `Override Attenuation` ✓
   - `Override Max Distance` 값 확인

---

## 🚀 마이그레이션

### Before (혼란스러운 오버로드)
```csharp
// 이게 Max인지 Min인지 헷갈림
Play3DSound(eventRef, transform, 40f)
```

### After (명확한 함수명)
```csharp
// 명확함!
Play3DSoundFollowingWithDistance(eventRef, transform, 40f)
```

---

**작성일**: 2025-10-29  
**버전**: 2.0  
**업데이트**: 명확한 함수명으로 재설계


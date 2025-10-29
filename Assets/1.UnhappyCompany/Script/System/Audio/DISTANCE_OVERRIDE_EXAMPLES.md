# MaxDistance 설정 예시 가이드

## 🎯 개요

이제 `Play3DSound`와 `PlayLoopSound`에서 **MaxDistance**와 **MinDistance**를 간편하게 설정할 수 있습니다!

---

## 📖 사용 방법

### 1️⃣ **기본 사용 (FMOD 기본값)**

```csharp
// MaxDistance는 FMOD Studio에 설정된 값 사용
AudioManager.instance.Play3DSound(
    FMODEvents.instance.rampageCollisionPlayer,
    transform.position
);
```

### 2️⃣ **MaxDistance만 설정**

```csharp
// MaxDistance를 50미터로 설정
AudioManager.instance.Play3DSound(
    FMODEvents.instance.rampageCollisionPlayer,
    transform.position,
    50f,  // MaxDistance
    "Collision Sound"
);
```

### 3️⃣ **MinDistance와 MaxDistance 모두 설정**

```csharp
// MinDistance 5미터, MaxDistance 50미터
AudioManager.instance.Play3DSound(
    FMODEvents.instance.rampageCollisionPlayer,
    transform.position,
    5f,   // MinDistance (100% 볼륨)
    50f,  // MaxDistance (0% 볼륨)
    "Collision Sound"
);
```

---

## 🎮 모든 오버로드

### Play3DSound (위치 기반)

```csharp
// 1. 기본
Play3DSound(EventReference, Vector3)

// 2. MaxDistance 설정
Play3DSound(EventReference, Vector3, float maxDistance)

// 3. Min/Max 설정
Play3DSound(EventReference, Vector3, float minDistance, float maxDistance)

// 4. 디버그명 추가
Play3DSound(EventReference, Vector3, string debugName)
Play3DSound(EventReference, Vector3, float maxDistance, string debugName)
Play3DSound(EventReference, Vector3, float minDistance, float maxDistance, string debugName)
```

### Play3DSound (Transform 추적)

```csharp
// 1. 기본
Play3DSound(EventReference, Transform)

// 2. MaxDistance 설정
Play3DSound(EventReference, Transform, float maxDistance)

// 3. Min/Max 설정
Play3DSound(EventReference, Transform, float minDistance, float maxDistance)

// 4. 디버그명 추가
Play3DSound(EventReference, Transform, string debugName)
Play3DSound(EventReference, Transform, float maxDistance, string debugName)
Play3DSound(EventReference, Transform, float minDistance, float maxDistance, string debugName)
```

### PlayLoopSound (루프 사운드)

```csharp
// 1. 기본
PlayLoopSound(EventReference, Transform)

// 2. MaxDistance 설정
PlayLoopSound(EventReference, Transform, float maxDistance)

// 3. Min/Max 설정
PlayLoopSound(EventReference, Transform, float minDistance, float maxDistance)

// 4. 디버그명 추가
PlayLoopSound(EventReference, Transform, string debugName)
PlayLoopSound(EventReference, Transform, float maxDistance, string debugName)
PlayLoopSound(EventReference, Transform, float minDistance, float maxDistance, string debugName)
```

---

## 💡 실전 예시

### 예시 1: 폭발 소리 (넓은 범위)

```csharp
// 폭발은 100미터까지 들려야 함
AudioManager.instance.Play3DSound(
    FMODEvents.instance.rampageExplode,
    explosionPosition,
    100f,  // MaxDistance = 100m
    "Rampage Explosion"
);
```

### 예시 2: 발소리 (좁은 범위)

```csharp
// 발소리는 15미터까지만
AudioManager.instance.Play3DSound(
    FMODEvents.instance.damage,
    player.transform,
    15f,  // MaxDistance = 15m
    "Player Footstep"
);
```

### 예시 3: 엔진음 (정밀 제어)

```csharp
// 가까이서는 100% 볼륨, 멀리서 서서히 감소
AudioManager.instance.PlayLoopSound(
    FMODEvents.instance.rampageIdle,
    vehicle.transform,
    3f,   // MinDistance = 3m (100% 볼륨 유지)
    30f,  // MaxDistance = 30m (완전히 사라짐)
    "Vehicle Engine"
);
```

### 예시 4: 거대 보스 울음소리 (매우 넓은 범위)

```csharp
// 맵 전체에 들리는 보스 사운드
AudioManager.instance.Play3DSound(
    FMODEvents.instance.rampageBreakWarning,
    bossPosition,
    10f,   // MinDistance = 10m
    200f,  // MaxDistance = 200m
    "Boss Roar"
);
```

### 예시 5: UI 버튼 (거리 무관, 2D)

```csharp
// UI 사운드는 거리 설정 불필요
AudioManager.instance.PlayUISound(
    FMODEvents.instance.computerCursorClick,
    "Button Click"
);
```

---

## 📊 Distance 값 가이드

| 사운드 종류 | 권장 MinDistance | 권장 MaxDistance | 이유 |
|------------|-----------------|------------------|------|
| 발소리 | 0~1m | 10~15m | 가까운 거리만 |
| 충돌음 | 1~2m | 20~30m | 중간 거리 |
| 폭발음 | 5~10m | 50~100m | 넓은 범위 |
| 엔진음 | 3~5m | 30~50m | 지속적 사운드 |
| 보스 울음 | 10~20m | 100~200m | 맵 전체 |
| UI 사운드 | - | - | 거리 무관 (2D) |

---

## ⚙️ MinDistance vs MaxDistance

### MinDistance (최소 거리)
- 이 거리 **이내**에서는 **100% 볼륨 유지**
- 사운드가 너무 가까워도 귀를 찢지 않게 함
- 기본값: 보통 1m

### MaxDistance (최대 거리)
- 이 거리에서 **완전히 들리지 않음 (0% 볼륨)**
- 성능 최적화: 멀리 있는 사운드는 재생 안 함
- 기본값: 사운드마다 다름 (FMOD Studio 설정)

### 감쇠 곡선
```
볼륨 100% |═════════╗
          |          ╚════╗
          |               ╚════╗
          |                    ╚════╗
    0%    |                         ╚═══════
          0m    MinDistance    MaxDistance
```

---

## 🎨 Scene 뷰에서 확인

Distance 오버라이드를 사용하면:
1. **Play 모드 실행**
2. **Scene 뷰에서 Emitter 선택**
3. **Inspector에 Override Attenuation ✓ 보임**
4. **Min/Max Distance 값 확인**
5. **Scene 뷰에 원으로 표시** (FMOD Gizmo)

---

## 🔧 RampageAIController 적용 예시

```csharp
// 기존 코드
moveLoopEmitter = AudioManager.instance.PlayLoopSound(
    FMODEvents.instance.rampageMoveLoop,
    transform,
    "Rampage MoveLoop"
);

// MaxDistance 추가
moveLoopEmitter = AudioManager.instance.PlayLoopSound(
    FMODEvents.instance.rampageMoveLoop,
    transform,
    40f,  // MaxDistance = 40m
    "Rampage MoveLoop"
);

// Min/Max 둘 다 설정
moveLoopEmitter = AudioManager.instance.PlayLoopSound(
    FMODEvents.instance.rampageMoveLoop,
    transform,
    5f,   // MinDistance = 5m
    40f,  // MaxDistance = 40m
    "Rampage MoveLoop"
);
```

---

## ⚠️ 주의사항

1. **MinDistance는 MaxDistance보다 작아야 함**
   ```csharp
   // ❌ 잘못된 예
   Play3DSound(eventRef, pos, 50f, 10f);  // Min > Max!
   
   // ✅ 올바른 예
   Play3DSound(eventRef, pos, 10f, 50f);  // Min < Max
   ```

2. **거리 단위는 미터(m)**
   - Unity 기본 단위 = 미터
   - 1 Unit = 1 미터

3. **2D 사운드는 거리 설정 불필요**
   - UI 사운드 → `PlayUISound` 사용

4. **FMOD Studio 기본값 우선**
   - 특별한 이유 없으면 FMOD Studio 설정 사용
   - 필요할 때만 오버라이드

---

## 🚀 성능 팁

- **MaxDistance를 적절히 설정**하면 멀리 있는 사운드를 재생 안 해서 **CPU 절약**
- 중요하지 않은 사운드는 **MaxDistance를 작게**
- 중요한 사운드는 **MaxDistance를 크게**

---

**작성일**: 2025-10-29  
**버전**: 1.1  
**업데이트**: Distance 오버라이드 추가


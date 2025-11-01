# AudioManager 빠른 참조 (Quick Reference)

## 🎵 사운드 재생 치트시트

### 1️⃣ UI 사운드 (2D)
```csharp
AudioManager.instance.PlayUISound(FMODEvents.instance.computerCursorClick);
```

### 2️⃣ 효과음 (3D, 한 번만)
```csharp
AudioManager.instance.Play3DSound(
    FMODEvents.instance.rampageCollisionPlayer,
    transform.position
);
```

### 3️⃣ 효과음 (3D, 오브젝트 추적)
```csharp
AudioManager.instance.Play3DSound(
    FMODEvents.instance.damage,
    player.transform
);
```

### 4️⃣ 루프 사운드 (시작)
```csharp
PooledEmitter emitter = AudioManager.instance.PlayLoopSound(
    FMODEvents.instance.rampageIdle,
    vehicle.transform
);
```

### 5️⃣ 루프 사운드 (정지)
```csharp
AudioManager.instance.StopEmitter(emitter);
```

### 6️⃣ 고급 옵션
```csharp
var options = new EmitterPlayOptions
{
    followTarget = transform,
    volume = 0.5f,
    maxDistance = 50f,
    lifetime = 10f
};
AudioManager.instance.PlayWithEmitter(FMODEvents.instance.TEST, options);
```

---

## 📋 언제 어떤 메서드를 쓸까?

| 상황 | 사용 메서드 | 예시 |
|------|------------|------|
| UI 버튼 클릭 | `PlayUISound` | 메뉴 사운드 |
| 충돌음 (위치 고정) | `Play3DSound(pos)` | 벽 충돌 |
| 발소리 (캐릭터 추적) | `Play3DSound(transform)` | 적 발소리 |
| 엔진음 (계속 재생) | `PlayLoopSound` | 차량 소리 |
| 복잡한 설정 필요 | `PlayWithEmitter(options)` | 파라미터 많은 사운드 |

---

## 🎨 Scene 뷰 디버깅

### 색상 의미
- 🔵 **청록색** = OneShot 사운드
- 🟡 **노란색** = 루프 사운드 (코루틴)
- 🟢 **녹색** = Emitter Pool 사운드

### 활성화 방법
1. AudioManager GameObject 선택
2. Inspector → `Show Sound Debug` ✓
3. Scene 뷰에서 확인

---

## ⚡ 성능 팁

### ✅ 해야 할 것
- UI 사운드 → `PlayUISound` 사용
- 3D 사운드 → `Play3DSound` 사용
- 루프 사운드 → `PlayLoopSound` 사용
- Pool 크기 적절히 설정 (Initial: 20, Max: 100)

### ❌ 하지 말아야 할 것
- Update에서 매 프레임 사운드 재생
- 필요 없는 Transform 추적
- Pool 크기를 너무 작게 설정
- 루프 사운드 정지 안 함

---

## 🔧 Inspector 설정

### 권장 설정 (중간 규모 게임)
```
Use Emitter Pool: ✓
Pool Settings:
  - Initial Pool Size: 20
  - Max Pool Size: 100
  - Auto Expand: ✓
  - Auto Cleanup Interval: 60
```

### 디버그 활성화
```
Show Sound Debug: ✓
Debug Sphere Size: 0.5
OneShot Color: Cyan
Loop Sound Color: Yellow
Emitter Color: Green
```

---

## 📞 자주 묻는 질문

**Q: PlayOneShot은 언제 써야 하나요?**  
A: UI 사운드나 특별한 이유가 있을 때. 일반적으로 `Play3DSound` 추천.

**Q: Transform이 파괴되면 어떻게 되나요?**  
A: Emitter가 자동으로 정리됩니다.

**Q: 파라미터는 어떻게 변경하나요?**  
A: `emitter.emitter.SetParameter("이름", 값)`

**Q: 성능이 얼마나 좋아지나요?**  
A: 동시 사운드가 많을수록 큰 차이 (60~76% 개선)

---

## 🚨 에러 대응

| 에러 메시지 | 원인 | 해결책 |
|-----------|------|--------|
| "Emitter 풀이 고갈" | 동시 사운드가 너무 많음 | Max Pool Size 증가 |
| "EventReference가 null" | FMODEvents 미설정 | Inspector에서 설정 |
| "Emitter Pool 비활성화" | useEmitterPool = false | Inspector에서 활성화 |

---

## 📝 테스트 단축키

게임 실행 중 키보드로 테스트:
- `1` - UI 사운드
- `2` - 3D 사운드 (위치)
- `3` - 3D 사운드 (추적)
- `4` - 루프 사운드 (토글)
- `5` - 고급 옵션
- `0` - Pool 정보

*(AudioManagerTestExample 컴포넌트 필요)*

---

**마지막 업데이트**: 2025-10-29


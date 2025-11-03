# 큐브 발소리 시스템 사용 가이드

## 📋 개요

큐브가 굴러다니면서 바닥에 닿을 때마다 발소리를 재생하는 시스템입니다.  
레이캐스트 기반으로 땅과의 접촉을 감지하고, 속도에 따라 볼륨을 조절합니다.

---

## 🎯 컴포넌트 구성

### 1. `GroundContactRaycast`
- 개별 레이캐스트를 쏘고 땅 접촉을 감지하는 컴포넌트
- 여러 개를 배치하여 큐브의 각 면/모서리에서 감지 가능
- 접촉 감지 시 이벤트를 발생시킴

### 2. `CubeRollingFootstepSystem`
- 여러 `GroundContactRaycast`를 관리하는 매니저
- 접촉 이벤트를 받아 사운드를 재생
- 자동으로 6개 면에 레이캐스트 포인트 생성 가능

---

## 🚀 빠른 시작 (자동 설정)

### 1단계: 큐브 오브젝트에 컴포넌트 추가
```
1. Hierarchy에서 Cube 오브젝트 선택
2. Add Component → CubeRollingFootstepSystem
3. Inspector에서 설정:
   - Auto Create Raycast Points: ✓ (체크)
   - Footstep Sound: FMODEvents의 cubeFootstep 할당
   - Sound Max Distance: 30
```

### 2단계: 완료!
자동으로 6개 면에 레이캐스트 포인트가 생성되고, 굴러다니면서 바닥에 닿을 때마다 발소리가 재생됩니다.

---

## 🔧 고급 설정 (수동 레이캐스트 배치)

특정 위치에만 레이캐스트를 배치하고 싶다면:

### 1단계: 빈 오브젝트 생성
```
1. Cube 오브젝트 하위에 빈 오브젝트 생성 (예: "RaycastPoint_Corner1")
2. 원하는 위치로 이동 (예: 모서리)
3. Add Component → GroundContactRaycast
```

### 2단계: 매니저 설정
```
1. Cube 오브젝트에 CubeRollingFootstepSystem 추가
2. Inspector에서:
   - Auto Create Raycast Points: ✗ (해제)
   - Manual Raycast Points: 생성한 GroundContactRaycast들을 드래그 앤 드롭
```

---

## ⚙️ 주요 설정 옵션

### CubeRollingFootstepSystem

#### Sound Settings
- **Footstep Sound**: 재생할 FMOD 이벤트 (FMODEvents.instance.cubeFootstep)
- **Sound Max Distance**: 사운드가 들리는 최대 거리 (기본: 30m)
- **Global Sound Cooldown**: 사운드 재생 쿨다운 (기본: 0.15초)

#### Volume Settings
- **Adjust Volume By Velocity**: 속도에 따라 볼륨 조절 여부
- **Min/Max Volume**: 최소/최대 볼륨 (0.3 ~ 1.0)
- **Min/Max Velocity**: 볼륨 계산에 사용되는 속도 범위 (1 ~ 10 m/s)

#### Raycast Points
- **Auto Create Raycast Points**: 자동으로 6개 면에 레이캐스트 생성
- **Raycast Point Distance**: 큐브 중심으로부터 거리 (기본: 0.5)

#### Debug
- **Show Debug Info**: 디버그 정보 표시 (Scene 뷰 + 화면)

---

### GroundContactRaycast

#### Raycast Settings
- **Ray Direction**: 레이캐스트 방향 (로컬 좌표계, 기본: 아래)
- **Ray Distance**: 레이캐스트 거리 (기본: 0.6)
- **Ground Layer**: 땅으로 인식할 레이어 (기본: Everything)

#### Contact Detection
- **Contact Cooldown**: 개별 포인트의 접촉 감지 쿨다운 (기본: 0.1초)
- **Min Velocity Threshold**: 이 속도 이하에서는 접촉 감지 안함 (기본: 0.5 m/s)

#### Debug
- **Show Debug Ray**: Scene 뷰에서 레이캐스트 시각화
- **Ray Color No Hit**: 레이 색상 (닿지 않았을 때)
- **Ray Color Hit**: 레이 색상 (닿았을 때)

---

## 🎨 디버그 시각화

### Scene 뷰
- **녹색 선**: 레이캐스트가 땅에 닿지 않음
- **빨간색 선**: 레이캐스트가 땅에 닿음
- **노란색 구**: 접촉 지점

### 화면 표시 (Show Debug Info 활성화 시)
- Active Raycast Points: 활성화된 레이캐스트 포인트 수
- Total Contacts: 총 접촉 횟수
- Velocity: 현재 속도
- Grounded Points: 현재 땅에 닿은 포인트 수

---

## 📝 사용 예시

### 예시 1: 기본 설정
```
큐브가 굴러다니면서 자연스럽게 발소리 재생
- Auto Create: ON
- Global Cooldown: 0.15초
- Volume by Velocity: ON
```

### 예시 2: 빠르게 굴러다니는 큐브
```
빠른 큐브는 더 자주 사운드 재생
- Global Cooldown: 0.1초 (짧게)
- Max Velocity: 20 m/s (높게)
```

### 예시 3: 모서리에만 감지
```
큐브의 8개 모서리에만 레이캐스트 배치
- Auto Create: OFF
- 수동으로 8개 포인트 생성 및 배치
```

---

## 🔍 트러블슈팅

### 문제: 사운드가 재생되지 않음
**해결책:**
1. FMODEvents에서 cubeFootstep 이벤트가 할당되었는지 확인
2. AudioManager가 씬에 존재하는지 확인
3. Debug Info를 켜서 "Total Contacts"가 증가하는지 확인

### 문제: 사운드가 너무 자주 재생됨
**해결책:**
1. Global Sound Cooldown을 늘림 (0.2~0.3초)
2. Min Velocity Threshold를 높임 (1.0~2.0 m/s)

### 문제: 사운드가 너무 적게 재생됨
**해결책:**
1. Ray Distance를 늘림 (0.8~1.0)
2. Contact Cooldown을 줄임 (0.05~0.08초)
3. 레이캐스트 포인트를 더 많이 배치

### 문제: 레이캐스트가 땅을 감지하지 못함
**해결책:**
1. Ground Layer 설정 확인
2. Raycast Point Distance 조정 (큐브 크기에 맞게)
3. Scene 뷰에서 레이캐스트 시각화 확인 (녹색/빨간색 선)

---

## 💡 최적화 팁

### 성능 최적화
1. **레이캐스트 포인트 수 최소화**: 6개면 충분, 필요하면 8개(모서리)
2. **Global Cooldown 활용**: 0.1초 이상 설정 추천
3. **Ground Layer 제한**: 땅 레이어만 포함시켜 연산 감소

### 사운드 품질
1. **Max Distance 조절**: 큐브 크기와 중요도에 맞게 설정
2. **Volume by Velocity**: 물리적 현실감 향상
3. **Min Velocity**: 너무 낮으면 미끄러질 때도 소리 나므로 0.5~1.0 추천

---

## 🎯 EnemyAICube에 적용하기

EnemyAICube 프리팹에 적용하는 방법:

### 1단계: 프리팹 편집
```
1. Hierarchy에서 Cube 적 선택
2. Add Component → CubeRollingFootstepSystem
```

### 2단계: 설정
```
- Footstep Sound: FMODEvents → cubeFootstep
- Sound Max Distance: 30
- Auto Create Raycast Points: ✓
- Adjust Volume By Velocity: ✓
- Global Sound Cooldown: 0.15
```

### 3단계: Inspector에서 FMODEvents 설정
```
1. Hierarchy에서 FMODEvents 오브젝트 선택
2. Inspector → Enemy - Cube → Cube Footstep
3. FMOD 이벤트 할당 (예: FootStep 또는 큐브 전용 사운드)
```

### 4단계: 테스트
```
1. Play 모드 실행
2. 큐브가 굴러다니는지 확인
3. Scene 뷰에서 레이캐스트 시각화 확인
4. 사운드가 재생되는지 확인
```

---

## 📚 코드 예시

### 런타임에 레이캐스트 포인트 추가
```csharp
CubeRollingFootstepSystem footstepSystem = GetComponent<CubeRollingFootstepSystem>();

// 새 레이캐스트 포인트 생성
GameObject newPoint = new GameObject("CustomRaycastPoint");
newPoint.transform.SetParent(transform);
newPoint.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);

GroundContactRaycast raycast = newPoint.AddComponent<GroundContactRaycast>();
footstepSystem.AddRaycastPoint(raycast);
```

### 사운드 재생 커스터마이징
```csharp
// CubeRollingFootstepSystem.cs의 PlayFootstepSound() 메서드를 수정하여
// 재질별로 다른 사운드를 재생할 수 있습니다.

private void PlayFootstepSound()
{
    // LastHit을 통해 접촉한 재질 확인
    var raycastPoint = activeRaycastPoints.Find(r => r.IsGrounded);
    if (raycastPoint != null)
    {
        string materialName = raycastPoint.LastHit.collider.material.name;
        
        // 재질별 사운드 선택
        EventReference soundToPlay = materialName.Contains("Metal") 
            ? FMODEvents.instance.cubeFootstepMetal 
            : FMODEvents.instance.cubeFootstep;
        
        // 사운드 재생
        AudioManager.instance.Play3DSoundAtPosition(soundToPlay, transform.position);
    }
}
```

---

## ✅ 체크리스트

설정이 완료되면 다음 항목을 확인하세요:

- [ ] CubeRollingFootstepSystem이 큐브에 추가됨
- [ ] Rigidbody가 큐브에 있음 (속도 감지용)
- [ ] FMODEvents.cubeFootstep이 할당됨
- [ ] AudioManager가 씬에 존재함
- [ ] Play 모드에서 레이캐스트가 시각화됨 (Scene 뷰)
- [ ] 큐브가 굴러다닐 때 사운드가 재생됨
- [ ] 속도에 따라 볼륨이 변화함 (Adjust Volume By Velocity ON일 때)

---

## 🎉 완료!

이제 큐브가 굴러다니면서 바닥에 닿을 때마다 발소리를 냅니다!  
추가 질문이나 문제가 있으면 언제든지 문의하세요.


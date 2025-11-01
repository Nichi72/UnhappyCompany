# Emitter Gizmo 설정 가이드

## 🎨 Gizmo 설정 옵션

AudioManager에 새로운 Gizmo 설정이 추가되었습니다!

---

## ⚙️ Inspector 설정

### **Emitter Gizmo 설정**

```
[Emitter Gizmo 설정]
✓ Always Show Emitter Gizmos       (항상 표시)
✓ Show Emitter Distance Gizmos     (Distance 범위 표시)
  Distance Gizmo Alpha: 0.15       (투명도)
```

---

## 📋 설정 상세

### 1️⃣ **Always Show Emitter Gizmos**
- **활성화** → 선택하지 않아도 모든 Emitter가 Scene 뷰에 표시됨
- **비활성화** → GameObject를 선택해야만 Gizmo 표시 (Unity 기본 동작)
- **권장**: ✓ 활성화 (디버깅 편리)

### 2️⃣ **Show Emitter Distance Gizmos**
- **활성화** → Min/Max Distance 범위를 원으로 표시
- **비활성화** → Distance 범위 숨김 (아이콘만 표시)
- **권장**: ✓ 활성화 (거리 조절 시 유용)

### 3️⃣ **Distance Gizmo Alpha**
- Distance 범위 원의 투명도 (0 = 투명, 1 = 불투명)
- **권장값**: 0.15 (너무 진하면 씬이 복잡해짐)

---

## 🎨 Gizmo 색상 의미

### **Emitter 아이콘**
- 🟢 **녹색 구** - Emitter 위치
- **녹색 선** - Transform 추적 중 (타겟까지 연결)

### **Distance 범위**
- 🟢 **녹색 원** (내부) - Min Distance (100% 볼륨 유지)
- 🔴 **빨간색 원** (외부) - Max Distance (완전히 사라짐)

### **기존 사운드 (PlayOneShot)**
- 🔵 **청록색** - OneShot 사운드
- 🟡 **노란색** - 루프 사운드 (코루틴)

---

## 🖼️ Scene 뷰에서 보이는 것

```
          🔴 빨간색 원 (Max Distance)
        ┌─────────────────┐
        │                 │
        │   🟢 녹색 원    │  (Min Distance)
        │   ┌─────┐       │
        │   │ 🟢  │       │  녹색 구 (Emitter)
        │   └─────┘       │
        │                 │
        └─────────────────┘
        
        │←─ Min ─→│←─ Fade Out ─→│
```

---

## 💡 사용 시나리오

### **시나리오 1: 일반 작업 (권장)**
```
✓ Always Show Emitter Gizmos: true
✓ Show Emitter Distance Gizmos: true
  Distance Gizmo Alpha: 0.15
```
- 모든 사운드 위치가 보임
- Distance 범위가 투명하게 표시됨
- 가장 균형잡힌 설정

### **시나리오 2: 성능 우선 (많은 사운드)**
```
✓ Always Show Emitter Gizmos: true
✗ Show Emitter Distance Gizmos: false
```
- Emitter 아이콘만 표시
- Distance 범위는 숨김
- Scene 뷰가 깔끔함

### **시나리오 3: Distance 조절 작업**
```
✓ Always Show Emitter Gizmos: true
✓ Show Emitter Distance Gizmos: true
  Distance Gizmo Alpha: 0.30
```
- Distance 범위를 더 진하게 표시
- Min/Max Distance 조절할 때 유용

### **시나리오 4: Unity 기본 동작**
```
✗ Always Show Emitter Gizmos: false
✗ Show Emitter Distance Gizmos: false
```
- GameObject 선택 시에만 표시
- 기존 Unity AudioSource와 동일한 동작

---

## 🔍 디버깅 정보

Scene 뷰에서 Emitter를 보면:

```
[EMITTER] Rampage MoveLoop
재생: 12.3초
[추적 중]
Min: 5.0m / Max: 40.0m
```

표시되는 정보:
- **사운드 이름** (debugName)
- **재생 시간**
- **Transform 추적 여부**
- **Min/Max Distance** (Override된 경우)

---

## ⚠️ 주의사항

### 1. **너무 많은 사운드가 동시에 재생되면**
- Scene 뷰가 복잡해질 수 있음
- `Show Emitter Distance Gizmos`를 끄는 것 추천

### 2. **Distance Gizmo가 너무 크면**
- `Distance Gizmo Alpha`를 낮춤 (0.10 이하)
- 또는 `Show Emitter Distance Gizmos` 끄기

### 3. **Gizmo가 안 보이면**
- Scene 뷰 상단 `Gizmos` 버튼 확인
- `Show Sound Debug` 활성화 확인

---

## 🎮 단축키로 빠르게 설정

Inspector에서 AudioManager GameObject를 선택한 상태에서:

| 설정 | 권장 값 | 용도 |
|------|---------|------|
| `Always Show Emitter Gizmos` | ✓ | 항상 표시 |
| `Show Emitter Distance Gizmos` | ✓ | 거리 범위 표시 |
| `Distance Gizmo Alpha` | 0.15 | 투명도 |

---

## 📊 Before vs After

### Before (기본 Unity)
- ❌ GameObject를 **선택해야만** Gizmo 보임
- ❌ 여러 사운드를 동시에 비교하기 어려움
- ❌ Distance 범위가 안 보임

### After (새로운 시스템)
- ✅ **모든 Emitter가 항상 보임**
- ✅ 여러 사운드를 한눈에 비교 가능
- ✅ **Min/Max Distance가 원으로 표시됨**
- ✅ Transform 추적도 선으로 표시

---

## 🚀 실전 활용

### Distance 조절 작업 순서

1. **Scene 뷰 열기**
2. **Play 모드 실행**
3. **사운드 재생** (예: 키보드 3번)
4. **녹색 구 확인** (Emitter 위치)
5. **빨간색 원 확인** (Max Distance)
6. **원이 너무 작거나 크면:**
   - 코드에서 `Play3DSoundFollowingWithDistance` 수정
   - MaxDistance 값 조절
   - 실시간으로 원의 크기 변화 확인

---

## 💡 팁

1. **Scene 뷰 카메라를 멀리 빼면**
   - 전체 Distance 범위를 한눈에 볼 수 있음

2. **여러 사운드를 동시에 재생하면**
   - 서로 겹치는 Distance 범위를 확인 가능
   - 사운드 마스킹 문제 확인 가능

3. **투명도를 낮추면**
   - Scene이 깔끔해짐
   - 많은 사운드를 동시에 확인 가능

---

**작성일**: 2025-10-29  
**버전**: 1.0  
**업데이트**: Always Show Emitter Gizmos 추가


# 밸런싱 수치

## 시간 관련 수치

### 확정된 수치
- **탐험 페이즈**: 12분 ✅
- **안전 탐험 시간**: 7분 ✅
- **위험 탐험 시간**: 5분 ✅
- **센터 대응 시간**: 3.5-4.5초 ✅

### 조정 필요한 수치
- **방어 페이즈**: 10-12분 🔄 (밸런싱 필요)

---

## 경제 관련 수치

### 배터리 관련
- **상점 가격**: 높게 설정 (경제적 압박 조성)
- **목표 밸런스**:
  - 1개 구매 → 50% 생존 확률
  - 2개 구매 → 75% 생존 확률
- **설계 의도**: 파밍 유도하되 최소 생존 보장

### 트랩 연료 관련
- **불통 가격**: 🔄 (상점에서 구매, 구체적 수치 테스트 필요)
- **물통 가격**: 🔄 (상점에서 구매, 구체적 수치 테스트 필요)
- **유압가스통 가격**: 🔄 (상점에서 구매, 구체적 수치 테스트 필요)
- **목표**: 각 속성별 균형잡힌 경제적 부담
- **제한**: 재장전 시간으로 남용 방지

### 기타 경제
- **폐철물 가치**: 탐험 위험도에 비례
- **쿠션 가격**: 렘페이지 대응용 (중간 가격대)
- **업그레이드 비용**: 장기 목표 설정

---

## 센터 관련 수치

### 확정된 수치
- **센터 크기**: 10m × 10m (달리기 5초 × 5초) ✅
- **계단 이동**: 1-2초 ✅
- **문 조작**: 토글 방식 (즉시) ✅
- **CCTV 제한**: 인당 5개 (최대 20개) ✅
- **센터 HP**: 100 (크리처별 피해량 조정) ✅

---

## 전력 소모 수치

### 테스트 필요한 수치 🔄

#### 기본 소모
- **컴퓨터**: X 전력/분
- **중앙 조명**: Y 전력/분

#### 가변 소모
- **CCTV**: Z 전력/개/분
- **문 닫기**: W 전력/분

### 밸런싱 목표
- **기본 생존**: 상점 배터리 1개로 가능
- **안전 생존**: 상점 배터리 2개로 여유
- **최적 플레이**: 파밍 배터리로 CCTV 다수 운영

---

## 난이도 곡선

### 사이클별 진행

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

### 자연스러운 난이도 증가

#### 맵 확장으로 인한 효과
- 탐험 시간 증가 (더 멀리 가야 함)
- 복귀 시간 증가 (안개 시 위험)
- CCTV 커버리지 한계 (20개로 부족)
- 크리처 접근 경로 다양화
- 자원 관리 복잡성 증가

#### 플레이어 부담감 증가
- **심리적 압박**: 더 넓은 영역 관리
- **물리적 압박**: 더 많은 이동 필요
- **전략적 압박**: 복잡한 의사결정
- **팀워크 압박**: 더 정교한 협조 필요

---

## 밸런싱 테스트 계획

### Phase 1: 기본 수치 설정
- [ ] 전력 소모량 기본값 설정
- [ ] 배터리 가격 1차 설정
- [ ] 크리처 피해량 기본값

### Phase 2: 플레이테스트 기반 조정
- [ ] 10회 이상 플레이테스트 진행
- [ ] 생존률 데이터 수집
- [ ] 플레이어 행동 패턴 분석

### Phase 3: 세밀한 튜닝
- [ ] 난이도 곡선 조정
- [ ] 경제 시스템 최적화
- [ ] 협동 플레이 밸런스

### 데이터 수집 항목
- **생존률**: 사이클별 플레이어 생존 확률
- **자원 사용량**: 배터리, 트랩 연료 소모 패턴
- **플레이 시간**: 각 페이즈별 소요 시간
- **실패 원인**: 게임오버 발생 이유 분석
- **협동 효율성**: 솔로 vs 멀티 성능 비교

### 밸런싱 도구
```csharp
public class BalancingManager : MonoBehaviour
{
    [Header("데이터 수집")]
    public bool enableDataCollection = true;
    
    [Header("실시간 조정")]
    public float powerConsumptionMultiplier = 1.0f;
    public float creatureDamageMultiplier = 1.0f;
    public float batteryPriceMultiplier = 1.0f;
    
    // 플레이테스트 중 실시간 수치 조정
    public void AdjustDifficulty(float modifier) { }
}
```

---

## 협동 플레이 밸런싱

### 인원수별 조정
- **1인**: 기본 난이도
- **2인**: 자원 1.5배, 크리처 1.3배
- **3인**: 자원 2배, 크리처 1.6배
- **4인**: 자원 2.5배, 크리처 2배

### 역할 분담 최적화
- **CCTV 담당**: 정보 수집 및 전달
- **문 담당**: 빠른 방어 대응
- **자원 담당**: 탐험 및 아이템 관리
- **지휘 담당**: 전체 상황 판단 및 지시

### 의사소통 도구
- **음성 채팅**: 실시간 소통 (권장)
- **마커 시스템**: 화면에 위치 표시
- **간단한 신호**: 위험/안전/도움 등

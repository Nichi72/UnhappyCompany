# 환경 시스템

## 안개 시스템 (핵심 압박 메커니즘)

### 기술적 구현
- **Unity URP Volume 시스템** 활용
- 시간에 따른 점진적 농도 변화
- 실시간 렌더링 최적화 필요

### 게임플레이 효과

#### 100% 시야 (0-7분)
- **아이템 발견율**: 100%
- **크리처 인지 거리**: 최대
- **길찾기 난이도**: 쉬움
- **심리적 상태**: 안전감, 계획 수립

#### 60% 시야 (7-12분)
- **아이템 발견율**: 75%
- **크리처 인지 거리**: 감소
- **길찾기 난이도**: 중간
- **미니맵 의존도**: 증가
- **심리적 상태**: 긴장감 시작

#### 30% 시야 (12분~)
- **아이템 발견율**: 25%
- **크리처 인지 거리**: 최소
- **길찾기 난이도**: 매우 어려움
- **미니맵 의존도**: 최대
- **심리적 상태**: 극도의 긴장





## 맵 구조

### 절차적 맵 생성

#### 기본 구조
- **기본 맵**: 병원 테마
- **확장 구조**: 센터를 중심으로 방사형 확장
- **생성 알고리즘**: 방-복도 연결 구조
- **특수 구역**: 계단이 있는 룸, 위험 지역, 고가치 아이템 구역

#### 맵 생성 규칙

센터 (0,0)
├── 북쪽 구역 (0, +Y)
├── 동쪽 구역 (+X, 0)
├── 남쪽 구역 (0, -Y)
└── 서쪽 구역 (-X, 0)


### 센터 (플레이어 거점)

#### 2층: 철제 구조물
- **컴퓨터** (CCTV 통합 관리)
- **전자기기들**
- **1층 전체 시야 확보**
- **계단/사다리**로 1층 연결

#### 1층: 정사각형 개방 공간 (5초×5초 크기)
- **동문 (E) ←→ 서문 (W)**: 5초 거리
- **남문 (S) ←→ 북문 (N)**: 5초 거리
- **중앙에서 각 문**: 2.5초 거리
- **각 문마다 독립 조작 버튼**
- **이동 방해 오브젝트 없음** (자유로운 이동)

### 절차적 생성 알고리즘

#### Level 1: 기본 맵 생성


#### Level 2: 방 타입별 생성
- **일반 방**: 기본 아이템 배치
- **의료실**: 의료용품 집중 배치
- **전기실**: 배터리 및 전자기기
- **저장고**: 고가치 아이템 (위험도 높음)
- **계단실**: 다층 구조 (복잡한 탐색)

### 아이템 배치 시스템

#### 위험도별 아이템 분포
- **안전 지역 (센터 반경 30m)**:
  - 기본 아이템 70%
  - 배터리 20%
  - 고가치 아이템 10%

- **위험 지역 (센터 반경 30m+)**:
  - 기본 아이템 40%
  - 배터리 30%
  - 고가치 아이템 30%

- **극위험 지역 (안개 지역)**:
  - 기본 아이템 20%
  - 배터리 40%
  - 고가치 아이템 40%

#### 동적 아이템 리스폰





## 조명 시스템

### 시간대별 조명 변화
- **06:00-13:00**: 밝은 자연광
- **13:00-16:00**: 중간 조명 (안개와 함께 어두워짐)
- **16:00-18:00**: 어두운 조명 (안개 + 저조도)
- **18:00-06:00**: 인공 조명만 (센터 내부)

### 전력 기반 조명
- **센터 조명**: 전력 소모, 필수 유지
- **복도 조명**: 전력 절약 시 자동 꺼짐
- **비상 조명**: 최소한의 시야 보장



## 사운드 환경

### 환경음
- **안개 시**: 바람 소리, 거리감 있는 소음
- **크리처 활동**: 멀리서 들리는 움직임
- **센터 내부**: 전자기기 작동음, 환풍기

### 거리 기반 사운드
- **가까운 소리**: 명확하고 방향성 있음
- **먼 소리**: 흐릿하고 에코 효과
- **안개 중 소리**: 왜곡되고 방향 파악 어려움






## 성능 최적화

### LOD (Level of Detail) 시스템
- **가까운 오브젝트**: 고해상도 모델
- **먼 오브젝트**: 저해상도 모델
- **안개 속 오브젝트**: 최저 해상도

### 컬링 최적화
- **프러스텀 컬링**: 시야 밖 오브젝트 렌더링 제외
- **오클루전 컬링**: 가려진 오브젝트 렌더링 제외
- **거리 컬링**: 안개로 보이지 않는 먼 오브젝트 제외

### 메모리 관리
- **오브젝트 풀링**: 아이템, 이펙트 재사용
- **텍스처 압축**: 메모리 사용량 최적화
- **가비지 컬렉션**: 불필요한 메모리 해제

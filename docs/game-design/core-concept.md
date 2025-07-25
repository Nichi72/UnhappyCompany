# 게임 컨셉

## 기본 컨셉

**장르**: 생존 호러 게임  
**핵심 메커니즘**: 오전-오후 사이클 기반 생존

### 게임플레이 구조
- **오전**: 리썰컴퍼니 스타일 탐험/파밍 (능동적 위험 감수)
- **오후**: 프레디의 피자가게 스타일 방어 생존 (수동적 생존)

## 참조 게임 분석

### 리썰컴퍼니에서 차용한 요소
- 팀워크 기반 협동 플레이
- 시간 압박 (할당량/복귀 시간)
- 자원 관리 및 위험-보상 트레이드오프
- 실시간 12분 = 게임 내 시간 흐름

### 프레디의 피자가게에서 차용한 요소
- 제한된 전력으로 방어 시설 운영
- CCTV 모니터링 시스템
- 문 닫기를 통한 크리처 차단
- 시간이 지날수록 증가하는 위협

## 차별화 포인트

### 물리적 이동이 핵심
프레디의 정적 플레이와 달리 우리 게임은 동적 이동이 핵심

### 탐험-방어 직접 연결
오전 성과가 오후 생존에 즉시 영향

### 무한 확장 구조
맵 확장으로 인한 자연스러운 난이도 증가

## 핵심 설계 원칙

### 시스템 설계 원칙 (개발자 관점)
1. **직접 연결성**: 탐험에서 얻은 자원이 방어 페이즈에 실질적 영향을 미쳐야 함
2. **다목적 자원**: 한 자원이 여러 용도로 활용 가능해야 함 (배터리 = 탐험 도구 + 방어 자원)
3. **명확한 비용**: 모든 선택에는 분명한 트레이드오프가 존재해야 함
4. **학습 곡선**: 신규자도 기본 생존 가능, 숙련자는 최적화 추구 가능
5. **개발 현실성**: 2인 인디팀 규모에 맞는 복잡도 유지

### 기술적 원칙
1. **확장성**: 절차적 생성과 모듈화로 콘텐츠 확장 용이성
2. **성능**: Unity 최적화 고려한 시스템 설계
3. **협동성**: 멀티플레이어 지원을 위한 네트워킹 준비
4. **데이터 기반**: 밸런싱을 위한 데이터 수집 시스템

### 플레이어 경험 원칙 (플레이어 관점)
1. **선택의 딜레마**: "더 탐험 vs 안전 복귀" 등 의미 있는 고민 상황 제공
2. **긴장감 조절**: 안전→위험→극한의 점진적 긴장감 증가
3. **팀워크 가치**: 혼자보다 함께할 때 명확한 이점과 즐거움 제공
4. **리플레이 가치**: 매 플레이마다 다른 전략과 선택으로 새로운 경험
5. **성취감**: 어려움을 극복했을 때의 명확한 보상과 만족감

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnhappyCompany
{
    /// <summary>
    /// 굴곡진(3D 곡면) 슬롯머신 휠 구현
    /// 실제 릴이 회전하는 대신 심볼들이 곡면 궤도를 따라 움직입니다.
    /// </summary>
    public class SlotMachine : MonoBehaviour
    {
        [Header("슬롯 설정")]
        [Tooltip("곡면의 기본 반지름")]
        public float radius = 2.0f;
        
        [Tooltip("X축 곡률 계수 (1이면 원, 0에 가까울수록 평평해짐)")]
        [Range(0.1f, 1.0f)]
        public float xCurvature = 0.7f;
        
        [Tooltip("수직 오프셋 (심볼 위치의 Y값)")]
        public float verticalOffset = 0.0f;
        
        [Tooltip("회전 속도 (도/초)")]
        public float spinSpeed = 100.0f;
        
        [Header("심볼 량 설정")]
        [Tooltip("심볼 간 각도 간격 (값이 작을수록 더 많은 심볼)")]
        [Range(5f, 90f)]
        public float symbolAngleSpacing = 30.0f;
        
        [Tooltip("화면에 보이는 각도 범위 (값이 클수록 더 많은 심볼 표시)")]
        [Range(30f, 180f)]
        public float visibleRange = 90.0f;
        
        [Tooltip("화면 중앙 각도 (보통 0도)")]
        public float visibleCenterAngle = 0.0f;
        
        // 계산된 X축 반지름 (더 큰 값 = 더 평평한 곡선)
        private float RadiusX => radius / xCurvature;
        // Z축 반지름은 유지
        private float RadiusZ => radius;
        
        [Header("이징 효과 설정")]
        [Tooltip("감속 시간 (초)")]
        public float decelerationTime = 3.0f;
        
        [Tooltip("이징 효과 유형")]
        public EasingType easingType = EasingType.EaseOutQuart;
        
        [Header("심볼 설정")]
        [Tooltip("심볼 프리팹 목록")]
        public List<GameObject> symbolPrefabs = new List<GameObject>();
        
        [Tooltip("심볼 홀더 (심볼들의 부모 오브젝트)")]
        public Transform symbolHolder;
        
        [Header("테스트 설정")]
        [Tooltip("에디터에서 테스트할 결과 심볼 인덱스")]
        [Range(0, 10)]
        public int testResultSymbolIndex = 0;
        
        // 이징 타입 열거형
        public enum EasingType
        {
            Linear,
            EaseOutQuad,
            EaseOutCubic,
            EaseOutQuart,
            EaseOutExpo,
            EaseOutSine,
            EaseOutElastic,
            EaseOutBounce,
            SlotMachineEaseOut
        }
        
        // 심볼 관리를 위한 클래스
        [System.Serializable]
        public class Symbol
        {
            public GameObject gameObject;
            public float angle;
            public int symbolIndex; // 심볼 종류 인덱스
            public SlotMachineSymbol symbolComponent; // 심볼 컴포넌트
            
            public Symbol(GameObject go, float angle, int symbolIndex)
            {
                this.gameObject = go;
                this.angle = angle;
                this.symbolIndex = symbolIndex;
                this.symbolComponent = go.GetComponent<SlotMachineSymbol>();
                
                // 컴포넌트가 없으면 추가
                if (this.symbolComponent == null)
                {
                    this.symbolComponent = go.AddComponent<SlotMachineSymbol>();
                }
            }
        }
        
        // 심볼 목록
        private List<Symbol> symbols = new List<Symbol>();
        
        // 곡면의 중심점
        private Vector3 centerPoint;
        
        // 회전 상태
        private bool isSpinning = false;
        
        // 멈출 최종 각도 (결과값)
        private float targetStopAngle;
        
        // 감속 관련 변수
        private float decelerationStartTime;
        private float initialSpinSpeed;
        private bool isDecelerating = false;
        
        // 스핀 완료 이벤트
        public delegate void SpinCompleteDelegate();
        public event SpinCompleteDelegate OnSpinComplete;
        
        private void Awake()
        {
            // 중심점 설정
            centerPoint = transform.position;
            
            // 초기 속도 설정
            initialSpinSpeed = spinSpeed;
            
            if (symbolHolder == null)
            {
                symbolHolder = transform;
            }
        }
        
        private void Start()
        {
            // 초기 심볼 생성
            InitializeSymbols();
        }
        
        private void Update()
        {
            if (isSpinning)
            {
                if (isDecelerating)
                {
                    // 감속 중일 때는 이징 함수 사용
                    float elapsedTime = Time.time - decelerationStartTime;
                    float normalizedTime = Mathf.Clamp01(elapsedTime / decelerationTime);
                    
                    // 선택된 이징 함수 적용
                    float t = ApplyEasing(normalizedTime);
                    
                    // 속도 계산 (1.0에서 시작하여 0.0으로 감소)
                    float speedFactor = 1.0f - t;
                    
                    // 현재 회전 속도 계산
                    float currentSpeed = initialSpinSpeed * speedFactor;
                    
                    // 심볼 회전
                    SpinSymbolsWithSpeed(currentSpeed);
                    
                    // 회전 완료 체크
                    if (normalizedTime >= 1.0f)
                    {
                        StopSpin();
                    }
                }
                else
                {
                    // 일반 회전 (등속)
                    SpinSymbolsWithSpeed(spinSpeed);
                }
            }
        }
        
        /// <summary>
        /// 선택된 이징 함수 적용
        /// </summary>
        private float ApplyEasing(float t)
        {
            switch (easingType)
            {
                case EasingType.Linear:
                    return SlotMachineEasingUtils.Linear(t);
                
                case EasingType.EaseOutQuad:
                    return SlotMachineEasingUtils.EaseOutQuad(t);
                
                case EasingType.EaseOutCubic:
                    return SlotMachineEasingUtils.EaseOutCubic(t);
                
                case EasingType.EaseOutQuart:
                    return SlotMachineEasingUtils.EaseOutQuart(t);
                
                case EasingType.EaseOutExpo:
                    return SlotMachineEasingUtils.EaseOutExpo(t);
                
                case EasingType.EaseOutSine:
                    return SlotMachineEasingUtils.EaseOutSine(t);
                
                case EasingType.EaseOutElastic:
                    return SlotMachineEasingUtils.EaseOutElastic(t);
                
                case EasingType.EaseOutBounce:
                    return SlotMachineEasingUtils.EaseOutBounce(t);
                
                case EasingType.SlotMachineEaseOut:
                    return SlotMachineEasingUtils.SlotMachineEaseOut(t);
                
                default:
                    return SlotMachineEasingUtils.SlotMachineEaseOut(t);
            }
        }
        
        /// <summary>
        /// 초기 심볼 생성 및 설정
        /// </summary>
        private void InitializeSymbols()
        {
            if (symbolPrefabs.Count == 0)
            {
                Debug.LogError("심볼 프리팹이 설정되지 않았습니다!");
                return;
            }
            
            // 기존 심볼 제거
            ClearSymbols();
            
            // 전체 360도를 심볼 간격으로 나누어 필요한 심볼 수 계산
            int totalSymbolCount = Mathf.CeilToInt(360f / symbolAngleSpacing);
            
            // 심볼 생성 및 배치
            for (int i = 0; i < totalSymbolCount; i++)
            {
                // 심볼 각도 설정
                float angle = i * symbolAngleSpacing;
                
                // 심볼 인덱스를 순차적으로 설정 (0부터 시작)
                int symbolIndex = i % symbolPrefabs.Count;
                GameObject symbolPrefab = symbolPrefabs[symbolIndex];
                
                // 심볼 인스턴스화
                GameObject symbolObj = Instantiate(symbolPrefab, Vector3.zero, Quaternion.identity, symbolHolder);
                symbolObj.name = $"Symbol_{i}";
                
                // 심볼 객체 생성
                Symbol symbol = new Symbol(symbolObj, angle, symbolIndex);
                symbols.Add(symbol);
                
                // 위치 업데이트
                UpdateSymbolPosition(symbol);
                
                // 화면에 보이는지 확인하여 활성화/비활성화
                bool isVisible = IsSymbolVisible(angle);
                symbolObj.SetActive(isVisible);
                
                // 심볼 컴포넌트에 각도 정보 전달
                if (symbol.symbolComponent != null)
                {
                    symbol.symbolComponent.SetAngle(angle, visibleCenterAngle);
                }
            }
            
            Debug.Log($"슬롯머신 초기화: {totalSymbolCount}개의 심볼 생성");
        }
        
        /// <summary>
        /// 심볼 위치 업데이트 (타원형 궤도 사용)
        /// </summary>
        private void UpdateSymbolPosition(Symbol symbol)
        {
            // 라디안으로 변환
            float rad = Mathf.Deg2Rad * symbol.angle;
            
            // 타원 방정식 적용 (X축은 곡률에 따라 조정)
            Vector3 pos = new Vector3(
                RadiusX * Mathf.Sin(rad),
                verticalOffset,
                RadiusZ * Mathf.Cos(rad)
            );
            
            // 위치 설정 (로컬 좌표계 사용)
            symbol.gameObject.transform.localPosition = pos;
            
            // 심볼의 회전 각도 계산 (타원에 맞게 조정)
            Vector3 normal = new Vector3(
                Mathf.Sin(rad) / (RadiusX * RadiusX),
                0,
                Mathf.Cos(rad) / (RadiusZ * RadiusZ)
            ).normalized;
            
            // 부모 객체(SlotMachine)의 로컬 좌표계 기준으로 회전 설정
            // 월드 회전이 아닌 로컬 회전만 설정하여 부모 회전에 따라 자연스럽게 따라가도록 함
            symbol.gameObject.transform.localRotation = Quaternion.LookRotation(-normal);
            symbol.gameObject.transform.localRotation = Quaternion.Euler(symbol.gameObject.transform.localRotation.eulerAngles.x, symbol.gameObject.transform.localRotation.eulerAngles.y, 90);
            
        }
        
        /// <summary>
        /// 심볼이 화면에 보이는지 확인
        /// </summary>
        private bool IsSymbolVisible(float symbolAngle)
        {
            float delta = Mathf.DeltaAngle(symbolAngle, visibleCenterAngle);
            return Mathf.Abs(delta) < visibleRange / 2f;
        }
        
        /// <summary>
        /// 특정 속도로 모든 심볼 회전
        /// </summary>
        private void SpinSymbolsWithSpeed(float speed)
        {
            foreach (Symbol symbol in symbols)
            {
                // 이전 가시성 상태 저장
                bool wasVisible = symbol.gameObject.activeSelf;
                
                // 각도 업데이트
                symbol.angle += speed * Time.deltaTime;
                
                // 각도가 360도를 넘어가면 0도로 리셋 (반복 회전)
                if (symbol.angle >= 360f)
                {
                    symbol.angle -= 360f;
                    
                    // 선택적으로 심볼 변경 가능
                    if (isSpinning && !isDecelerating)
                    {
                        symbol.symbolIndex = Random.Range(0, symbolPrefabs.Count);
                        // 기존 심볼 제거 및 새 심볼 생성
                        Destroy(symbol.gameObject);
                        GameObject newSymbolObj = Instantiate(symbolPrefabs[symbol.symbolIndex], Vector3.zero, Quaternion.identity, symbolHolder);
                        newSymbolObj.name = symbol.gameObject.name;
                        symbol.gameObject = newSymbolObj;
                        
                        // 심볼 컴포넌트 가져오기
                        symbol.symbolComponent = newSymbolObj.GetComponent<SlotMachineSymbol>();
                        if (symbol.symbolComponent == null)
                        {
                            symbol.symbolComponent = newSymbolObj.AddComponent<SlotMachineSymbol>();
                        }
                    }
                }
                
                // 위치 업데이트
                UpdateSymbolPosition(symbol);
                
                // 가시성 업데이트
                bool isVisible = IsSymbolVisible(symbol.angle);
                
                // 가시성이 변경된 경우만 처리
                if (wasVisible != isVisible)
                {
                    symbol.gameObject.SetActive(isVisible);
                    
                    // 심볼이 새로 나타나는 경우 등장 효과 재생
                    if (isVisible && !wasVisible && symbol.symbolComponent != null)
                    {
                        // 게임 오브젝트가 활성화되어 있는지 확인
                        if (symbol.gameObject.activeInHierarchy)
                        {
                            symbol.symbolComponent.PlayEnterEffect();
                        }
                    }
                    
                    // 심볼이 사라지는 경우 퇴장 효과 재생
                    if (!isVisible && wasVisible && symbol.symbolComponent != null)
                    {
                        // 게임 오브젝트가 활성화되어 있는지 확인 (이미 비활성화된 경우에는 호출하지 않음)
                        if (symbol.gameObject.activeInHierarchy)
                        {
                            symbol.symbolComponent.PlayExitEffect();
                        }
                    }
                }
                
                // 심볼의 각도 정보 업데이트
                if (symbol.symbolComponent != null)
                {
                    symbol.symbolComponent.SetAngle(symbol.angle, visibleCenterAngle);
                }
            }
        }
        
        /// <summary>
        /// 회전 시작
        /// </summary>
        [ContextMenu("Start Spin")]
        public void StartSpin()
        {
            isSpinning = true;
            isDecelerating = false;
            initialSpinSpeed = spinSpeed;
            
            Debug.Log("슬롯머신 회전 시작");
        }
        
        /// <summary>
        /// 특정 심볼이 중앙에 오도록 결과 설정
        /// </summary>
        public void SetResult(int centerSymbolIndex)
        {
            if (symbolPrefabs.Count == 0)
            {
                Debug.LogError("심볼 프리팹이 설정되지 않았습니다!");
                return;
            }

            // 인덱스가 유효한 범위 내에 있도록 조정
            centerSymbolIndex = centerSymbolIndex % symbolPrefabs.Count;
            
            // 목표 심볼 찾기 (반드시 존재함)
            Symbol targetSymbol = symbols[centerSymbolIndex];
            
            // 멈출 각도 계산 (현재 각도를 계속 회전하여 중앙에 오도록)
            float currentAngle = targetSymbol.angle;
            float angleDifference = Mathf.DeltaAngle(currentAngle, visibleCenterAngle);
            
            // 최소 1바퀴 이상 돌도록 설정
            if (angleDifference > 0)
            {
                targetStopAngle = currentAngle + (360f - angleDifference);
            }
            else
            {
                targetStopAngle = currentAngle - angleDifference;
            }
            
            // 감속 시작
            StartDeceleration();
            
            Debug.Log($"슬롯머신 결과 설정: {centerSymbolIndex}번 심볼");
        }
        
        /// <summary>
        /// 에디터에서 테스트 인덱스 값으로 결과 설정
        /// </summary>
        [ContextMenu("Set Test Result")]
        public void SetTestResult()
        {
            if (symbolPrefabs.Count == 0)
            {
                Debug.LogError("심볼 프리팹이 설정되지 않았습니다!");
                return;
            }
            
            // 실제 인덱스는 심볼 수를 넘지 않게 조정
            int actualIndex = testResultSymbolIndex % symbolPrefabs.Count;
            
            // 이미 회전 중이 아니면 회전 시작
            if (!isSpinning)
            {
                StartSpin();
            }
            
            // 약간의 딜레이 후 결과 설정 (에디터에서도 작동하도록)
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                SetResult(actualIndex);
            };
#else
            // 빌드에서는 코루틴 사용
            StartCoroutine(SetSpecificResultAfterDelay(1.0f, actualIndex));
#endif
            
            Debug.Log($"테스트 결과 설정: {actualIndex}번 심볼");
        }
        
        /// <summary>
        /// 일정 시간 후 특정 결과 설정 (빌드용)
        /// </summary>
        private IEnumerator SetSpecificResultAfterDelay(float delay, int symbolIndex)
        {
            yield return new WaitForSeconds(delay);
            
            // 결과 설정
            SetResult(symbolIndex);
        }
        
        /// <summary>
        /// 랜덤 결과로 회전 시작 및 결과 설정
        /// </summary>
        [ContextMenu("Spin with Random Result")]
        public void SpinWithRandomResult()
        {
            if (symbolPrefabs.Count == 0)
            {
                Debug.LogError("심볼 프리팹이 설정되지 않았습니다!");
                return;
            }
            
            // 회전 시작
            StartSpin();
            
            // 랜덤 심볼 선택
            int randomSymbolIndex = Random.Range(0, symbolPrefabs.Count);
            
            // 약간의 딜레이 후 결과 설정 (에디터에서도 작동하도록)
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                SetResult(randomSymbolIndex);
            };
#else
            // 빌드에서는 코루틴 사용
            StartCoroutine(SetSpecificResultAfterDelay(1.0f, randomSymbolIndex));
#endif
            
            Debug.Log($"랜덤 결과로 회전 시작: {randomSymbolIndex}번 심볼");
        }
        
        /// <summary>
        /// 감속 시작
        /// </summary>
        private void StartDeceleration()
        {
            isDecelerating = true;
            decelerationStartTime = Time.time;
        }
        
        /// <summary>
        /// 회전 정지
        /// </summary>
        [ContextMenu("Stop Spin")]
        private void StopSpin()
        {
            isSpinning = false;
            isDecelerating = false;
            
            // 정확한 위치에 심볼 배치
            AlignSymbolsToFinalPosition();
            
            // 스핀 완료 이벤트 발생
            if (OnSpinComplete != null)
            {
                OnSpinComplete.Invoke();
            }
            
            Debug.Log("슬롯머신 회전 정지");
        }
        
        /// <summary>
        /// 최종 위치에 심볼 정렬
        /// </summary>
        private void AlignSymbolsToFinalPosition()
        {
            // 모든 심볼의 위치를 정확한 각도로 조정
            foreach (Symbol symbol in symbols)
            {
                // 현재 심볼의 각도를 visibleCenterAngle 기준으로 조정
                float adjustedAngle = Mathf.DeltaAngle(symbol.angle, visibleCenterAngle);
                
                // 심볼의 위치를 정확한 각도로 설정
                symbol.angle = visibleCenterAngle + adjustedAngle;
                
                // 심볼의 위치 업데이트
                UpdateSymbolPosition(symbol);
                
                // 심볼 컴포넌트의 각도 정보 업데이트
                if (symbol.symbolComponent != null)
                {
                    symbol.symbolComponent.SetAngle(symbol.angle, visibleCenterAngle);
                }
            }
        }
        
        /// <summary>
        /// 모든 심볼 제거
        /// </summary>
        private void ClearSymbols()
        {
            foreach (Symbol symbol in symbols)
            {
                if (symbol.gameObject != null)
                {
                    Destroy(symbol.gameObject);
                }
            }
            
            symbols.Clear();
        }
        
        /// <summary>
        /// 심볼 간격 및 가시 범위 변경 후 심볼 재생성
        /// </summary>
        [ContextMenu("재생성")]
        public void RecreateSymbols()
        {
            // 이미 회전 중이면 중단
            if (isSpinning)
            {
                Debug.LogWarning("슬롯머신이 회전 중입니다. 정지 후 심볼을 재생성하세요.");
                return;
            }
            
            InitializeSymbols();
        }
        
        private void OnDestroy()
        {
            ClearSymbols();
        }
        
        /// <summary>
        /// 심볼 간격 설정 (값이 작을수록 더 많은 심볼)
        /// </summary>
        public void SetSymbolAngleSpacing(float spacing)
        {
            symbolAngleSpacing = Mathf.Clamp(spacing, 5f, 90f);
            if (!isSpinning)
            {
                RecreateSymbols();
            }
        }
        
        /// <summary>
        /// 가시 범위 설정 (값이 클수록 더 많은 심볼 표시)
        /// </summary>
        public void SetVisibleRange(float range)
        {
            visibleRange = Mathf.Clamp(range, 30f, 180f);
            if (!isSpinning)
            {
                RecreateSymbols();
            }
        }
    }
} 
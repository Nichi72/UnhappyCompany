using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnhappyCompany
{
    /// <summary>
    /// 가위바위보 슬롯머신 릴 3개를 관리하는 컨트롤러
    /// 스택 개수에 따라 릴 개수를 조절하고, 승리 시 오른쪽 릴부터 정지시킵니다.
    /// </summary>
    public class SlotMachineController : MonoBehaviour
    {
        [Header("슬롯머신 릴 설정")]
        [Tooltip("왼쪽 릴")]
        public SlotMachine leftReel;
        
        [Tooltip("중앙 릴")]
        public SlotMachine middleReel;
        
        [Tooltip("오른쪽 릴")]
        public SlotMachine rightReel;
        
        [Header("가위바위보 설정")]
        [Tooltip("가위 심볼 인덱스")]
        public int scissorsSymbolIndex = 0;
        
        [Tooltip("바위 심볼 인덱스")]
        public int rockSymbolIndex = 1;
        
        [Tooltip("보 심볼 인덱스")]
        public int paperSymbolIndex = 2;
        
        [Header("심볼 양 설정")]
        [Tooltip("심볼 간격 설정 슬라이더 (낮을수록 더 많은 심볼)")]
        public Slider symbolSpacingSlider;
        
        [Tooltip("가시 범위 설정 슬라이더 (높을수록 더 많은 심볼 표시)")]
        public Slider visibleRangeSlider;
        
        [Tooltip("기본 심볼 간격 (도)")]
        [Range(5f, 90f)]
        public float defaultSymbolSpacing = 30f;
        
        [Tooltip("기본 가시 범위 (도)")]
        [Range(30f, 180f)]
        public float defaultVisibleRange = 90f;
        
        [Header("UI 설정")]
        [Tooltip("가위 버튼")]
        public Button scissorsButton;
        
        [Tooltip("바위 버튼")]
        public Button rockButton;
        
        [Tooltip("보 버튼")]
        public Button paperButton;
        
        [Tooltip("결과 텍스트")]
        public Text resultText;
        
        [Tooltip("심볼 간격 텍스트")]
        public Text symbolSpacingText;
        
        [Tooltip("가시 범위 텍스트")]
        public Text visibleRangeText;
        
        // 현재 활성화된 릴 개수
        private int activeReelCount = 1;
        
        // 현재 남은 릴 개수
        private int remainingReels = 0;
        
        // 플레이어 선택
        private RSPChoice playerChoice = RSPChoice.Rock;
        
        // 적 선택
        private RSPChoice enemyChoice = RSPChoice.Rock;
        
        // RSP 시스템 참조
        private RSPSystem rspSystem;
        
        // 컴퓨터의 선택이 결정되었는지 여부
        private bool isEnemyChoiceDetermined = false;
        
        // 현재 게임 진행 중인지 여부
        private bool isGameInProgress = false;
        
        private void Awake()
        {
            // RSP 시스템 참조 가져오기
            rspSystem = GetComponent<RSPSystem>();
            if (rspSystem == null)
            {
                rspSystem = gameObject.AddComponent<RSPSystem>();
            }
            
            // 릴이 없으면 경고 로그
            if (leftReel == null || middleReel == null || rightReel == null)
            {
                Debug.LogWarning("일부 슬롯머신 릴이 설정되지 않았습니다!");
            }
            
            // 초기 릴 상태 설정
            InitializeReels();
            
            // 버튼 이벤트 설정
            SetupButtons();
            
            // 슬라이더 초기화
            InitializeSliders();
        }
        
        /// <summary>
        /// 슬라이더 초기화
        /// </summary>
        private void InitializeSliders()
        {
            // 심볼 간격 슬라이더 설정
            if (symbolSpacingSlider != null)
            {
                symbolSpacingSlider.minValue = 5f;
                symbolSpacingSlider.maxValue = 90f;
                symbolSpacingSlider.value = defaultSymbolSpacing;
                
                // 이벤트 연결
                symbolSpacingSlider.onValueChanged.AddListener(OnSymbolSpacingChanged);
                
                // 초기 텍스트 업데이트
                UpdateSymbolSpacingText(defaultSymbolSpacing);
            }
            
            // 가시 범위 슬라이더 설정
            if (visibleRangeSlider != null)
            {
                visibleRangeSlider.minValue = 30f;
                visibleRangeSlider.maxValue = 180f;
                visibleRangeSlider.value = defaultVisibleRange;
                
                // 이벤트 연결
                visibleRangeSlider.onValueChanged.AddListener(OnVisibleRangeChanged);
                
                // 초기 텍스트 업데이트
                UpdateVisibleRangeText(defaultVisibleRange);
            }
            
            // 초기 값 적용
            ApplySymbolSettingsToAllReels();
        }
        
        /// <summary>
        /// 심볼 간격 슬라이더 변경 처리
        /// </summary>
        private void OnSymbolSpacingChanged(float value)
        {
            UpdateSymbolSpacingText(value);
            
            // 게임 중이 아닐 때만 적용
            if (!isGameInProgress)
            {
                ApplySymbolSettingsToAllReels();
            }
        }
        
        /// <summary>
        /// 가시 범위 슬라이더 변경 처리
        /// </summary>
        private void OnVisibleRangeChanged(float value)
        {
            UpdateVisibleRangeText(value);
            
            // 게임 중이 아닐 때만 적용
            if (!isGameInProgress)
            {
                ApplySymbolSettingsToAllReels();
            }
        }
        
        /// <summary>
        /// 심볼 간격 텍스트 업데이트
        /// </summary>
        private void UpdateSymbolSpacingText(float value)
        {
            if (symbolSpacingText != null)
            {
                symbolSpacingText.text = $"심볼 간격: {value:F1}° ({Mathf.RoundToInt(360f / value)}개)";
            }
        }
        
        /// <summary>
        /// 가시 범위 텍스트 업데이트
        /// </summary>
        private void UpdateVisibleRangeText(float value)
        {
            if (visibleRangeText != null)
            {
                visibleRangeText.text = $"가시 범위: {value:F1}° (약 {Mathf.RoundToInt(value / (symbolSpacingSlider?.value ?? defaultSymbolSpacing))}개 표시)";
            }
        }
        
        /// <summary>
        /// 모든 릴에 심볼 설정 적용
        /// </summary>
        private void ApplySymbolSettingsToAllReels()
        {
            float symbolSpacing = symbolSpacingSlider != null ? symbolSpacingSlider.value : defaultSymbolSpacing;
            float visibleRange = visibleRangeSlider != null ? visibleRangeSlider.value : defaultVisibleRange;
            
            // 각 릴에 설정 적용
            ApplySymbolSettingsToReel(leftReel, symbolSpacing, visibleRange);
            ApplySymbolSettingsToReel(middleReel, symbolSpacing, visibleRange);
            ApplySymbolSettingsToReel(rightReel, symbolSpacing, visibleRange);
        }
        
        /// <summary>
        /// 단일 릴에 심볼 설정 적용
        /// </summary>
        private void ApplySymbolSettingsToReel(SlotMachine reel, float symbolSpacing, float visibleRange)
        {
            if (reel != null)
            {
                reel.symbolAngleSpacing = symbolSpacing;
                reel.visibleRange = visibleRange;
                reel.RecreateSymbols();
            }
        }
        
        /// <summary>
        /// 초기 릴 상태 설정
        /// </summary>
        private void InitializeReels()
        {
            // 기본적으로 모든 릴 비활성화
            if (leftReel != null) leftReel.gameObject.SetActive(false);
            if (middleReel != null) middleReel.gameObject.SetActive(false);
            if (rightReel != null) rightReel.gameObject.SetActive(false);
            
            // 액티브 릴 개수 1개로 시작
            SetActiveReelCount(1);
        }
        
        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtons()
        {
            if (scissorsButton != null)
            {
                scissorsButton.onClick.AddListener(() => {
                    MakePlayerChoice(RSPChoice.Scissors);
                });
            }
            
            if (rockButton != null)
            {
                rockButton.onClick.AddListener(() => {
                    MakePlayerChoice(RSPChoice.Rock);
                });
            }
            
            if (paperButton != null)
            {
                paperButton.onClick.AddListener(() => {
                    MakePlayerChoice(RSPChoice.Paper);
                });
            }
        }
        
        /// <summary>
        /// 플레이어 선택 처리
        /// </summary>
        public void MakePlayerChoice(RSPChoice choice)
        {
            if (!isGameInProgress) return;
            
            playerChoice = choice;
            
            // 선택 결과 계산
            CalculateResult();
            
            // 버튼 비활성화
            SetButtonsInteractable(false);
        }
        
        /// <summary>
        /// 키보드 입력 처리 (RSPSystem과 통합)
        /// </summary>
        private void Update()
        {
            if (!isGameInProgress) return;
            
            // 키보드 입력 처리
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                MakePlayerChoice(RSPChoice.Scissors);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                MakePlayerChoice(RSPChoice.Rock);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                MakePlayerChoice(RSPChoice.Paper);
            }
        }
        
        /// <summary>
        /// 가위바위보 스택에 따라 활성화할 릴 개수 설정
        /// </summary>
        public void SetActiveReelCount(int count)
        {
            // 유효한 범위로 조정 (1-3)
            activeReelCount = Mathf.Clamp(count, 1, 3);
            
            // 왼쪽 릴은 항상 활성화
            if (leftReel != null) leftReel.gameObject.SetActive(true);
            
            // 중앙 릴은 카운트가 2 이상일 때 활성화
            if (middleReel != null) middleReel.gameObject.SetActive(activeReelCount >= 2);
            
            // 오른쪽 릴은 카운트가 3일 때만 활성화
            if (rightReel != null) rightReel.gameObject.SetActive(activeReelCount == 3);
            
            // 심볼 설정 적용
            ApplySymbolSettingsToAllReels();
            
            Debug.Log($"활성화된 릴 개수: {activeReelCount}");
        }
        
        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (isGameInProgress) return;
            
            isGameInProgress = true;
            remainingReels = activeReelCount;
            isEnemyChoiceDetermined = false;
            
            // 결과 텍스트 초기화
            if (resultText != null)
            {
                resultText.text = "가위바위보 중...";
            }
            
            // 버튼 활성화
            SetButtonsInteractable(true);
            
            // 슬라이더 비활성화
            SetSlidersInteractable(false);
            
            // 모든 활성화된 릴 회전 시작
            if (leftReel != null && leftReel.gameObject.activeSelf)
            {
                leftReel.StartSpin();
            }
            
            if (middleReel != null && middleReel.gameObject.activeSelf)
            {
                middleReel.StartSpin();
            }
            
            if (rightReel != null && rightReel.gameObject.activeSelf)
            {
                rightReel.StartSpin();
            }
            
            Debug.Log("가위바위보 게임 시작");
        }
        
        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        private void SetButtonsInteractable(bool interactable)
        {
            if (scissorsButton != null) scissorsButton.interactable = interactable;
            if (rockButton != null) rockButton.interactable = interactable;
            if (paperButton != null) paperButton.interactable = interactable;
        }
        
        /// <summary>
        /// 슬라이더 활성화/비활성화
        /// </summary>
        private void SetSlidersInteractable(bool interactable)
        {
            if (symbolSpacingSlider != null) symbolSpacingSlider.interactable = interactable;
            if (visibleRangeSlider != null) visibleRangeSlider.interactable = interactable;
        }
        
        /// <summary>
        /// 적의 선택 결정 (랜덤 또는 지정된 값)
        /// </summary>
        private void DetermineEnemyChoice()
        {
            if (isEnemyChoiceDetermined) return;
            
            enemyChoice = rspSystem.GetRandomRSPChoice();
            isEnemyChoiceDetermined = true;
            
            Debug.Log($"적의 선택: {enemyChoice}");
        }
        
        /// <summary>
        /// 가위바위보 결과 계산 및 처리
        /// </summary>
        private void CalculateResult()
        {
            // 적의 선택 결정
            DetermineEnemyChoice();
            
            // 결과 계산
            RSPResult result = rspSystem.RSPCalculate(playerChoice, enemyChoice);
            
            // 결과에 따른 처리
            switch (result)
            {
                case RSPResult.Win:
                    if (resultText != null) resultText.text = "승리!";
                    StopReelsBasedOnResult(true);
                    break;
                
                case RSPResult.Lose:
                    if (resultText != null) resultText.text = "패배...";
                    StopReelsBasedOnResult(false);
                    break;
                
                case RSPResult.Draw:
                    if (resultText != null) resultText.text = "무승부.";
                    StopReelsBasedOnResult(false);
                    break;
            }
            
            Debug.Log($"가위바위보 결과: {result}");
        }
        
        /// <summary>
        /// 결과에 따라 릴 정지
        /// </summary>
        private void StopReelsBasedOnResult(bool isWin)
        {
            if (isWin)
            {
                // 승리 시 오른쪽 릴부터 정지
                if (activeReelCount == 3 && rightReel != null && rightReel.gameObject.activeSelf)
                {
                    StopReel(rightReel, GetSymbolIndexForChoice(enemyChoice));
                    remainingReels--;
                }
                else if (activeReelCount >= 2 && middleReel != null && middleReel.gameObject.activeSelf)
                {
                    StopReel(middleReel, GetSymbolIndexForChoice(enemyChoice));
                    remainingReels--;
                }
                else if (leftReel != null && leftReel.gameObject.activeSelf)
                {
                    StopReel(leftReel, GetSymbolIndexForChoice(enemyChoice));
                    remainingReels--;
                }
                
                // 모든 릴이 정지되었는지 확인
                if (remainingReels <= 0)
                {
                    // 게임 종료
                    GameCompleted();
                }
                else
                {
                    // 다음 라운드 준비
                    PrepareNextRound();
                }
            }
            else
            {
                // 패배 또는 무승부 시 모든 릴 계속 회전
                PrepareNextRound();
            }
        }
        
        /// <summary>
        /// 다음 라운드 준비
        /// </summary>
        private void PrepareNextRound()
        {
            // 버튼 다시 활성화
            SetButtonsInteractable(true);
            isEnemyChoiceDetermined = false;
            
            if (resultText != null)
            {
                resultText.text = "다시 선택하세요...";
            }
        }
        
        /// <summary>
        /// 게임 완료 처리
        /// </summary>
        private void GameCompleted()
        {
            isGameInProgress = false;
            
            if (resultText != null)
            {
                resultText.text = "모든 릴 정지! 성공!";
            }
            
            // 슬라이더 다시 활성화
            SetSlidersInteractable(true);
            
            Debug.Log("가위바위보 게임 완료");
            
            // 여기에 게임 종료 후 처리 추가 가능
        }
        
        /// <summary>
        /// 선택에 따른 심볼 인덱스 반환
        /// </summary>
        private int GetSymbolIndexForChoice(RSPChoice choice)
        {
            switch (choice)
            {
                case RSPChoice.Scissors:
                    return scissorsSymbolIndex;
                case RSPChoice.Rock:
                    return rockSymbolIndex;
                case RSPChoice.Paper:
                    return paperSymbolIndex;
                default:
                    return 0;
            }
        }
        
        /// <summary>
        /// 특정 릴 정지
        /// </summary>
        private void StopReel(SlotMachine reel, int symbolIndex)
        {
            if (reel == null) return;
            
            reel.SetResult(symbolIndex);
            
            // 릴 정지 이벤트 리스너 설정
            reel.OnSpinComplete += OnReelStopped;
        }
        
        /// <summary>
        /// 릴 정지 완료 이벤트 핸들러
        /// </summary>
        private void OnReelStopped()
        {
            Debug.Log("릴 정지 완료");
        }
        
        /// <summary>
        /// 심볼 간격 직접 적용 (편의 함수)
        /// </summary>
        [ContextMenu("Apply Min Symbol Spacing")]
        public void ApplyMinSymbolSpacing()
        {
            if (symbolSpacingSlider != null)
            {
                symbolSpacingSlider.value = symbolSpacingSlider.minValue;
            }
            else
            {
                ApplySymbolSettingsToAllReels(5f, defaultVisibleRange);
            }
        }
        
        /// <summary>
        /// 심볼 간격 직접 적용 (편의 함수)
        /// </summary>
        [ContextMenu("Apply Max Symbol Spacing")]
        public void ApplyMaxSymbolSpacing()
        {
            if (symbolSpacingSlider != null)
            {
                symbolSpacingSlider.value = symbolSpacingSlider.maxValue;
            }
            else
            {
                ApplySymbolSettingsToAllReels(90f, defaultVisibleRange);
            }
        }
        
        /// <summary>
        /// 심볼 설정 직접 적용
        /// </summary>
        private void ApplySymbolSettingsToAllReels(float symbolSpacing, float visibleRange)
        {
            ApplySymbolSettingsToReel(leftReel, symbolSpacing, visibleRange);
            ApplySymbolSettingsToReel(middleReel, symbolSpacing, visibleRange);
            ApplySymbolSettingsToReel(rightReel, symbolSpacing, visibleRange);
            
            // 텍스트 업데이트
            UpdateSymbolSpacingText(symbolSpacing);
            UpdateVisibleRangeText(visibleRange);
        }
        
        /// <summary>
        /// 게임 종료 및 초기화
        /// </summary>
        [ContextMenu("Reset Game")]
        public void ResetGame()
        {
            isGameInProgress = false;
            
            // 슬라이더 활성화
            SetSlidersInteractable(true);
            
            // 릴 회전 중지
            if (leftReel != null && leftReel.gameObject.activeSelf)
            {
                leftReel.StopAllCoroutines();
            }
            
            if (middleReel != null && middleReel.gameObject.activeSelf)
            {
                middleReel.StopAllCoroutines();
            }
            
            if (rightReel != null && rightReel.gameObject.activeSelf)
            {
                rightReel.StopAllCoroutines();
            }
            
            // 릴 초기화
            InitializeReels();
            
            // UI 초기화
            if (resultText != null)
            {
                resultText.text = "게임 초기화됨";
            }
            
            SetButtonsInteractable(true);
        }
    }
} 
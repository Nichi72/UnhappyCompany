using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnhappyCompany
{
    /// <summary>
    /// 가위바위보 메커니즘을 가진 슬롯머신 구현
    /// 스택 수에 따라 1~3개의 릴을 관리합니다.
    /// </summary>
    public class RSPSlotMachine : MonoBehaviour
    {
        public EnemyAIRSP enemyAIRSP;
        public RSPSlotMachineConnector connector;
        [Header("릴 설정")]
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

        [Header("테스트 설정")]
        [Tooltip("테스트용 스택 수 (1-3)")]
        [Range(1, 3)]
        public int testStackCount = 1;

        // 현재 활성화된 릴 수 (스택 수에 따라 결정)
        private int activeReelCount;

        // 각 릴의 상태 (회전 중, 정지)
        private bool[] reelStates = new bool[3]; // true = 회전 중, false = 정지

        // 현재 선택 중인 릴 인덱스 (0 = 왼쪽, 1 = 중앙, 2 = 오른쪽)
        private int currentReelIndex;

        // 가위바위보 결과 열거형
        public enum RSPResult { Win, Lose, Draw }

        // 가위바위보 선택 열거형
        public enum RSPChoice { Rock, Scissors, Paper }

        // 게임 상태 열거형
        private enum GameState { Ready, Spinning, WaitingForInput, ShowingResult }
        private GameState currentState;
        [SerializeField] private string currentStateNameForDebug;

        // 현재 AI의 선택 (랜덤)
        private RSPChoice aiChoice;

        private void Start()
        {
            InitializeReels();
        }

        private void Update()
        {
            currentStateNameForDebug = currentState.ToString();
            // 상태에 따른 업데이트 로직
            switch (currentState)
            {
                case GameState.Ready:
                    // Ready 상태에서는 별도 업데이트 불필요
                    break;

                case GameState.Spinning:
                    // 모든 활성화된 릴이 회전 중인지 확인
                    CheckAllReelsSpinning();
                    break;

                case GameState.WaitingForInput:
                    // 플레이어 입력 처리
                    HandlePlayerInput();
                    break;

                case GameState.ShowingResult:
                    // 결과 표시 중에는 별도 업데이트 불필요
                    break;
            }
        }

        /// <summary>
        /// 릴 초기화
        /// </summary>
        private void InitializeReels()
        {
            // 초기 상태 설정
            currentState = GameState.Ready;
            currentReelIndex = 0;

            // 모든 릴 상태 초기화 (정지 상태)
            for (int i = 0; i < reelStates.Length; i++)
            {
                reelStates[i] = false;
            }

            // 릴 참조 확인
            if (leftReel == null)
            {
                Debug.LogError("왼쪽 릴이 설정되지 않았습니다!");
            }

            if (middleReel == null)
            {
                Debug.LogWarning("중앙 릴이 설정되지 않았습니다!");
            }

            if (rightReel == null)
            {
                Debug.LogWarning("오른쪽 릴이 설정되지 않았습니다!");
            }

            // 릴 이벤트 설정
            SetupReelEvents();
        }

        /// <summary>
        /// 릴 이벤트 설정
        /// </summary>
        private void SetupReelEvents()
        {
            // 각 릴의 회전 완료 이벤트 등록
            if (leftReel != null)
            {
                leftReel.OnSpinComplete += OnLeftReelSpinComplete;
            }

            if (middleReel != null)
            {
                middleReel.OnSpinComplete += OnMiddleReelSpinComplete;
            }

            if (rightReel != null)
            {
                rightReel.OnSpinComplete += OnRightReelSpinComplete;
            }
        }

        /// <summary>
        /// 모든 릴의 회전이 계속되고 있는지 확인
        /// </summary>
        private void CheckAllReelsSpinning()
        {
            // 현재 활성화된 릴만 확인하도록 수정
            if (reelStates[currentReelIndex])
            {
                currentState = GameState.WaitingForInput;
                Debug.Log($"릴 {currentReelIndex}이(가) 회전 중입니다. 선택을 기다립니다.");
                
                // 여기서 AI의 선택을 미리 결정
                GenerateAIChoice();
            }
        }

        /// <summary>
        /// AI의 선택 생성 (랜덤)
        /// </summary>
        private void GenerateAIChoice()
        {
            int choice = Random.Range(0, 3);
            aiChoice = (RSPChoice)choice;
            Debug.Log($"AI가 선택한 것: {aiChoice}");
        }

        /// <summary>
        /// 플레이어 입력 처리
        /// </summary>
        private void HandlePlayerInput()
        {
            if (currentState != GameState.WaitingForInput) 
            {
                Debug.Log("플레이어 입력 처리 불가능");
                return;
            }
            

            RSPChoice playerChoice = RSPChoice.Rock;
            bool inputDetected = false;

            // 키보드 입력 감지
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerChoice = RSPChoice.Scissors;
                inputDetected = true;
                Debug.Log("플레이어 선택: 가위");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerChoice = RSPChoice.Rock;
                inputDetected = true;
                Debug.Log("플레이어 선택: 바위");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerChoice = RSPChoice.Paper;
                inputDetected = true;
                Debug.Log("플레이어 선택: 보");
            }

            if (inputDetected)
            {
                // 결과 계산 및 처리
                ProcessResult(playerChoice);
            }
        }

        /// <summary>
        /// 결과 처리
        /// </summary>
        private void ProcessResult(RSPChoice playerChoice)
        {
            // 가위바위보 결과 계산
            RSPResult result = CalculateRSPResult(playerChoice, aiChoice);
            
            // 상태를 결과 표시로 변경
            currentState = GameState.ShowingResult;
            
            // 릴 결과 설정
            SetReelResult(currentReelIndex, playerChoice, result);
            
            Debug.Log($"결과: {result}");
        }

        /// <summary>
        /// 가위바위보 결과 계산
        /// </summary>
        public RSPResult CalculateRSPResult(RSPChoice playerChoice, RSPChoice aiChoice)
        {
            if (playerChoice == aiChoice)
            {
                return RSPResult.Draw;
            }

            if ((playerChoice == RSPChoice.Rock && aiChoice == RSPChoice.Scissors) ||
                (playerChoice == RSPChoice.Scissors && aiChoice == RSPChoice.Paper) ||
                (playerChoice == RSPChoice.Paper && aiChoice == RSPChoice.Rock))
            {
                return RSPResult.Win;
            }
            
            return RSPResult.Lose;
        }

        /// <summary>
        /// 릴 결과 설정
        /// </summary>
        private void SetReelResult(int reelIndex, RSPChoice playerChoice, RSPResult result)
        {
            int symbolIndex = GetSymbolIndexForChoice(playerChoice);
            
            // 해당 릴의 결과 설정
            SlotMachine targetReel = GetReelByIndex(reelIndex);
            if (targetReel != null)
            {
                targetReel.SetResult(symbolIndex);
                
                // 릴 상태 업데이트
                reelStates[reelIndex] = false;
            }

            // 결과에 따른 후속 처리
            StartCoroutine(ProcessAfterResult(result));
        }

        /// <summary>
        /// 선택에 해당하는 심볼 인덱스 반환
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
                    return rockSymbolIndex;
            }
        }

        /// <summary>
        /// 인덱스로 릴 가져오기
        /// </summary>
        private SlotMachine GetReelByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return leftReel;
                case 1:
                    return middleReel;
                case 2:
                    return rightReel;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 결과 후 처리
        /// </summary>
        private IEnumerator ProcessAfterResult(RSPResult result)
        {
            // 결과 확인 시간 대기
            yield return new WaitForSeconds(1.5f);
            if (result == RSPResult.Win)
            {
                // 승리 시 오른쪽 릴부터 정지 (이미 정지된 상태)
                Debug.Log($"승리! 릴 {currentReelIndex}이(가) 정지됩니다.");
                
                // 릴 승리 이벤트 호출 시도
                Transform parent = transform.parent;
                if (parent != null)
                {
                    if (connector != null)
                    {
                        Debug.Log($"RSPSlotMachine: 릴 {currentReelIndex} 승리 이벤트 발생");
                        connector.OnReelWon(currentReelIndex);
                        enemyAIRSP.DecrementStack();
                    }
                }
                
                // 다음 릴로 이동 (왼쪽 방향으로)
                if (currentReelIndex > 0)
                {
                    currentReelIndex--;
                    currentState = GameState.WaitingForInput;
                    
                    // 다음 릴에 대한 AI 선택 생성
                    GenerateAIChoice();
                    Debug.Log($"다음 릴로 이동: {currentReelIndex}");
                }
                else
                {
                    // 모든 릴 승리 (게임 완료)
                    Debug.Log("마지막 릴까지 승리! 게임 완료 처리 진행");
                    GameComplete();
                }
            }
            else
            {
                // 패배나 무승부 시 현재 릴만 다시 시작
                Debug.Log(result == RSPResult.Lose ? $"릴 {currentReelIndex} 패배! 다시 시도합니다." : $"릴 {currentReelIndex} 무승부! 다시 시도합니다.");
                
                // 현재 릴 다시 시작
                SlotMachine currentReel = GetReelByIndex(currentReelIndex);
                if (currentReel != null)
                {
                    currentReel.StartSpin();
                    reelStates[currentReelIndex] = true;
                }
                
                // 상태 업데이트
                currentState = GameState.Spinning;
                
                // 새로운 AI 선택 생성
                GenerateAIChoice();
            }
        }

        /// <summary>
        /// 게임 완료 처리
        /// </summary>
        private void GameComplete()
        {
            Debug.Log("모든 릴에서 승리! 게임 완료!");
            currentState = GameState.Ready;
            var holdingState = enemyAIRSP.currentState as RSPHoldingState;
            if(holdingState != null)
            {
                holdingState.gameInProgress = false;
            }
            else
            {
                Debug.LogWarning("RSPHoldingState가 없습니다!");
            }
            enemyAIRSP.ChangeState(new RSPPatrolState(enemyAIRSP));
            
            // 게임 완료 이벤트 호출
            // SendMessageUpwards를 사용하여 부모 객체의 OnReelWon 메서드 호출
            // 여기서는 모든 릴이 승리한 상태이므로 0번 릴 인덱스를 전달 (가장 왼쪽 릴)
            // Debug.Log("RSPSlotMachine: 게임 완료 이벤트 전송 시도");
            // Transform parent = transform.parent;
            // if (parent != null)
            // {
            //     RSPSlotMachineConnector connector = parent.GetComponent<RSPSlotMachineConnector>();
            //     if (connector != null)
            //     {
            //         Debug.Log("RSPSlotMachine: RSPSlotMachineConnector 찾음, OnReelWon 호출");
            //         connector.OnReelWon(0); // 0번 릴 (최종 릴)의 승리를 알림
            //     }
            //     else
            //     {
            //         Debug.LogWarning("RSPSlotMachine: 부모 객체에 RSPSlotMachineConnector가 없습니다!");
            //     }
            // }
            // else
            // {
            //     Debug.LogWarning("RSPSlotMachine: 부모 객체가 없습니다!");
            // }
        }

        /// <summary>
        /// 가위바위보 게임 시작
        /// </summary>
        [ContextMenu("Start RSP Game")]
        public void StartRSPGame()
        {
            StartRSPGameWithStacks(testStackCount);
        }

        /// <summary>
        /// 지정된 스택 수로 가위바위보 게임 시작
        /// </summary>
        public void StartRSPGameWithStacks(int stackCount)
        {
            // 스택 수 범위 제한 (1-3)
            activeReelCount = Mathf.Clamp(stackCount, 1, 3);
            
            // 시작 릴 인덱스 설정 (항상 가장 오른쪽 활성화된 릴부터 시작)
            currentReelIndex = activeReelCount - 1;
            
            // 초기 상태 설정
            currentState = GameState.Spinning;
            
            Debug.Log($"가위바위보 게임 시작! 스택 수: {activeReelCount}");
            
            // 활성화된 릴 시작
            for (int i = 0; i < activeReelCount; i++)
            {
                SlotMachine reel = GetReelByIndex(i);
                if (reel != null)
                {
                    reel.StartSpin();
                    reelStates[i] = true;
                }
            }
        }

        // 릴 회전 완료 이벤트 핸들러
        private void OnLeftReelSpinComplete()
        {
            Debug.Log("왼쪽 릴 회전 완료");
            reelStates[0] = false;
        }

        private void OnMiddleReelSpinComplete()
        {
            Debug.Log("중앙 릴 회전 완료");
            reelStates[1] = false;
        }

        private void OnRightReelSpinComplete()
        {
            Debug.Log("오른쪽 릴 회전 완료");
            reelStates[2] = false;
        }

        private void OnDestroy()
        {
            // 이벤트 등록 해제
            if (leftReel != null)
            {
                leftReel.OnSpinComplete -= OnLeftReelSpinComplete;
            }

            if (middleReel != null)
            {
                middleReel.OnSpinComplete -= OnMiddleReelSpinComplete;
            }

            if (rightReel != null)
            {
                rightReel.OnSpinComplete -= OnRightReelSpinComplete;
            }
        }
    }
} 
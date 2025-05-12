using System;
using System.Collections;
using UnityEngine;

namespace UnhappyCompany
{
    /// <summary>
    /// RSPSlotMachine과 RSPSystem 사이의 연결을 관리하는 컴포넌트
    /// 2단계에서 EnemyAIRSP와 연동하기 위한 준비 클래스입니다.
    /// </summary>
    public class RSPSlotMachineConnector : MonoBehaviour
    {
        [Header("슬롯머신 참조")]
        [Tooltip("연결할 RSP 슬롯머신")]
        public RSPSlotMachine rspSlotMachine;
        
        [Header("이벤트 콜백")]
        [Tooltip("게임 완료 시 호출될 이벤트")]
        public Action OnGameComplete;
        
        [Tooltip("게임 결과에 따라 호출될 이벤트 (승리 횟수)")]
        public Action<int> OnGameResult;
        
        // 현재 스택 수
        private int currentStackCount = 0;
        
        // 승리한 릴 수
        private int wonReelCount = 0;
        
        private void Awake()
        {
            // 슬롯머신 참조 확인
            if (rspSlotMachine == null)
            {
                rspSlotMachine = GetComponent<RSPSlotMachine>();
                
                if (rspSlotMachine == null)
                {
                    Debug.LogError("RSPSlotMachine 컴포넌트가 없습니다!");
                }
            }
        }
        
        /// <summary>
        /// 스택 수로 게임 시작
        /// </summary>
        public void StartGame(int stackCount)
        {
            if (rspSlotMachine == null) return;
            
            currentStackCount = Mathf.Clamp(stackCount, 1, 3);
            wonReelCount = 0;
            
            // 슬롯머신 게임 시작
            rspSlotMachine.StartRSPGameWithStacks(currentStackCount);
            
            Debug.Log($"RSP 슬롯머신 게임 시작 (스택: {currentStackCount})");
        }
        
        /// <summary>
        /// 테스트용 게임 시작
        /// </summary>
        [ContextMenu("Start Test Game")]
        public void StartTestGame()
        {
            // 테스트 게임 시작 (3스택)
            StartGame(3);
        }
        
        /// <summary>
        /// 릴 승리 처리
        /// </summary>
        public void OnReelWon(int reelIndex)
        {
            wonReelCount++;
            
            Debug.Log($"RSPSlotMachineConnector: 릴 {reelIndex} 승리! (누적: {wonReelCount}/{currentStackCount})");
            
            // 모든 릴에서 승리했는지 확인
            if (wonReelCount >= currentStackCount)
            {
                Debug.Log($"RSPSlotMachineConnector: 모든 릴 승리 완료! ({wonReelCount}/{currentStackCount}) 게임 완료 처리");
                GameComplete();
            }
        }
        
        /// <summary>
        /// 게임 완료 처리
        /// </summary>
        private void GameComplete()
        {
            Debug.Log("RSPSlotMachineConnector: 게임 완료!");
            
            // 게임 완료 이벤트 발생
            if (OnGameComplete != null)
            {
                Debug.Log("RSPSlotMachineConnector: OnGameComplete 이벤트 발생");
                OnGameComplete.Invoke();
            }
            else
            {
                Debug.LogWarning("RSPSlotMachineConnector: OnGameComplete 이벤트가 null입니다!");
            }
            
            // 게임 결과 이벤트 발생 (승리한 릴 수)
            if (OnGameResult != null)
            {
                Debug.Log($"RSPSlotMachineConnector: OnGameResult 이벤트 발생 (승리 릴 수: {wonReelCount})");
                OnGameResult.Invoke(wonReelCount);
            }
        }
        
        /// <summary>
        /// RSPSystem 생성
        /// </summary>
        /// <returns>생성된 RSPSystem 인스턴스</returns>
        public RSPChoice GetAIChoice()
        {
            // 랜덤 선택 (0 = 바위, 1 = 가위, 2 = 보)
            int randomChoice = UnityEngine.Random.Range(0, 3);
            
            // UnhappyCompany 네임스페이스의 RSPChoice와 외부 RSPChoice 간 변환
            switch (randomChoice)
            {
                case 0:
                    return (RSPChoice)0; // Rock
                case 1:
                    return (RSPChoice)1; // Scissors
                case 2:
                    return (RSPChoice)2; // Paper
                default:
                    return (RSPChoice)0; // 기본값
            }
        }
        
        /// <summary>
        /// 결과 계산
        /// </summary>
        public RSPSlotMachine.RSPResult CalculateResult(RSPSlotMachine.RSPChoice playerChoice, RSPChoice enemyChoice)
        {
            // RSPChoice 타입 변환
            RSPSlotMachine.RSPChoice aiChoice = ConvertRSPChoice(enemyChoice);
            
            // 결과 계산 및 반환
            return rspSlotMachine.CalculateRSPResult(playerChoice, aiChoice);
        }
        
        /// <summary>
        /// RSPChoice 타입 변환
        /// </summary>
        private RSPSlotMachine.RSPChoice ConvertRSPChoice(RSPChoice choice)
        {
            switch (choice)
            {
                case (RSPChoice)0: // Rock
                    return RSPSlotMachine.RSPChoice.Rock;
                case (RSPChoice)1: // Scissors
                    return RSPSlotMachine.RSPChoice.Scissors;
                case (RSPChoice)2: // Paper
                    return RSPSlotMachine.RSPChoice.Paper;
                default:
                    return RSPSlotMachine.RSPChoice.Rock;
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 참조 해제
            OnGameComplete = null;
            OnGameResult = null;
        }
    }
} 
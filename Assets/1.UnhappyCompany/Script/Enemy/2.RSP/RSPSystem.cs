using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 가위바위보 게임 시스템 (게임 로직만 담당)
/// </summary>
public class RSPSystem : MonoBehaviour
{
    [SerializeField] private RSPMedalGame medalGame; // Inspector에서 참조 설정
    
    // 가위바위보 결과 확률 설정
    private float winProbability = 80f;
    private float drawProbability = 10f;
    private float loseProbability = 10f;
    
    private bool isRSPActive = false;
    private System.Action<RSPResult, int, bool> onGameComplete; // RSP 결과, 메달 게임 결과, 코인 부족 여부 콜백
    private List<GameObject> rspSymbols;
    public RSPUI rspUI;
    
    [Header("RSP AI 참조")]
    [Tooltip("RSP AI 컨트롤러 컴포넌트")]
    [SerializeField] private EnemyAIRSP rspAI;

    [Header("코인 설정")]
    [Tooltip("코인을 생성할 위치 Transform")]
    [SerializeField] private Transform coinSpawnTransform;
    
    [Tooltip("코인 생성 간격 (초)")]
    [SerializeField] private float coinSpawnInterval = 0.15f;
    
    [Tooltip("코인 배출 기본 힘 방향 (X, Y=up, Z=forward)")]
    [SerializeField] private Vector3 coinForceDirection = new Vector3(0f, 3f, 5f);
    
    [Tooltip("코인 배출 힘의 랜덤 범위 (각 축별 ±값)")]
    [SerializeField] private Vector3 coinForceRandomRange = new Vector3(0.5f, 1f, 1f);
    
    [Tooltip("코인 배출 시 재생할 사운드")]
    [SerializeField] private bool playCoinSpawnSound = true;
    
    private Player currentPlayer; // 현재 게임 중인 플레이어
    [SerializeField] private ItemData coinItemData; // 코인 ItemData

    
    void Start()
    {
        // medalGame 참조 확인
        if (medalGame == null)
        {
            Debug.LogWarning("RSPSystem: 메달 게임 참조가 설정되지 않았습니다.");
        }
        else
        {
            medalGame.Initialize();
        }
        
        // RSP AI 참조 확인
        if (rspAI == null)
        {
            Debug.LogError("RSPSystem: EnemyAIRSP 참조가 설정되지 않았습니다!");
        }
        
        // 확률 합계 검증
        ValidateProbabilities();
    }

    

    
    // Update is called once per frame
    void Update()
    {
        
    }

    // 확률 값 유효성 검사
    private void ValidateProbabilities()
    {
        float sum = winProbability + drawProbability + loseProbability;
        if (Mathf.Abs(sum - 100f) > 0.01f)
        {
            Debug.LogWarning($"RSP: 결과 확률의 총합이 100%가 아닙니다 (현재: {sum}%). 자동으로 조정됩니다.");
            
            // 비율 유지하면서 총합을 100%로 조정
            float scale = 100f / sum;
            winProbability *= scale;
            drawProbability *= scale;
            loseProbability *= scale;
        }
    }
    
    // 가위바위보 확률 설정 메서드 (외부에서 호출 가능)
    public void SetRSPProbabilities(float win, float draw, float lose)
    {
        winProbability = win;
        drawProbability = draw;
        loseProbability = lose;
        ValidateProbabilities();
    }
    
    // 설정된 확률에 따라 RSP 결과 결정
    public RSPResult GetWeightedRSPResult()
    {
        float random = Random.Range(0f, 100f);
        float cumulative = 0f;
        
        cumulative += winProbability;
        if (random <= cumulative) return RSPResult.Win;
        
        cumulative += drawProbability;
        if (random <= cumulative) return RSPResult.Draw;
        
        return RSPResult.Lose;
    }

    // 가위바위보 결과 계산
    public RSPResult RSPCalculate(RSPChoice playerChoice, RSPChoice enemyChoice)
    {
        if (playerChoice == enemyChoice)
        {
            return RSPResult.Draw;
        }

        if ((playerChoice == RSPChoice.Rock && enemyChoice == RSPChoice.Scissors) ||
            (playerChoice == RSPChoice.Scissors && enemyChoice == RSPChoice.Paper) ||
            (playerChoice == RSPChoice.Paper && enemyChoice == RSPChoice.Rock))
        {
            return RSPResult.Win;
        }
        return RSPResult.Lose;
    }

    // 확률에 따른 적 선택 생성
    public RSPChoice GetEnemyChoiceByWeight(RSPChoice playerChoice)
    {
        // 원하는 결과에 따라 적의 선택 결정
        RSPResult targetResult = GetWeightedRSPResult();
        
        switch(targetResult)
        {
            case RSPResult.Win:
                // 플레이어가 이기게 하는 적 선택
                switch(playerChoice)
                {
                    case RSPChoice.Rock: return RSPChoice.Scissors;     // 바위 > 가위
                    case RSPChoice.Scissors: return RSPChoice.Paper;    // 가위 > 보
                    case RSPChoice.Paper: return RSPChoice.Rock;        // 보 > 바위
                }
                break;
                
            case RSPResult.Draw:
                // 무승부 되는 선택
                return playerChoice;  // 같은 것 내면 무승부
                
            case RSPResult.Lose:
                // 플레이어가 지게 하는 적 선택
                switch(playerChoice)
                {
                    case RSPChoice.Rock: return RSPChoice.Paper;        // 바위 < 보
                    case RSPChoice.Scissors: return RSPChoice.Rock;     // 가위 < 바위
                    case RSPChoice.Paper: return RSPChoice.Scissors;    // 보 < 가위
                }
                break;
        }
        
        // 예외 처리
        return GetRandomRSPChoice();
    }

    // 기존 랜덤 선택 메서드 (필요시 사용)
    public RSPChoice GetRandomRSPChoice()
    {
        int randomValue = Random.Range(0, 3);
        switch (randomValue)
        {
            case 0:
                return RSPChoice.Rock;
            case 1:
                return RSPChoice.Scissors;
            case 2:
                return RSPChoice.Paper;
            default:
                return RSPChoice.Rock;
        }
    }

    // 전체 가위바위보 + 메달 게임 시작
    public void StartRSPWithMedalGame(System.Action<RSPResult, int, bool> callback, Player player)
    {
        // RSP가 이미 죽은 상태라면 게임 시작 불가
        if (rspAI != null && rspAI.IsRSPDead())
        {
            Debug.LogWarning("RSPSystem: RSP가 이미 죽어서 게임을 시작할 수 없습니다.");
            callback?.Invoke(RSPResult.Lose, 0, true);
            return;
        }
        
        onGameComplete = callback;
        currentPlayer = player;
        isRSPActive = true;
        
        // 코루틴 시작
        StartCoroutine(PlayRSPAndMedalGame());
    }

    // 가위바위보와 메달 게임 연동 코루틴
    private IEnumerator PlayRSPAndMedalGame()
    {
        // 0. 코인 확인 및 소모
        if (currentPlayer != null && currentPlayer.quickSlotSystem != null)
        {
            bool hasCoin = currentPlayer.quickSlotSystem.HasCoin();
            Debug.Log($"RSP: 코인 확인 결과 = {hasCoin}");
            
            if (!hasCoin)
            {
                Debug.LogWarning("RSP: 코인이 없어 게임을 시작할 수 없습니다!");
                Debug.Log($"RSP: 콜백 호출 전 - onGameComplete null 체크 = {(onGameComplete != null)}");
                isRSPActive = false;
                onGameComplete?.Invoke(RSPResult.Lose, 0, true); // 코인 없음을 알림 (hasNoCoin = true)
                Debug.Log("RSP: 코인 부족 콜백 호출 완료");
                yield break;
            }
            
            // 코인 소모
            int removedCoinValue = currentPlayer.quickSlotSystem.RemoveCoin();
            Debug.Log($"RSP: 코인 {removedCoinValue}개를 소모하여 게임을 시작합니다.");
        }
        else
        {
            Debug.LogWarning("RSP: 플레이어 참조가 없어 코인 시스템을 사용할 수 없습니다.");
        }
        
        // 1. 가위바위보 게임 진행
        RSPResult rspResult = RSPResult.Draw; // 기본값
        
        // 간단한 키 입력으로 가위바위보 결과 결정
        Debug.Log("RSP: 가위바위보 게임 시작 (0: 바위, 1: 가위, 2: 보)");
        
        bool rspCompleted = false;
        RSPChoice playerChoice = RSPChoice.Rock; // 기본값
        rspUI.PlayCenterRSPAnimation();
        
        while (!rspCompleted)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerChoice = RSPChoice.Rock;
                Debug.Log("사용자가 바위를 선택했습니다.");
                rspCompleted = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerChoice = RSPChoice.Scissors;
                Debug.Log("사용자가 가위를 선택했습니다.");
                rspCompleted = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerChoice = RSPChoice.Paper;
                Debug.Log("사용자가 보를 선택했습니다.");
                rspCompleted = true;
            }
            yield return null;
        }
        
        // 확률에 따른 적 선택 결정
        RSPChoice enemyChoice = GetEnemyChoiceByWeight(playerChoice);
        rspUI.StopCenterRSPAnimation(enemyChoice);
        Debug.Log($"적이 {enemyChoice}를 선택했습니다.");
        
        // 최종 결과 계산
        rspResult = RSPCalculate(playerChoice, enemyChoice);
        Debug.Log($"RSP 결과: {rspResult}");
        
        // 가위바위보 결과에 따른 UI 표시
        if (rspUI != null)
        {
            rspUI.ShowResult(rspResult);
        }
        
        // 가위바위보 결과에 따른 사운드 재생
        if (rspResult == RSPResult.Win)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWin, transform, 40f, "RSP Win");
        }
        else if (rspResult == RSPResult.Lose)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspLose, transform, 40f, "RSP Lose");
        }
        else if (rspResult == RSPResult.Draw)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspDraw, transform, 40f, "RSP Draw");
        }
        
        yield return new WaitForSeconds(1.0f); // 사운드 재생 시간
        
        // 2. 메달 게임은 승리한 경우에만 시작 (무승부는 스킵)
        int medalResult = 0;
        
        if (rspResult == RSPResult.Win && medalGame != null)
        {
            Debug.Log("가위바위보에서 승리하여 메달 게임을 시작합니다.");
            
            // 메달 게임 시작 및 결과 콜백 등록
            medalGame.StartGame();
            // 메달 게임 완료 대기
            yield return new WaitUntil(() => medalGame.IsGameActive());
            
            // 메달 게임 결과 가져오기
            medalResult = medalGame.GetLastResult();
            Debug.Log($"메달 게임 최종 결과: {medalResult}점");
            
            // 승리 시 리워드만큼 코인 생성 (순차적으로)
            if (medalResult > 0 && rspAI != null)
            {
                // 실제로 지급할 코인 개수 계산 (RSP가 가진 HP보다 많을 수 없음)
                int coinsToDeduct = Mathf.Min(medalResult, rspAI.GetCurrentCoinHP());
                
                Debug.Log($"RSPSystem: {coinsToDeduct}개의 코인을 순차적으로 배출합니다.");
                
                // 코인을 하나씩 배출하면서 실시간으로 HP 차감
                yield return StartCoroutine(SpawnRewardCoinsSequentially(coinsToDeduct));
                
                // 모든 코인 배출이 완료된 후 HP 체크 및 죽음 처리
                rspAI.CheckAndHandleDeath();
            }
        }
        else if (rspResult == RSPResult.Draw)
        {
            Debug.Log("무승부입니다. 메달 게임(룰렛) 없이 게임이 종료됩니다.");
            yield return new WaitForSeconds(1.0f); // 잠시 대기
        }
        else
        {
            Debug.Log("가위바위보에서 승리하지 못하여 메달 게임을 진행하지 않습니다.");
            yield return new WaitForSeconds(1.0f); // 잠시 대기
        }
        
        // 3. 전체 게임 완료 및 결과 반환
        isRSPActive = false;
        onGameComplete?.Invoke(rspResult, medalResult, false); // 정상적으로 게임이 진행된 경우 hasNoCoin = false
    }
    
    /// <summary>
    /// RSP가 코인을 순차적으로 생성하여 뱉습니다 (잭팟 효과)
    /// </summary>
    /// <param name="amount">생성할 코인 개수</param>
    private IEnumerator SpawnRewardCoinsSequentially(int amount)
    {
        if (coinItemData == null)
        {
            Debug.LogError("RSP: 코인 ItemData가 로드되지 않았습니다!");
            yield break;
        }
        
        // 코인 생성 위치 결정
        Vector3 spawnPos = coinSpawnTransform != null ? coinSpawnTransform.position : transform.position + Vector3.up * 2f;
        
        Debug.Log($"RSPSystem: {amount}개의 코인을 순차적으로 생성합니다.");
        
        for (int i = 0; i < amount; i++)
        {
            // 코인 생성 전에 RSP AI의 HP 차감
            if (rspAI != null)
            {
                rspAI.DecreaseCoinHP(1);
            }
            
            Debug.Log($"RSPSystem: 코인 배출 [{i + 1}/{amount}]");
            
            // 코인 생성 (ItemData의 prefab을 Instantiate)
            GameObject coinObj = Instantiate(coinItemData.prefab, spawnPos, Quaternion.identity);
            
            if (coinObj != null)
            {
                // 코인 배출 사운드 재생 (2단계)
                if (playCoinSpawnSound && AudioManager.instance != null && FMODEvents.instance != null)
                {
                    // 1단계: 코인 배출 소리
                    AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspCoinSpawn, coinObj.transform, 20f, "Coin Spawn");
                    
                    // 2단계: 0.2초 후 코인 떨어지는 소리 (코루틴으로 처리)
                    StartCoroutine(PlayCoinDropSound(coinObj.transform));
                }
                
                // 약간의 랜덤 방향으로 힘 추가 (뱉는 효과)
                Rigidbody rb = coinObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 기본 힘 방향 + 랜덤 범위 (로컬 좌표)
                    Vector3 localForce = new Vector3(
                        coinForceDirection.x + UnityEngine.Random.Range(-coinForceRandomRange.x, coinForceRandomRange.x),
                        coinForceDirection.y + UnityEngine.Random.Range(-coinForceRandomRange.y, coinForceRandomRange.y),
                        coinForceDirection.z + UnityEngine.Random.Range(-coinForceRandomRange.z, coinForceRandomRange.z)
                    );
                    
                    // coinSpawnTransform의 로컬 좌표계를 기준으로 월드 좌표로 변환
                    Transform spawnTransform = coinSpawnTransform != null ? coinSpawnTransform : transform;
                    Vector3 worldForce = spawnTransform.TransformDirection(localForce);
                    
                    rb.AddForce(worldForce, ForceMode.Impulse);
                    
                    // 약간의 회전도 추가
                    Vector3 randomTorque = new Vector3(
                        UnityEngine.Random.Range(-10f, 10f),
                        UnityEngine.Random.Range(-10f, 10f),
                        UnityEngine.Random.Range(-10f, 10f)
                    );
                    rb.AddTorque(randomTorque, ForceMode.Impulse);
                }
            }
            
            // 다음 코인 생성까지 대기
            yield return new WaitForSeconds(coinSpawnInterval);
        }
        
        Debug.Log($"RSPSystem: 코인 {amount}개 배출 완료!");
    }
    
    /// <summary>
    /// 코인 떨어지는 소리를 0.2초 후에 재생하는 코루틴
    /// </summary>
    private IEnumerator PlayCoinDropSound(Transform coinTransform)
    {
        // 0.2초 대기
        yield return new WaitForSeconds(0.2f);
        // 코인이 아직 존재하면 떨어지는 소리 재생
       AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspCoinDrop, coinTransform, 20f, "Coin Drop");
    }

    public void RSPAnimation()
    {
        // 가위바위보 애니메이션 재생
        // 메달 게임 애니메이션 재생
    }

    /// <summary>
    /// 게임 진행 중인지 확인
    /// </summary>
    public bool IsGameActive()
    {
        return isRSPActive;
    }
}

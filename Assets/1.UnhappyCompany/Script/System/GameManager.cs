using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public PlayerStatus playerStatus;
    public Player currentPlayer;
    public int totalGold = 10;
    public EGameState currentGameState = EGameState.None;
    public bool isPressedStartBtn = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        GameStart();
        StartCoroutine(CheckGameOver());

    }
    private void GameStart()
    {
        currentGameState = EGameState.Ready;
        OnChangeReady();
        StartCoroutine(GameStateFSM());
    }

    

    private IEnumerator GameStateFSM()
    {
        EGameState previousState = currentGameState;
        while (true)
        {
            // if (previousState != currentGameState)
            {
                switch (currentGameState)
                {
                    case EGameState.Ready:
                        // Ready 상태에서의 로직 처리
                        
                        // 예를 들어, 플레이어가 준비되면 Playing 상태로 전환
                        if (isPressedStartBtn)
                        {
                            OnChangePlaying();
                            // Debug.Log("게임 준비 완료");
                        }
                        break;

                    case EGameState.Playing:
                        // Playing 상태에서의 로직 처리
                        // Debug.Log("게임 진행 중");
                        // 게임 오버 조건을 체크하여 End 상태로 전환
                        if (TimeManager.instance.HasDayPassed)
                        {
                            OnChangeEnd();
                        }
                        break;

                    case EGameState.End:
                        // End 상태에서의 로직 처리
                        Debug.Log("게임 종료");
                        // UIManager.instance.gameOverImage.SetActive(true);
                        // 게임을 재시작하거나 메인 메뉴로 돌아가는 로직 추가 가능
                        if(true)
                        {
                            OnChangeReady();
                        }
                        break;
                }
                // previousState = currentGameState;
            }
            yield return null;
        }
    }
    private void OnChangeReady()
    {
        currentGameState = EGameState.Ready;
        TimeManager.instance.IsStop = true;
        CentralBatterySystem.Instance.isStop = true;
    }

    private void OnChangePlaying()
    {
        currentGameState = EGameState.Playing;
        Debug.Log("OnChangePlaying");
        CentralBatterySystem.Instance.isStop = false;
        // Playing 상태에서 호출되는 함수
        isPressedStartBtn = false;
        TimeManager.instance.IsStop = false;
        FadeManager.instance.FadeInThenFadeOut(1f, 1f, 1f);
        EnemyManager.instance.SpawnEgg();
    }

    private void OnChangeEnd()
    {
        currentGameState = EGameState.End;
        // End 상태에서 호출되는 함수
        TimeManager.instance.IsStop = true;
        TimeManager.instance.CheckAndResetDayPassed();
        FadeManager.instance.FadeInThenFadeOut(1f, 1f, 1f);
    }
    


    private void Update()
    {
        UIManager.instance.UpdateGold(totalGold);

       
    }
    private IEnumerator CheckGameOver()
    {
        while (true)
        {
            if(CentralBatterySystem.Instance.currentBatteryLevel <= 0 )
            {
                UIManager.instance.gameOverImage.SetActive(true);
            }

            if(playerStatus.CurrentHealth <= 0)
            {
                UIManager.instance.gameOverImage.SetActive(true);
            }
            yield return null;
        }
    }

    public bool BuyItemWithGold(ItemData itemData)
    {
        var temp = totalGold - itemData.BuyPrice;
        if(temp < 0)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }
        totalGold -= itemData.BuyPrice;
        return true;
    }
}

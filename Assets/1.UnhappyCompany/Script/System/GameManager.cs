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
        
        StartCoroutine(FadeOut());
        GameStart();
        StartCoroutine(CheckGameOver());
        // CloseCenterDoor();
    }
    private void GameStart()
    {
        currentGameState = EGameState.Ready;
        OnChangeEndToReady();
        StartCoroutine(GameStateFSM());
    }

    private IEnumerator FadeOut()
    {
        FadeManager.instance.fadeOverlay.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        FadeManager.instance.FadeOut(FadeManager.instance.fadeOverlay, 1f);
    }

    public IEnumerator ShowDayText()
    {
        UIManager.instance.screenDayText.gameObject.SetActive(true);
        UIManager.instance.screenDayText.text = $"Day {TimeManager.instance.days}";
        yield return new WaitForSeconds(2.5f);
        UIManager.instance.screenDayText.gameObject.SetActive(false);
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
                            OnChangeReadyToPlaying();
                            
                            // Debug.Log("게임 준비 완료");
                        }
                        break;

                    case EGameState.Playing:
                        // Playing 상태에서의 로직 처리
                        // Debug.Log("게임 진행 중");
                        // 게임 오버 조건을 체크하여 End 상태로 전환
                        if (TimeManager.instance.HasDayPassed)
                        {
                            OnChangePlayingToEnd();
                        }
                        break;

                    case EGameState.End:
                        // End 상태에서의 로직 처리
                        Debug.Log("게임 종료");
                        // UIManager.instance.gameOverImage.SetActive(true);
                        // 게임을 재시작하거나 메인 메뉴로 돌아가는 로직 추가 가능
                        if(true)
                        {
                            OnChangeEndToReady();
                        }
                        break;
                }
                // previousState = currentGameState;
            }
            yield return null;
        }
    }
    

    /// <summary>
    /// 게임 준비 상태에서 게임 시작 상태로 전환
    /// </summary>
    private void OnChangeReadyToPlaying()
    {
        currentGameState = EGameState.Playing;
        Debug.Log("OnChangeReadyToPlaying");
        CentralBatterySystem.Instance.isStop = false;
        isPressedStartBtn = false;
        TimeManager.instance.IsStop = false;
        EnemyManager.instance.SpawnEggsInEachRoom();
        OpenCenterDoor();
        CentralBatterySystem.Instance.isStop = false;
        RoomManager.Instance.centerNavMeshSurface.enabled = true; // 센터에 몬스터 접근 가능해짐.
    }

    /// <summary>
    /// 게임 진행 상태에서 게임 종료 상태로 전환
    /// </summary>
    private void OnChangePlayingToEnd()
    {
        currentGameState = EGameState.End;
        Debug.Log("OnChangePlayingToEnd");
        // End 상태에서 호출되는 함수
        TimeManager.instance.IsStop = true;
        TimeManager.instance.CheckAndResetDayPassed();
        FadeManager.instance.FadeInThenFadeOut(1f, 1f, 1f);
        CloseCenterDoor();
        CentralBatterySystem.Instance.isStop = true;
        RoomManager.Instance.centerNavMeshSurface.enabled = false; // 센터에 몬스터 접근 불가하게 하기 위해서
    }

    /// <summary>
    /// 게임 종료 상태에서 게임 준비 상태로 전환
    /// </summary>
    private void OnChangeEndToReady()
    {
        currentGameState = EGameState.Ready;
        Debug.Log("OnChangeEndToReady");
        TimeManager.instance.IsStop = true;
        CentralBatterySystem.Instance.isStop = true;
        CloseCenterDoor();
        RoomManager.Instance.centerNavMeshSurface.enabled = false; // 센터에 몬스터 접근 불가하게 하기 위해서
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

    public Player GetNearestPlayer(Transform transform)
    {
        // 멀티 상황에서 플레이어 중 가장 가까운 플레이어를 반환
        // 지금은 싱글이라 그냥 반환
        return currentPlayer;
    }
    public void CloseCenterDoor()
    {
        if(RoomManager.Instance == null)
        {
            return;
        }
        Debug.Log("CloseCenterDoor");
        foreach(var door in RoomManager.Instance.centerDoorList)
        {
            door.CloseDoor();
        }
    }

    public void OpenCenterDoor()
    {
        Debug.Log("OpenCenterDoor");
        foreach(var door in RoomManager.Instance.centerDoorList)
        {
            door.OpenDoor();
        }
    }
}

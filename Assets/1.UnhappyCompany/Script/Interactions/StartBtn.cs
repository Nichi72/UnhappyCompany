using UnityEngine;

public class StartBtn : MonoBehaviour , IInteractable
{
    public void HitEventInteractionF(Player rayOrigin)
    {
        if(GameManager.instance.currentGameState == GameState.None || GameManager.instance.currentGameState == GameState.Ready)
        {
            GameManager.instance.currentGameState = GameState.Ready;
            GameManager.instance.isPressedStartBtn = true;
            Debug.Log("StartBtn HitEventInteractionF");
        }
        else
        {
            Debug.Log("게임이 이미 시작되었습니다.");
        }
    }
}


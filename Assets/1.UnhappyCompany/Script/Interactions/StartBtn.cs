using UnityEngine;
using MyUtility;

public class StartBtn : MonoBehaviour , IInteractable
{
    public string InteractionText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "StartBtn_ITR"); set => InteractionText = value; }

    public void HitEventInteractionF(Player rayOrigin)
    {
        if(GameManager.instance.currentGameState == EGameState.None || GameManager.instance.currentGameState == EGameState.Ready)
        {
            GameManager.instance.currentGameState = EGameState.Ready;
            GameManager.instance.isPressedStartBtn = true;
            Debug.Log("StartBtn HitEventInteractionF");
        }
        else
        {
            Debug.Log("게임이 이미 시작되었습니다.");
        }
    }
}


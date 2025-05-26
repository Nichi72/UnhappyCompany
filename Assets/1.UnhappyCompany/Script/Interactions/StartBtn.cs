using UnityEngine;
using MyUtility;

public class StartBtn : MonoBehaviour , IInteractableF
{
    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "StartBtn_ITR"); set => InteractionTextF = value; }
    public bool IgnoreInteractionF { get; set; } = false;
    public void HitEventInteractionF(Player rayOrigin)
    {
        if(GameManager.instance.currentGameState == EGameState.None || GameManager.instance.currentGameState == EGameState.Ready)
        {
            GameManager.instance.currentGameState = EGameState.Ready;
            GameManager.instance.isPressedStartBtn = true;
            Debug.Log("StartBtn HitEventInteractionF");
            AudioManager.instance.PlayTestBeep("StartBtn HitEventInteractionF", transform);
            StartCoroutine(GameManager.instance.ShowDayText());

        }
        else
        {
            Debug.Log("게임이 이미 시작되었습니다.");
        }
    }
}


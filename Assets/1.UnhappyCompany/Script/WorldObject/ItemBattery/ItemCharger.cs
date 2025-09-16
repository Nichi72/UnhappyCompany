using System.Collections;
using UnityEngine;
using MyUtility;


// FIXME: 충전 완료될 때, 씨네마씬 카메라 애니메이션이 끝나지 않았을 때 움직이면 살짝 움직임이 한 번 이동하는 버그 수정 필요
public class ItemCharger : MonoBehaviour, IInteractableF, IToolTip
{
    enum ChargerState
    {
        Open,
        Close
    }

    public Transform cameraTarget;
    public Transform playerTarget;
    private Player currentUsePlayer;
    [SerializeField] private ChargerState chargerState;

    // public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemCharger_ITR"); set => InteractionTextF = value; }
    public string InteractionTextF { get => "IF_충전하기"; set => InteractionTextF = value; }
    public bool IgnoreInteractionF { get; set; } = false;
    public string ToolTipText { get => "충전기 사용 중..."; set => ToolTipText = value; }
    public string ToolTipText2 { get => ""; set => ToolTipText2 = value; }
    public string ToolTipText3 { get => ""; set => ToolTipText3 = value; }

    private void Start()
    {
        chargerState = ChargerState.Close;
    }

    public void HitEventInteractionF(Player player)
    {
        if (chargerState == ChargerState.Close)
        {
            currentUsePlayer = player;
            if (currentUsePlayer != null)
            {
                OpenCharger(currentUsePlayer);
                chargerState = ChargerState.Open; // 즉시 상태 변경
                ToolTipUI.instance.SetToolTip(this);

                // 1초 후 자동으로 닫기
                StartCoroutine(AutoCloseAfterDelay());
            }
        }
    }

    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        if (currentUsePlayer != null)
        {
            CloseCharger(currentUsePlayer);
            currentUsePlayer = null;
            chargerState = ChargerState.Close;
        }
    }

    public void OpenCharger(Player player)
    {
        Debug.Log("OpenCharger");

        // 플레이어를 playerTarget 위치로 이동
        if (playerTarget != null)
        {
            player.transform.position = playerTarget.position;
            player.transform.rotation = playerTarget.rotation;
            player.firstPersonController.SmoothChangeCinemachineCameraTarget(cameraTarget.gameObject);
        }

        player.firstPersonController._input.SetCursorLock(false);
    }

    public void CloseCharger(Player player)
    {
        Debug.Log("CloseCharger");

        player.firstPersonController._input.SetCursorLock(true);
        player.firstPersonController.SmoothChangeCinemachineCameraTarget(player.firstPersonController.CinemachineCameraTarget.gameObject);
        StartCoroutine(ResetCinemachineCameraDamping(player, 0.7f));
    }

    public IEnumerator ResetCinemachineCameraDamping(Player player, float delay)
    {
        yield return new WaitForSeconds(delay);
        player.firstPersonController.ResetCinemachineCameraDamping();
    }

    public void BtnEvtCloseCharger()
    {
        if (currentUsePlayer != null)
        {
            CloseCharger(currentUsePlayer);
            currentUsePlayer = null;
            chargerState = ChargerState.Close;
        }
    }

    private void ChargeItem(IRechargeable rechargeableItem)
    {
        // 충전 로직 구현
        Debug.Log("Charging item...");
        // TODO: 실제 충전 로직 구현
        // 1. 플레이어를 cameraTarget 위치로 이동
    }

    public void BtnEvtChargeItem(IRechargeable rechargeableItem)
    {
        ChargeItem(rechargeableItem);
    }
}

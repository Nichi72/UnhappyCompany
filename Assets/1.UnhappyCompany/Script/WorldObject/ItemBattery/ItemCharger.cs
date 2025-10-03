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
            // 손에 든 아이템이 충전 가능한 경우에만 작동하도록 게이트
            if (!TryGetRechargeableHeldItem(player, out _)) return;

            currentUsePlayer = player;
            OpenCharger(currentUsePlayer);
            chargerState = ChargerState.Open; // 즉시 상태 변경
            ToolTipUI.instance.SetToolTip(this);

            // 플레이어가 손에 든 아이템이 충전 가능한지 확인하고 충전
            CheckAndChargePlayerItem(player);

            // 1초 후 자동으로 닫기
            StartCoroutine(AutoCloseAfterDelay());
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
        // 플레이어를 playerTarget 위치로 이동
        if (playerTarget != null)
        {
            player.transform.position = playerTarget.position;
            player.transform.rotation = playerTarget.rotation;
            player.firstPersonController.SmoothChangeCinemachineCameraTarget(cameraTarget.gameObject);
        }

        // 입력 및 시점 고정
        player.firstPersonController._input.SetCursorLock(false);
        player.firstPersonController._input.FreezePlayerInput(true);
    }

    public void CloseCharger(Player player)
    {
        player.firstPersonController._input.SetCursorLock(true);
        player.firstPersonController._input.FreezePlayerInput(false);
        player.firstPersonController.SmoothChangeCinemachineCameraTarget(player.firstPersonController.CinemachineCameraTarget.gameObject);
        StartCoroutine(ResetCinemachineCameraDamping(player, 0.7f));

        // 툴팁 클리어
        if (ToolTipUI.instance != null)
        {
            ToolTipUI.instance.SetToolTip(null); // TODO: SetToolTip Item으로 ㄱㄱ헛? item의 내용이 떠야 하는뎅
        }
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

    private void ChargeItem(ICentralBatteryRechargeable rechargeableItem)
    {
        // 이미 완전 충전완료 상태인지 확인
        if (rechargeableItem.IsFullyCharged)
        {
            Debug.Log("이미 충전 완료되었습니다.");
            return;
        }

        var result = rechargeableItem.TryChargeFromCentralBattery();

        switch (result)
        {
            case ChargeResult.Success:
                Debug.Log($"충전 성공! 아이템: {rechargeableItem.GetItemName()}");
                break;
            case ChargeResult.AlreadyFull:
                Debug.Log($"충전 불필요: {rechargeableItem.GetItemName()}은 이미 완전 충전되었습니다.");
                break;
            case ChargeResult.CentralBatteryEmpty:
                Debug.Log($"충전 실패: 중앙 배터리 잔량 부족:");
                break;
            case ChargeResult.SystemUnavailable:
                Debug.Log($"충전 실패: 시스템을 사용할 수 없습니다");
                break;
        }
    }

    private void CheckAndChargePlayerItem(Player player)
    {
        if (!TryGetRechargeableHeldItem(player, out var rechargeableItem)) return;
        ChargeItem(rechargeableItem);
    }

    /// <summary>
    /// 플레이어가 현재 손에 들고 있는 아이템이 충전 가능한지 검사하고 결과를 반환합니다.
    /// </summary>
    /// <param name="player">대상 플레이어</param>
    /// <param name="rechargeableItem">충전 가능한 아이템 (없으면 null)</param>
    /// <returns>충전 가능 여부</returns>
    private bool TryGetRechargeableHeldItem(Player player, out ICentralBatteryRechargeable rechargeableItem)
    {
        rechargeableItem = null;
        if (player == null || player.quickSlotSystem == null) return false;
        var currentObj = player.quickSlotSystem.currentItemObject;
        if (currentObj == null) return false;

        var currentItem = currentObj.GetComponent<Item>();
        if (currentItem is ICentralBatteryRechargeable castItem)
        {
            rechargeableItem = castItem;
            return true;
        }
        return false;
    }
}

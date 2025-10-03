using System.Collections;
using UnityEngine;
using Cinemachine;
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

    [Header("Camera Return Timing")]
    [SerializeField] private float cameraReturnLerpDuration = 0.1f; // was 0.3f

    [Header("Ground Snap")]
    [SerializeField] private float groundCheckUp = 1.5f;
    [SerializeField] private float groundCheckDown = 5f;

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
            Vector3 groundedPos = GetGroundedPositionForCharacter(player, playerTarget.position);
            TeleportCharacterController(player, groundedPos, playerTarget.rotation);
            player.firstPersonController.SmoothChangeCinemachineCameraTarget(cameraTarget.gameObject);
        }

        // 입력 및 시점 고정
        player.firstPersonController._input.SetCursorLock(false);
        player.firstPersonController._input.FreezePlayerInput(true);
    }

    public void CloseCharger(Player player)
    {
        // 카메라를 원래 타겟으로 전환하되, 입력은 잠시 더 고정해서 스냅 현상 방지
        player.firstPersonController.SmoothChangeCinemachineCameraTarget(player.firstPersonController.CinemachineCameraTarget.gameObject);

        // 부드러운 전환이 끝날 때까지 감쇠를 유지하고 점진적으로 0으로 낮춘 뒤 입력 해제
        StartCoroutine(CloseChargerSequence(player));

        // 툴팁 클리어
        if (ToolTipUI.instance != null)
        {
            ToolTipUI.instance.SetToolTip(null); // TODO: QuickSlotCurrentItem 같은거 참조해서 tooltip불러올 수 있도록 만듬
        }
    }

    public IEnumerator ResetCinemachineCameraDamping(Player player, float delay)
    {
        yield return new WaitForSeconds(delay);
        player.firstPersonController.ResetCinemachineCameraDamping();
    }

    private IEnumerator CloseChargerSequence(Player player)
    {
        // 입력 해제 및 커서 잠금 복구
        player.firstPersonController._input.SetCursorLock(true);
        player.firstPersonController._input.FreezePlayerInput(false);

        // 감쇠를 점진적으로 낮춤 (스냅 방지)
        var vcam = player.firstPersonController.cinemachineVirtualCamera;
        var follow = vcam != null ? vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>() : null;
        if (follow != null)
        {
            Vector3 start = follow.Damping;
            Vector3 end = new Vector3(0f, 0f, 0f);
            float t = 0f;
            while (t < cameraReturnLerpDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / cameraReturnLerpDuration);
                Vector3 v = Vector3.Lerp(start, end, k);
                follow.Damping.x = v.x;
                follow.Damping.y = v.y;
                follow.Damping.z = v.z;
                yield return null;
            }
            // 최종 스냅 방지용 아주 미세한 감쇠 유지가 필요하면 아래 값을 0.05f 등으로 조정 가능
            follow.Damping.x = 0f;
            follow.Damping.y = 0f;
            follow.Damping.z = 0f;
        }
    }

    private Vector3 GetGroundedPositionForCharacter(Player player, Vector3 targetPosition)
    {
        if (player == null) return targetPosition;

        var cc = player.GetComponent<CharacterController>();
        if (cc == null)
        {
            // CharacterController가 없으면 y만 유지하고 xz만 이동
            return new Vector3(targetPosition.x, player.transform.position.y, targetPosition.z);
        }

        // 위에서 아래로 레이캐스트하여 지면 위치를 찾음
        Vector3 rayOrigin = targetPosition + Vector3.up * groundCheckUp;
        float rayDistance = groundCheckUp + groundCheckDown;

        // 플레이어 컨트롤러가 쓰는 GroundLayers 우선 사용, 없으면 기본값으로 처리
        LayerMask groundMask = (player.firstPersonController != null) ? player.firstPersonController.GroundLayers : Physics.DefaultRaycastLayers;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            // 캡슐의 최하단이 지면에 스치도록 오프셋 계산
            float epsilon = Mathf.Max(cc.skinWidth, 0.02f);
            Vector3 desiredLowestPoint = hit.point + Vector3.up * epsilon;
            // CharacterController의 최하단점은 transform.position + center - up*(height/2)
            Vector3 newPosition = desiredLowestPoint + Vector3.up * (cc.height / 2f) - cc.center;
            return newPosition;
        }

        // 지면을 찾지 못했을 경우 현재 y 유지 (부유 방지)
        return new Vector3(targetPosition.x, player.transform.position.y, targetPosition.z);
    }

    private void TeleportCharacterController(Player player, Vector3 position, Quaternion rotation)
    {
        if (player == null) return;
        var cc = player.GetComponent<CharacterController>();
        bool wasEnabled = false;
        if (cc != null)
        {
            wasEnabled = cc.enabled;
            cc.enabled = false;
        }
        player.transform.SetPositionAndRotation(position, rotation);
        if (cc != null)
        {
            cc.enabled = wasEnabled;
        }
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

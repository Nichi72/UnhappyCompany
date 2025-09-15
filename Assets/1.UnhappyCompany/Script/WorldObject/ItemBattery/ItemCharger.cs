using System.Collections;
using UnityEngine;
using MyUtility;

public class ItemCharger : MonoBehaviour, IInteractableF, IToolTip
{
    enum ChargerState
    {
        Open,
        Close
    }

    public Transform cameraTarget;
    public Player currentUsePlayer;
    [SerializeField] private ChargerState chargerState;

    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "ItemCharger_ITR"); set => InteractionTextF = value; }
    public bool IgnoreInteractionF { get; set; } = false;
    public string ToolTipText { get => "F : 충전기 나가기"; set => ToolTipText = value; }
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
                OpenCharger(player);
                DelayChangeState(ChargerState.Open);
                ToolTipUI.instance.SetToolTip(this);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (chargerState == ChargerState.Open)
            {
                if (currentUsePlayer != null)
                {
                    CloseCharger(currentUsePlayer);
                    currentUsePlayer = null;
                    DelayChangeState(ChargerState.Close);
                }
            }
        }
    }

    private void DelayChangeState(ChargerState state)
    {
        StartCoroutine(DelayChangeStateCoroutine(state));
    }

    private IEnumerator DelayChangeStateCoroutine(ChargerState state)
    {
        yield return new WaitForSeconds(1f);
        chargerState = state;
    }

    public void OpenCharger(Player player)
    {
        Debug.Log("OpenCharger");
        player.firstPersonController._input.SetCursorLock(false);
        if (cameraTarget != null)
            player.firstPersonController.SmoothChangeCinemachineCameraTarget(cameraTarget.gameObject);
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
            DelayChangeState(ChargerState.Close);
        }
    }

    private void ChargeItem(IRechargeable rechargeableItem)
    {
        // 충전 로직 구현
        Debug.Log("Charging item...");
        // TODO: 실제 충전 로직 구현
    }

    public void BtnEvtChargeItem(IRechargeable rechargeableItem)
    {
        ChargeItem(rechargeableItem);
    }
}

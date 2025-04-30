using System.Collections;
using UnityEngine;
using MyUtility;

public class Computer : MonoBehaviour , IInteractableF , IToolTip
{
    enum ComputerState
    {
        Open,
        Close
    }
    public Transform spwanTr;
    public Transform cameraTarget;
    public Player currentUsePlayer;
    [SerializeField] private ComputerState computerState;

    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Computer_ITR"); set => InteractionTextF = value; }
    public string ToolTipText { get => "F :컴퓨터 나가기"; set => ToolTipText = value; }
    public string ToolTipText2 { get => ""; set => ToolTipText2 = value; }
    public string ToolTipText3 { get => ""; set => ToolTipText3 = value; }

    private void Start()
    {
        computerState = ComputerState.Close;
    }
    public void HitEventInteractionF(Player player)
    {
        if(computerState == ComputerState.Close)
        {
            currentUsePlayer = player;
            if(currentUsePlayer != null)
            {
                ComputerSystem.instance.OpenComputer(player);
                DelayChangeState(ComputerState.Open);
                ToolTipUI.instance.SetToolTip(this);
            }
        }
    }
   

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            if(computerState == ComputerState.Open)
            {
                if(currentUsePlayer != null)
                {
                    ComputerSystem.instance.CloseComputer(currentUsePlayer);
                    currentUsePlayer = null;
                    DelayChangeState(ComputerState.Close);
                }
                
            }
        }
    }

    private void DelayChangeState(ComputerState state)
    {
        StartCoroutine(DelayChangeStateCoroutine(state));
    }

    private IEnumerator DelayChangeStateCoroutine(ComputerState state)
    {
        yield return new WaitForSeconds(1f);
        computerState = state;
    }
   


}

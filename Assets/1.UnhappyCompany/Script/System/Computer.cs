using System.Collections;
using UnityEngine;

public class Computer : MonoBehaviour , IInteractable
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

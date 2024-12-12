using UnityEngine;

public class Computer : MonoBehaviour , IInteractable
{
    public Transform spwanTr;
    public Player currentUsePlayer;
    public void HitEventInteractionF(Player player)
    {
        currentUsePlayer = player;
        ComputerSystem.instance.OpenComputer(player);
    }

   
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

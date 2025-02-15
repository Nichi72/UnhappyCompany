using UnityEngine;

public class PlayerArmAnimationEvents : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Player player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AniEvt_ItemsMeleeAttack_Attack()
    {
        Debug.Log("AniEvt_Attack");
        player.quickSlotSystem.currentItemObject.GetComponent<ItemsMeleeAttack>().AniEvt_Attack();
    }
}

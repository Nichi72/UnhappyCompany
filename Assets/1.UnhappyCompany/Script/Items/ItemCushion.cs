using UnityEngine;

public class ItemCushion : Item
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Use(Player player)
    {
        player.buildSystem.StartPlacing(itemData.prefab.gameObject, this.gameObject, true);
    }
}

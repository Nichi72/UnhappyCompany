using UnityEngine;

public class ItemCushion : Item
{
    public bool isPreview = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void Use(Player player)
    {
        if (isPreview == false)
        {
            player.buildSystem.StartPlacing(itemData.prefab.gameObject, this.gameObject, true);
        }
    }
}

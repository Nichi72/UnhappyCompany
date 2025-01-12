using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform rightHandPos;
    public StarterAssets.FirstPersonController firstPersonController;
    public InteractionSystem interactionSystem;
    public QuickSlotSystem quickSlotSystem;
    public BuildSystem buildSystem;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            quickSlotSystem.DropItem();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(quickSlotSystem.currentItemObject == null)
            {
                return;
            }
            //playt
            var tempItem = quickSlotSystem.currentItemObject.GetComponent<Item>();
            if (tempItem == null) return;

            tempItem.Use(this);
        }
    }
}

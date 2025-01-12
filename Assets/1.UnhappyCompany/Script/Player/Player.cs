using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform rightHandPos;
    public StarterAssets.FirstPersonController firstPersonController;
    public InteractionSystem interactionSystem;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            QuickSlotSystem.instance.DropItem();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(QuickSlotSystem.instance.currentItemObject == null)
            {
                return;
            }
            //playt
            var tempItem = QuickSlotSystem.instance.currentItemObject.GetComponent<Item>();
            if (tempItem == null) return;

            tempItem.Use();
        }
    }
}

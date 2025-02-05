using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform rightHandPos;
    public StarterAssets.FirstPersonController firstPersonController;
    public InteractionSystem interactionSystem;
    public QuickSlotSystem quickSlotSystem;
    public BuildSystem buildSystem;
    public Animator armAnimator;
    public Animator ItemAnimator;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F3))
        {
            armAnimator.SetTrigger("test");
            ItemAnimator.SetTrigger("test");
        }

        if(quickSlotSystem.currentItemObject == null) return;


        var overrideUpdate = quickSlotSystem.currentItemObject.GetComponent<Item>() as IOverrideUpdate;
        if(overrideUpdate != null)
        {
            overrideUpdate.OverrideUpdate();
        }
        if(Input.GetKeyDown(KeyCode.G))
        {
            quickSlotSystem.DropItem();
        }
        // if(Input.GetKeyDown(KeyCode.Mouse1))

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

using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform rightHandPos;
    public Transform noneModelHandTransform;
    public Transform modelHandTransform;
    public StarterAssets.FirstPersonController firstPersonController;
    public InteractionSystem interactionSystem;
    public QuickSlotSystem quickSlotSystem;
    public BuildSystem buildSystem;
    public Animator armAnimator;
    public List<Transform> OffsetLists;
    public PlayerStatus playerStatus;
    public GameObject Head;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      

        if(quickSlotSystem.currentItemObject == null) return;


        var overrideUpdate = quickSlotSystem.currentItemObject.GetComponent<Item>() as IItemOverrideUpdate;
        if(overrideUpdate != null)
        {
            overrideUpdate.OverrideUpdate();
        }
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

    public void SetModelHandTransform(bool isModelHandAnimation)
    {
        if(isModelHandAnimation)
        {
            rightHandPos = modelHandTransform;
        }
        else
        {
            rightHandPos = noneModelHandTransform;
        }
    }
}

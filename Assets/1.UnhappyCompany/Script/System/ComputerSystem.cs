using System.Collections;
using UnityEngine;

public class ComputerSystem : MonoBehaviour
{
    public static ComputerSystem instance = null;

    public Computer computer;
    public ShopSystem shopSystem;
    public GameObject computerView;



    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        computerView.SetActive(false);
    }

    public void OpenComputer(Player player)
    {
        Debug.Log("OpenComputer");
        player.firstPersonController._input.SetCursorLock(false);
        computerView.SetActive(true);
        player.firstPersonController.SmoothChangeCinemachineCameraTarget(computer.cameraTarget.gameObject);
        
    }
    public void CloseComputer(Player player)
    {
        Debug.Log("CloseComputer");
        player.firstPersonController._input.SetCursorLock(true);
        computerView.SetActive(false);
        player.firstPersonController.SmoothChangeCinemachineCameraTarget(player.firstPersonController.CinemachineCameraTarget.gameObject);
        StartCoroutine(ResetCinemachineCameraDamping(player, 0.7f));
        computer.currentUsePlayer = null;
        
        // 컴퓨터 화면 닫는 소리 재생
        AudioManager.instance.PlayUISound(FMODEvents.instance.computerScreenClose, "Computer Screen Closed");
    }
    
    public void BtnEvtCloseComputer()
    {
        CloseComputer(computer.currentUsePlayer);
        UIManager.instance.ToggleObject(computerView);
    }
    public IEnumerator ResetCinemachineCameraDamping(Player player, float delay)
    {
        yield return new WaitForSeconds(delay);
        player.firstPersonController.ResetCinemachineCameraDamping();
    }

    private void BuyItem(ItemData itemData)
    {
        if (GameManager.instance.BuyItemWithGold(itemData))
        {
            var itemObj = Instantiate(itemData.prefab);
            itemObj.transform.position = computer.spwanTr.position;
            var rigidbody = itemObj.GetComponent<Rigidbody>();
            rigidbody.AddForce(computer.spwanTr.transform.right * 250f);
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.shopItemDrop, itemObj.transform, 20f, "BuyItem Drop");
        }
    }

    public void BtnEvtBuyItem(ItemData itemData)
    {
        BuyItem(itemData);
    }

   
    public void BtnEvtPressed(GameObject view)
    {
        Debug.Log("BtnEvtPressed");
        
        // 컴퓨터 클릭음 재생
        AudioManager.instance.PlayUISound(FMODEvents.instance.computerCursorClick, "Computer Cursor Click");
        
        // 창을 닫을 때 (현재 활성화 상태 -> 비활성화) 창 닫는 소리 재생
        if (view != null && view.activeSelf)
        {
            AudioManager.instance.PlayUISound(FMODEvents.instance.computerScreenClose, "Computer Screen Closed");
        }
        
        UIManager.instance.ToggleObject(view);
    }
}


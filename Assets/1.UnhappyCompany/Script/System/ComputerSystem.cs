using System.Collections;
using UnityEngine;

public class ComputerSystem : MonoBehaviour
{
    public Computer computer;
    public GameObject computerView;
    public static ComputerSystem instance = null;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // UIManager.instance.computerView = computerView;
    }

    // Update is called once per frame
    void Update()
    {
        
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
        }
    }

    public void BtnEvtBuyItem(ItemData itemData)
    {
        BuyItem(itemData);
    }
}

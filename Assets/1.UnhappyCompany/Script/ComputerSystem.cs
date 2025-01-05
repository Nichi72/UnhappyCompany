using System.Collections;
using UnityEngine;

public class ComputerSystem : MonoBehaviour
{
    public Computer computer;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenComputer(Player player)
    {
        Debug.Log("OpenComputer");
        player.firstPersonController._input.SetCursorLock(false);
        UIManager.instance.computerView.SetActive(true);
        player.firstPersonController.SmoothChangeCinemachineCameraTarget(computer.cameraTarget.gameObject);
        
    }
    public void CloseComputer(Player player)
    {
        Debug.Log("CloseComputer");
        player.firstPersonController._input.SetCursorLock(true);
        UIManager.instance.computerView.SetActive(false);
        player.firstPersonController.SmoothChangeCinemachineCameraTarget(player.firstPersonController.CinemachineCameraTarget.gameObject);
        StartCoroutine(ResetCinemachineCameraDamping(player, 0.7f));
        computer.currentUsePlayer = null;
    }
    public void BtnEvtCloseComputer()
    {
        CloseComputer(computer.currentUsePlayer);
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
            itemObj.AddComponent<Rigidbody>();
        }
    }
    public void BtnEvtBuyItem(ItemData itemData)
    {
        BuyItem(itemData);
    }
}

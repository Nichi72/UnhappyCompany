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
        // OpenComputer
        player.firstPersonController._input.cursorInputForLook = false;
        player.firstPersonController._input.cursorLocked = false;
        player.firstPersonController._input.SetCursorState(false);
        UIManager.Instance.computerView.SetActive(true);
    }
    public void CloseComputer(Player player)
    {
        player.firstPersonController._input.cursorInputForLook = true;
        player.firstPersonController._input.cursorLocked = true;
        player.firstPersonController._input.SetCursorState(true);
        UIManager.Instance.computerView.SetActive(false);
        computer.currentUsePlayer = null;
    }
    public void BtnEvtCloseComputer()
    {
        CloseComputer(computer.currentUsePlayer);
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

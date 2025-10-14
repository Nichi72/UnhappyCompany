using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemBuyItem : MonoBehaviour, IPointerEnterHandler
{
    public TextMeshProUGUI itemName;
    public Button buyBtn;
    public ItemData itemData;
    public int itemPrice;
    public void Init(ItemData itemData)
    {
        this.itemName.text = itemData.itemName;
        this.itemData = itemData;
        this.itemPrice = itemData.BuyPrice;
        itemName.text = $"{itemPrice}G : {itemData.itemName} ";
    }

    public void BtnPressed()
    {
        // 컴퓨터 클릭음 재생
        AudioManager.instance.PlayOneShot(FMODEvents.instance.computerCursorClick, transform, "Computer Cursor Click");
        
        ComputerSystem.instance.BtnEvtBuyItem(itemData);
        Debug.Log("BuyItem");
        AudioManager.instance.PlayOneShot(FMODEvents.instance.shopItemBuy, transform, "BuyItem BtnPressed");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.computerCursorHover, transform, "BuyItem OnPointerEnter");
    }
}

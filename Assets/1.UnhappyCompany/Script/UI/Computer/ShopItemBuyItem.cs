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
        ComputerSystem.instance.BtnEvtBuyItem(itemData);
        Debug.Log("BuyItem");
        AudioManager.instance.PlayTestBeep("BuyItem BtnPressed", transform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.instance.PlayTestBeep("BuyItem OnPointerEnter", transform);
    }
}

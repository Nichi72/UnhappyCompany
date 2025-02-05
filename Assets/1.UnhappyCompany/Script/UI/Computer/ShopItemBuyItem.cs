using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemBuyItem : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public ItemData itemData;
    public void Init(ItemData itemData)
    {
        this.itemName.text = itemData.itemName;
        this.itemData = itemData;
    }

    public void BtnPressed()
    {
        ComputerSystem.instance.BtnEvtBuyItem(itemData);
        Debug.Log("BuyItem");
    }


}

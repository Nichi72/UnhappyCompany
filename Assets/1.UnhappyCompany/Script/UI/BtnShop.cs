using TMPro;
using UnityEngine;

public class BtnShop : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;

    public void SetItem(ItemData itemData)
    {
        itemName.text = itemData.itemName;
        itemPrice.text = itemData.BuyPrice.ToString();
    }

    public void BtnBuyItem(ItemData itemData)
    {
        ComputerSystem.instance.BtnEvtBuyItem(itemData);
    }   
}

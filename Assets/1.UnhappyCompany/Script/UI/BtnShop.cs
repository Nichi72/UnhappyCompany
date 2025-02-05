using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BtnShop : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;
    [SerializeField]private ItemData itemData;

    public void Start()
    {
        SetItem(itemData);
    }

    public void SetItem(ItemData itemData)
    {
        itemName.text = itemData.itemName;
        itemPrice.text = itemData.BuyPrice.ToString();
    }

    public void BtnBuyItem()
    {
        ComputerSystem.instance.BtnEvtBuyItem(itemData);
    }   
}

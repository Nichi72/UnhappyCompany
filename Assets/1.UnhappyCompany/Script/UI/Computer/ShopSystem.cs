using UnityEngine;
using TMPro;
public class ShopSystem : MonoBehaviour
{
    public GameObject shopUIPrefab;
    public Transform shopUIParent;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI totalGoldText;
    private void Awake()
    {
        LoadShopItems();
    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        totalGoldText.text = $"Total Gold : {GameManager.instance.totalGold}G";
    }

    // Update is called once per frame
    void Update()
    {
        totalGoldText.text = $"Total Gold : {GameManager.instance.totalGold}G";
    }

     private void LoadShopItems()
    {
        ItemData[] shopItems = Resources.LoadAll<ItemData>("ScriptableObj/Item/Shop");
        foreach (ItemData item in shopItems)
        {
            Debug.Log($"Loaded item: {item.itemName}");
            GameObject shopItemBuyItem = Instantiate(shopUIPrefab, shopUIParent);
            shopItemBuyItem.GetComponent<ShopItemBuyItem>().Init(item);
        }
    }
}

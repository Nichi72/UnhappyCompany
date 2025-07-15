using System.Collections.Generic;
using UnityEngine;

public class SalesStore : MonoBehaviour
{
    public TMPro.TextMeshPro textMeshPro;
    public List<GameObject> sellObjects;
    public SalesStoreSellPoint salesStoreSellPoint;

    public void ChangeText(string newText)
    {
        if (textMeshPro != null)
        {
            string text = $"$ {newText}";
            textMeshPro.text = text;
        }
        else
        {
            Debug.LogError("TextMeshPro component is not assigned.");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ChangeText(GameManager.instance.totalGold.ToString());
    }
    int totalPrice = 0;
    public void SellObject()
    {
        foreach(var sellObject in sellObjects)
        {
            Destroy(sellObject);
        }
        totalPrice = 0;
        ChangeText(totalPrice.ToString());
    }

    public void AddSellObject(GameObject sellObject)
    {
        sellObjects.Add(sellObject);
        totalPrice += sellObject.GetComponent<Item>().itemData.SellPrice;
        sellObject.layer = LayerMask.NameToLayer("Default");
        ChangeText(totalPrice.ToString());
    }

    
}


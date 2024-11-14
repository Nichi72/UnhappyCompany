using UnityEngine;

public class SellPoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != Tag.Item.ToString())
        {
            Debug.Log($"OnTriggerEnter {other.name} ");
            return;
        }
        var temp = other.GetComponent<Item>();
        if (temp == null)
        {
            Debug.LogError("Item�� ���������ʽ��ϴ�.");
            return;
        }

        GameManager.instance.totalGold += temp.itemData.SellPrice;
        Destroy(other.gameObject);
    }
}

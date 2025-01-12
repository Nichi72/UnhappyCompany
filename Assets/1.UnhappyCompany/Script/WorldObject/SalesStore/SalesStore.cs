using UnityEngine;

public class SalesStore : MonoBehaviour
{

    public TMPro.TextMeshPro textMeshPro;

    public void ChangeText(string newText)
    {
        if (textMeshPro != null)
        {
            string text = $"$ {newText} 원";
            textMeshPro.text = newText;
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

    
}

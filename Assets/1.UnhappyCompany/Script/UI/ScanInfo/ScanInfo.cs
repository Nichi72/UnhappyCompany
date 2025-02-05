using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScanInfo : MonoBehaviour
{
    public TextMeshProUGUI scanInfoText;
    public Image itemImage;

    public void SetScanInfoText(string text)
    {
        scanInfoText.text = text;
    }
}

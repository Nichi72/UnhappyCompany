using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NotificationItem : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;
    public Sprite[] categoryImages;

    public void Init(string message, ENotificationCategory category)
    {
        text.text = message;
        image.sprite = categoryImages[(int)category];
    }
}

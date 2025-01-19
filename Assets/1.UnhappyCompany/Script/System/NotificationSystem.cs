using System.Collections.Generic;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    private Queue<string> notificationQueue = new Queue<string>();
    public GameObject notificationUIPrefab;
    public Transform notificationUIParent;

    void Update()
    {
        if (notificationQueue.Count > 0)
        {
            DisplayNotification(notificationQueue.Dequeue());
        }
    }

    public void AddNotification(string message)
    {
        notificationQueue.Enqueue(message);
    }

    private void DisplayNotification(string message)
    {
        GameObject notificationUI = Instantiate(notificationUIPrefab, notificationUIParent);
        notificationUI.GetComponentInChildren<UnityEngine.UI.Text>().text = message;
        Destroy(notificationUI, 5f); // 5초 후에 알림 UI를 제거

        // if(message == "알 획득")

    }
}
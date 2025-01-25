using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem instance;
    // 하루 뒤에 보여주는 큐
    public Queue<(string, ENotificationCategory)> delayedNotificationQueue = new Queue<(string, ENotificationCategory)>();
    // 즉시 보여주는 큐
    public Queue<(string, ENotificationCategory)> immediateNotificationQueue = new Queue<(string, ENotificationCategory)>();
    public Queue<(string, ENotificationCategory)> consoleQueue = new Queue<(string, ENotificationCategory)>();
    public GameObject notificationUIPrefab;
    public RectTransform notificationUIParent;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        StartCoroutine(ShowNotification());
    }

    public void AddNotification(string message, string doneMessage, ENotificationCategory category)
    {
        delayedNotificationQueue.Enqueue((doneMessage, category));
        immediateNotificationQueue.Enqueue((message, category));
    }

    
    public void ReceiveEnemyData(Dictionary<Transform, EnemyAIData> detectedEnemies, Dictionary<Transform, Egg> detectedEggs)
    {
        // foreach (var enemy in detectedEnemies)
        // {
        //     Debug.Log($"Enemy: {enemy.Key.name}, Data: {enemy.Value}");
        // }
        
        foreach (var egg in detectedEggs)
        {
            Egg eggData = egg.Value;
            Debug.Log($"Egg: {egg.Key.name}, Data: {egg.Value}");
            // [Category] Time : <Time> Egg ID : <EGG.ID> Egg Scanning…
            string scaningMessage = $"[{eggData.enemyAIData.category}] [{TimeManager.instance.GameTime}] Egg ID : {eggData.id} Egg Scanning…";
            string doneMessage = $"[{eggData.enemyAIData.category}] [{TimeManager.instance.GameTime}] Egg ID : {eggData.id} Egg Scanning…";

            Debug.Log(scaningMessage);
            AddNotification(scaningMessage,doneMessage, eggData.enemyAIData.category);
        }
    }
    
    IEnumerator ShowNotification()
    {
        while (true)
        {
            if (immediateNotificationQueue.Count > 0)
            {
                (string message, ENotificationCategory category) = immediateNotificationQueue.Dequeue();
                Debug.Log(message);
                InitNotification(message, category);
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.45f));
        }
    }

    // 알림창에 보여주는 함수
    public void InitNotification(string message,ENotificationCategory eNotificationCategory)
    {
        Debug.Log(message);
        GameObject notification = Instantiate(notificationUIPrefab, notificationUIParent.transform);
        notification.transform.SetSiblingIndex(0);
        notification.GetComponent<NotificationItem>().Init(message, eNotificationCategory);
    }
}

[System.Serializable]
public class Notification
{
    public string Message;
    public DateTime Time;
    public ENotificationCategory Category;

    public Notification(string message, DateTime time, ENotificationCategory category)
    {
        Message = message;
        Time = time;
        Category = category;
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrollableText : MonoBehaviour
{
    public TMP_Text textMeshPro; // TextMeshPro 컴포넌트
    public int maxLines = 10; // 최대 표시 줄 수
    private Queue<string> messageQueue = new Queue<string>(); // 메시지 저장소

    // 새로운 메시지 추가
    public void AddMessage(string message)
    {
        // 메시지를 큐에 추가
        messageQueue.Enqueue(message);

        // 최대 줄 수 초과 시 가장 오래된 메시지 제거
        if (messageQueue.Count > maxLines)
        {
            messageQueue.Dequeue();
        }

        // 텍스트 업데이트
        UpdateText();
    }

    // 텍스트 업데이트
    private void UpdateText()
    {
        textMeshPro.text = string.Join("\n", messageQueue.ToArray());
    }
}

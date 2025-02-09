using UnityEngine;
using UnityEngine.UI;

public class AdjustBGSize : MonoBehaviour
{
    public RectTransform bgRect; // BG의 RectTransform
    public TMPro.TextMeshProUGUI interactionText; // 텍스트 컴포넌트

    void Update()
    {
        Vector2 textSize = interactionText.rectTransform.sizeDelta;
        bgRect.sizeDelta = textSize + new Vector2(20, 20); // 패딩 추가
    }
}

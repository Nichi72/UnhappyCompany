using System.Collections;
using UnityEngine;

public class CCTVFrameRateController : MonoBehaviour
{
    public Camera cctvCamera; // CCTV 카메라 참조
    public RenderTexture renderTexture; // CCTV 화면에 렌더링할 텍스처
    public float updateInterval = 0.5f; // 화면 업데이트 간격 (초)

    private WaitForSeconds waitTime;
    private bool isUpdating;

    void Start()
    {
        if (cctvCamera == null)
        {
            Debug.LogError("CCTV 카메라가 할당되지 않았습니다.");
            return;
        }

        if (renderTexture == null)
        {
            Debug.LogError("렌더 텍스처가 할당되지 않았습니다.");
            return;
        }

        cctvCamera.targetTexture = renderTexture;
        waitTime = new WaitForSeconds(updateInterval);
        StartCoroutine(UpdateCCTVView());
    }

    IEnumerator UpdateCCTVView()
    {
        while (true)
        {
            // CCTV 카메라를 활성화하여 해당 프레임을 렌더링
            cctvCamera.Render();
            // 화면을 일정 시간 대기한 후 업데이트
            yield return waitTime;
        }
    }
}

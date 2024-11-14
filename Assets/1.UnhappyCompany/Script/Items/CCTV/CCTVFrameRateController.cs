using System.Collections;
using UnityEngine;

public class CCTVFrameRateController : MonoBehaviour
{
    public Camera cctvCamera; // CCTV ī�޶� ����
    public RenderTexture renderTexture; // CCTV ȭ�鿡 �������� �ؽ�ó
    public float updateInterval = 0.5f; // ȭ�� ������Ʈ ���� (��)

    private WaitForSeconds waitTime;
    private bool isUpdating;

    void Start()
    {
        if (cctvCamera == null)
        {
            Debug.LogError("CCTV ī�޶� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (renderTexture == null)
        {
            Debug.LogError("���� �ؽ�ó�� �Ҵ���� �ʾҽ��ϴ�.");
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
            // CCTV ī�޶� Ȱ��ȭ�Ͽ� �ش� �������� ������
            cctvCamera.Render();
            // ȭ���� ���� �ð� ����� �� ������Ʈ
            yield return waitTime;
        }
    }
}

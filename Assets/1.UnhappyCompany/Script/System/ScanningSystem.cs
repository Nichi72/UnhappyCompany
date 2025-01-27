using UnityEngine;
using System.Collections.Generic;

public class ScanningSystem : MonoBehaviour
{
    [Header("Camera & Target")]
    public Camera targetCamera;
    public LayerMask obstacleLayerMask;    // 가로막을 수 있는 레이어(벽, 장애물 등)

    [Header("Raycast Sampling")]
    [Range(1, 100)]
    public int horizontalSamples = 50;
    [Range(1, 100)]
    public int verticalSamples = 50;

    // 카메라 뷰에 "Enemy"가 있는지 스캔하는 메서드
    public void ScanForEnemies()
    {
        if (targetCamera == null)
            return;

        // (1) N×M 샘플링하여 레이캐스트
        int totalRays = horizontalSamples * verticalSamples;
        int hitCount = 0;

        Dictionary<Transform, EnemyAIData> detectedEnemies = new Dictionary<Transform, EnemyAIData>();
        Dictionary<Transform, Egg> detectedEggs = new Dictionary<Transform, Egg>();
        for (int i = 0; i < horizontalSamples; i++)
        {
            for (int j = 0; j < verticalSamples; j++)
            {
                // viewport X, Y를 [0..1] 사이에서 균등 분할
                float u = (i + 0.5f) / (float)horizontalSamples;  // 샘플 위치를 중앙쯤으로 놓기 위해 +0.5
                float v = (j + 0.5f) / (float)verticalSamples;

                // 각 viewport 포인트를 Ray로 변환
                Ray ray = targetCamera.ViewportPointToRay(new Vector3(u, v, 0f));

                // 레이 길이(대략) - 충분히 멀리까지
                float distance = 100f; // 적절한 거리로 설정

                // 레이캐스트
                if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, obstacleLayerMask))
                {
                    // 맞은 것이 "Enemy" 태그를 가진 오브젝트인지 확인
                    if (hitInfo.transform.CompareTag("Enemy"))
                    {
                        // 적 데이터 가져오기 
                        Egg egg = hitInfo.transform.GetComponent<Egg>();
                        if(egg != null && !detectedEggs.ContainsKey(hitInfo.transform))
                        {
                            if(egg.isScanning == false)
                            {
                                // Egg 타입일때 처리
                                detectedEggs.Add(hitInfo.transform, egg);
                                hitCount++;
                                Debug.Log($"Egg detected: {hitInfo.transform.name}");
                                egg.isScanning = true;
                            }
                        }
                        // 일반 적도 개발해야함
                    }
                }
            }
        }

        // (2) 가시율 계산 및 결과 판정
        if (hitCount > 0)
        {
            Debug.Log($"Total enemies detected: {hitCount}");
            NotificationSystem.instance.ReceiveEnemyData(detectedEnemies, detectedEggs);
        }
        else
        {
            Debug.Log("No enemy detected in view.");
        }
    }
}


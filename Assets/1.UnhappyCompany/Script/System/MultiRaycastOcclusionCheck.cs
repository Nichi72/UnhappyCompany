using UnityEngine;
using System.Collections.Generic;

public class MultiRaycastOcclusionCheck : MonoBehaviour
{
    [Header("Camera & Target")]
    public Camera targetCamera;
    public LayerMask obstacleLayerMask;    // 가로막을 수 있는 레이어(벽, 장애물 등)

    [Header("Raycast Sampling")]
    [Range(1, 100)]
    public int horizontalSamples = 50;
    [Range(1, 100)]
    public int verticalSamples = 50;

    // 가시율(0.0~1.0) 중 몇 % 이상이면 "보인다"고 판정할지
    [Range(0f, 1f)]
    public float visibilityThreshold = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 스캔을 트리거하는 입력을 감지합니다. (예: 스페이스바)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScanForEnemies();
            // Debug.Break();
        }
    }

    // 카메라 뷰에 "Enemy"가 있는지 스캔하는 메서드
    void ScanForEnemies()
    {
        if (targetCamera == null)
            return;

        // (1) N×M 샘플링하여 레이캐스트
        int totalRays = horizontalSamples * verticalSamples;
        int hitCount = 0;
        HashSet<Transform> detectedEnemies = new HashSet<Transform>();

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
                        if (detectedEnemies.Add(hitInfo.transform))
                        {
                            hitCount++;
                            Debug.Log($"Enemy detected: {hitInfo.transform.name}");
                        }
                    }
                }
            }
        }

        // (2) 가시율 계산 및 결과 판정
        if (hitCount > 0)
        {
            Debug.Log($"Total enemies detected: {hitCount}");
            foreach (var enemy in detectedEnemies)
            {
                Debug.Log($"Enemy: {enemy.name}");
            }
        }
        else
        {
            Debug.Log("No enemy detected in view.");
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    [SerializeField] private ObjectTrackerUI objectTrackerUI;
    [SerializeField] private float scanCooldown = 1f; // 쿨타임 설정 (초 단위)

    
    // 카메라 뷰에 "Enemy"가 있는지 스캔하는 메서드
    private bool canScan = true;

    public void ScanForEnemies()
    {
        if (!canScan || targetCamera == null)
            return;

        StartCoroutine(ScanCooldown());

        // (1) N×M 샘플링하여 레이캐스트
        int totalRays = horizontalSamples * verticalSamples;
        int hitCount = 0;

        // 레거시 시스템 호환성 유지를 위한 딕셔너리
        Dictionary<Transform, BaseEnemyAIData> detectedEnemies = new Dictionary<Transform, BaseEnemyAIData>();
        Dictionary<Transform, Egg> detectedEggs = new Dictionary<Transform, Egg>();
        
        // 중복 스캔 방지를 위한 HashSet
        HashSet<IScannable> scannedObjects = new HashSet<IScannable>();
        
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
                    // IScannable 인터페이스를 구현한 객체인지 확인
                    IScannable scannable = hitInfo.transform.GetComponent<IScannable>();
                    if (scannable != null && !scannedObjects.Contains(scannable))
                    {
                        scannedObjects.Add(scannable);
                        
                        // UI 표시 (타입은 IScannable이 알아서 제공)
                        objectTrackerUI.AddTarget(scannable.GetTransform());
                        
                        // 스캔 콜백 호출
                        scannable.OnScanned();
                        
                        hitCount++;
                        Debug.Log($"[Scan] {scannable.GetScanName()} - {scannable.GetScanDescription()}");
                        
                        // 레거시 시스템과의 호환성을 위한 처리 (Egg)
                        if (scannable is Egg egg && !detectedEggs.ContainsKey(hitInfo.transform))
                        {
                            detectedEggs.Add(hitInfo.transform, egg);
                        }
                    }
                }
            }
        }

        // (2) 가시율 계산 및 결과 판정
        if (hitCount > 0)
        {
            Debug.Log($"[Scan] Total {hitCount} objects scanned");
            
            // 레거시 시스템 호환성 유지
            if (detectedEggs.Count > 0)
            {
                NotificationSystem.instance.ReceiveEnemyData(detectedEnemies, detectedEggs);
            }
        }
        else
        {
            Debug.Log("[Scan] No scannable objects detected in view.");
        }
    }

    private IEnumerator ScanCooldown()
    {
        canScan = false;
        yield return new WaitForSeconds(scanCooldown);
        canScan = true;
    }
}


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectTrackerUI : MonoBehaviour
{
    
    
    [SerializeField] private Camera mobileCamera;
    [Header("추적할 타겟 (월드 오브젝트)")]
    // public Transform target; // 월드 공간에 있는 아이템의 Transform

    [Header("연결할 UI 요소 (RectTransform)")]
    // public RectTransform uiElement; // Canvas 하위에 있는 UI 패널이나 텍스트 등

    [Header("UI 프리팹")]
    public GameObject uiPrefab; // 동적으로 생성할 UI 프리팹
    public GameObject enemyUI;
    public GameObject eggUI;
    public GameObject collectibleItemUI;

    [SerializeField] private Canvas canvas;
    [SerializeField] private List<Transform> targets = new List<Transform>();
    [SerializeField] private List<GameObject> uiElements = new List<GameObject>();

    [SerializeField] private float showTime = 3f; // 
    [SerializeField] private Transform initTarget;

    void Start()
    {
        if (canvas == null)
        {
            Debug.LogError("UI 요소가 포함된 Canvas를 찾을 수 없습니다.");
        }
    }



    void LateUpdate()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Transform target = targets[i];
            GameObject uiElement = uiElements[i];

            if (target == null || uiElement == null || canvas == null)
                continue;

            Vector3 screenPos = mobileCamera.WorldToScreenPoint(target.position);
            
            // 카메라 뒤에 있는 오브젝트는 UI 숨기기 (z <= 0이면 카메라 뒤)
            if (screenPos.z <= 0)
            {
                uiElement.SetActive(false);
                continue;
            }
            
            // 화면 밖에 있는 오브젝트도 UI 숨기기
            if (screenPos.x < 0 || screenPos.x > Screen.width || 
                screenPos.y < 0 || screenPos.y > Screen.height)
            {
                uiElement.SetActive(false);
                continue;
            }
            
            // 화면에 보이는 경우에만 UI 활성화 및 위치 업데이트
            uiElement.SetActive(true);
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, 
                screenPos, 
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mobileCamera, 
                out localPoint
            );

            uiElement.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }
    }

    /// <summary>
    /// 스캔 가능한 객체의 UI를 추가합니다.
    /// IScannable 인터페이스를 통해 타입과 정보를 자동으로 결정합니다.
    /// </summary>
    public void AddTarget(Transform newTarget)
    {
        if (newTarget == null || targets.Contains(newTarget))
            return;
        
        // IScannable 인터페이스 확인
        IScannable scannable = newTarget.GetComponent<IScannable>();
        if (scannable == null)
        {
            Debug.LogWarning($"[ObjectTrackerUI] {newTarget.name}에 IScannable 인터페이스가 없습니다.");
            return;
        }
        
        // IScannable에서 타입과 정보 가져오기
        EObjectTrackerUIType targetType = scannable.GetUIType();
        string displayText = scannable.GetScanName();
        
        GameObject newUI = null;
        
        switch (targetType)
        {
            case EObjectTrackerUIType.Enemy:
                newUI = Instantiate(enemyUI, initTarget.transform);
                newUI.GetComponent<ScanInfo>().SetScanInfoText(displayText);
                break;
                
            case EObjectTrackerUIType.Egg:
                // Egg는 특수 처리 (스캔 애니메이션)
                Egg egg = newTarget.GetComponent<Egg>();
                if(egg != null)
                {
                    newUI = Instantiate(eggUI, initTarget.transform);
                    ScanInfo scanInfo = newUI.GetComponent<ScanInfo>();
                    
                    if(egg.isScanningOver)
                    {
                        // 이미 스캔된 상태면 바로 위험도 표시
                        scanInfo.SetScanInfoText(GetEggDangerLevel(egg));
                    }
                    else
                    {
                        // 아직 스캔되지 않은 상태면 Scanning... 표시 후 1초 후 업데이트
                        scanInfo.SetScanInfoText("Scanning...");
                        StartCoroutine(UpdateEggInfoAfterDelay(scanInfo, egg, 1f));
                    }
                }
                break;
                
            case EObjectTrackerUIType.CollectibleItem:
                newUI = Instantiate(collectibleItemUI, initTarget.transform);
                newUI.GetComponent<ScanInfo>().SetScanInfoText(displayText);
                break;
                
            default:
                newUI = Instantiate(uiPrefab, initTarget.transform);
                if(newUI != null)
                {
                    newUI.GetComponent<ScanInfo>()?.SetScanInfoText(displayText);
                }
                break;
        }
        
        // UI 등록 및 활성화
        if(newUI != null)
        {
            targets.Add(newTarget);
            uiElements.Add(newUI);
            newUI.SetActive(true);
            StartCoroutine(RemoveUIAfterDelay(newTarget, newUI, showTime));
        }
    }
    
    // 2초 후에 알 정보 업데이트를 위한 코루틴
    private IEnumerator UpdateEggInfoAfterDelay(ScanInfo scanInfo, Egg egg, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(scanInfo != null && egg != null)
        {
            scanInfo.SetScanInfoText(GetEggDangerLevel(egg));
        }
    }

    private string GetEggDangerLevel(Egg egg)
    {
        return egg.enemyAIData.dangerLevel.ToString();
    }

    private IEnumerator RemoveUIAfterDelay(Transform target, GameObject uiElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        uiElement.SetActive(false);
        uiElements.Remove(uiElement);
        targets.Remove(target);
    }
}

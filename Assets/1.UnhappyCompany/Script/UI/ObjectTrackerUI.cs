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

    // 기존 AddTarget 함수 주석 처리
    /*
    public void AddTarget(Transform newTarget, EObjectTrackerUIType targetType)
    {
        if (newTarget != null && !targets.Contains(newTarget))
        {
            GameObject newUI = null;
            switch (targetType)
            {
                case EObjectTrackerUIType.Enemy:
                    BaseEnemyAIData enemyData = newTarget.GetComponent<BaseEnemyAIData>();
                    string enemyName = "...";
                    if(enemyData != null)
                    {
                        enemyName = enemyData.enemyName;
                    }
                    newUI = Instantiate(enemyUI, initTarget.transform);
                    newUI.GetComponent<ScanInfo>().SetScanInfoText(enemyName);
                    break;
                case EObjectTrackerUIType.Egg:
                    Egg egg = newTarget.GetComponent<Egg>();
                    string scanInfoText = "...";
                    if(egg != null)
                    {
                        if(egg.isScanning == false)
                        {
                            scanInfoText = "Scanning...";
                        }
                        else if(egg.isScanningOver == true)
                        {
                            
                            scanInfoText = $"{egg.eggType.ToString()}";
                        }
                        else
                        {
                            Debug.LogError("스캔 Egg 상태 오류");
                            scanInfoText = "...";
                        }
                    }
                    newUI = Instantiate(eggUI, initTarget.transform);
                    newUI.GetComponent<ScanInfo>().SetScanInfoText(scanInfoText);
                    break;
                case EObjectTrackerUIType.CollectibleItem:
                    Item item = newTarget.GetComponent<Item>();
                    string price = "...";
                    if(item != null)
                    {
                        price = item.itemData.SellPrice.ToString();
                    }
                    
                    newUI = Instantiate(collectibleItemUI, initTarget.transform);
                    newUI.GetComponent<ScanInfo>().SetScanInfoText(price);
                    break;
                default:
                    newUI = Instantiate(uiPrefab, initTarget.transform);
                    break;
            }
            targets.Add(newTarget);
            uiElements.Add(newUI);
            newUI.SetActive(true);
            StartCoroutine(RemoveUIAfterDelay(newTarget, newUI, showTime));
        }
    }
    */

    // 새로운 AddTarget 함수
    public void AddTarget(Transform newTarget, EObjectTrackerUIType targetType)
    {
        if (newTarget != null && !targets.Contains(newTarget))
        {
            GameObject newUI = null;
            switch (targetType)
            {
                case EObjectTrackerUIType.Enemy:
                    BaseEnemyAIData enemyData = newTarget.GetComponent<BaseEnemyAIData>();
                    string enemyName = "...";
                    if(enemyData != null)
                    {
                        enemyName = enemyData.enemyName;
                    }
                    newUI = Instantiate(enemyUI, initTarget.transform);
                    newUI.GetComponent<ScanInfo>().SetScanInfoText(enemyName);
                    break;
                case EObjectTrackerUIType.Egg:
                    Egg egg = newTarget.GetComponent<Egg>();
                    if(egg != null)
                    {
                        newUI = Instantiate(eggUI, initTarget.transform);
                        ScanInfo scanInfo = newUI.GetComponent<ScanInfo>();
                        
                        if(egg.isScanningOver)
                        {
                            // 이미 스캔된 상태면 바로 eggType 표시
                            scanInfo.SetScanInfoText(GetEggDangerLevel(egg));
                        }
                        else
                        {
                            // 아직 스캔되지 않은 상태면 Scanning... 표시 후 1초 후에 eggType으로 변경
                            scanInfo.SetScanInfoText("Scanning...");
                            StartCoroutine(UpdateEggInfoAfterDelay(scanInfo, egg, 1f));
                        }
                    }
                    break;
                case EObjectTrackerUIType.CollectibleItem:
                    // IScannable 인터페이스를 통해 정보 가져오기
                    IScannable scannableItem = newTarget.GetComponent<IScannable>();
                    string displayText = "...";
                    
                    if(scannableItem != null)
                    {
                        // IScannable의 GetScanName()을 사용 (itemData 없어도 작동)
                        displayText = scannableItem.GetScanName();
                    }
                    else
                    {
                        // 레거시 방식 (ItemData 직접 접근)
                        Item item = newTarget.GetComponent<Item>();
                        if(item != null && item.itemData != null)
                        {
                            displayText = item.itemData.SellPrice.ToString();
                        }
                    }
                    
                    newUI = Instantiate(collectibleItemUI, initTarget.transform);
                    newUI.GetComponent<ScanInfo>().SetScanInfoText(displayText);
                    break;
                default:
                    newUI = Instantiate(uiPrefab, initTarget.transform);
                    break;
            }
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

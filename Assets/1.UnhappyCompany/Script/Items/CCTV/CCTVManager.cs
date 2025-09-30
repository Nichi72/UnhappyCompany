using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CCTVManager : MonoBehaviour
{
    public static CCTVManager instance = null;
    public List<CCTV> cctvs; 
    public int currentIndex;
    public Coroutine blinkCoroutine;
    private Color originalColor;
    public Material cctvMaterial;
    public MobileManager mobileManager;

    
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        cctvs = new List<CCTV>();
    }
    void Start()
    {
        currentIndex = 0;
        // TurnOnOnlyOneByCurrentIndex();
        originalColor = cctvMaterial.color;
        
        // MobileManager의 UI 상태 변경 감지
        if (mobileManager != null)
        {
            // mobileManager.OnMobileUIStateChanged가 있는 경우에 활용 가능
        }
    }
    public void NextCCTV()
    {
        ClampIndex(1);
        if(blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        foreach(var cctv in cctvs)
        {
            cctv.CCTVIcon.img.color = originalColor;
        }
        TurnOnOnlyOneByCurrentIndex();
    }

    
    public void BeforeCCTV()
    {
        ClampIndex(-1);
        if(blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        foreach(var cctv in cctvs)
        {
            cctv.CCTVIcon.img.color = originalColor;
        }
        TurnOnOnlyOneByCurrentIndex();
    }
    public void PressedCCTV(CCTV cctv)
    {
        if(blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        foreach(var cctvTemp in cctvs)
        {
            cctvTemp.CCTVIcon.img.color = originalColor;
        }
        TurnOnOnlyOneByCCTV(cctv);
    }

    public void ClampIndex(int amount)
    {
        currentIndex += amount;
        if (currentIndex < 0)
        {
            currentIndex = cctvs.Count - 1;
        }
        else if (cctvs.Count <= currentIndex)
        {
            currentIndex = 0;
        }
    }

    public CCTV TurnOnOnlyOneByCurrentIndex()
    {
        CCTV currentCCTV = null;
        for (int i = 0; i < cctvs.Count; i++)
        {
            if(currentIndex == i)
            {
                currentCCTV = cctvs[i];
                currentCCTV.currentCamera.enabled = true;
                blinkCoroutine = StartCoroutine(BlinkMaterial(currentCCTV.CCTVIcon.img));
            }
            else
            {
                cctvs[i].currentCamera.enabled = false;
            }
        }
        return currentCCTV;
    }
    public void TurnOnOnlyOneByCCTV(CCTV cctv)
    {
        for (int i = 0; i < cctvs.Count; i++)
        {
            if(cctv == cctvs[i])
            {
                // cctvs[i].currentCamera.gameObject.SetActive(true);
                cctvs[i].currentCamera.enabled = true;
                currentIndex = i;
                blinkCoroutine = StartCoroutine(BlinkMaterial(cctvs[i].CCTVIcon.img));
            }
            else
            {
                // cctvs[i].currentCamera.gameObject.SetActive(false);
                cctvs[i].currentCamera.enabled = false;
            }
        }
    }
    
    private IEnumerator BlinkMaterial(Image cctvMaterial)
    {
        float duration = 0.3f;
        while(true)
        {
            if(cctvMaterial == null || !cctvMaterial.isActiveAndEnabled)
            {
                Debug.Log("CCTV 이미지가 더 이상 존재하지 않습니다. 블링크 코루틴을 종료합니다.");
                yield break;
            }

            if(mobileManager == null || mobileManager.uiObjmobile == null || !mobileManager.uiObjmobile.activeSelf)
            {
                yield return null;
                continue;
            }
            
            Color originalColor = cctvMaterial.color;
            Color blinkColor = Color.black;
            yield return new WaitForSeconds(duration);

            if(cctvMaterial == null || !cctvMaterial.isActiveAndEnabled)
            {
                Debug.Log("CCTV 이미지가 더 이상 존재하지 않습니다. 블링크 코루틴을 종료합니다.");
                yield break;
            }

            cctvMaterial.color = blinkColor;
            yield return new WaitForSeconds(duration);
            
            if(cctvMaterial == null || !cctvMaterial.isActiveAndEnabled)
            {
                Debug.Log("CCTV 이미지가 더 이상 존재하지 않습니다. 블링크 코루틴을 종료합니다.");
                yield break;
            }
            
            cctvMaterial.color = originalColor; 
        }
    }

    public bool IsAllCCTVsDisabled()
    {
        foreach(var cctv in cctvs)
        {
            if (cctv.currentCamera.enabled)
            {
                return false;
            }
        }
        return true;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void TurnOnOffAllCCTVCamera(bool isOn)
    {
        foreach(var cctv in cctvs)
        {
            cctv.currentCamera.enabled = isOn;
        }
    }

    public void TurnOnOffCurrentCCTV(bool isOn)
    {
        // 리스트가 비어있거나 인덱스가 범위를 벗어나는지 체크
        if (cctvs == null || cctvs.Count == 0 || currentIndex < 0 || currentIndex >= cctvs.Count)
        {
            Debug.LogWarning("CCTV 리스트가 비어있거나 currentIndex가 유효하지 않습니다: " + currentIndex);
            return;
        }

        if(cctvs[currentIndex] == null)
        {
            return;
        }
        cctvs[currentIndex].currentCamera.enabled = isOn;
    }
}

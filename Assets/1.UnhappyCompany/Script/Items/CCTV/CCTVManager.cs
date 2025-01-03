using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CCTVManager : MonoBehaviour
{
    public static CCTVManager instance = null;
    public List<CCTV> cctvs; // 총 3개라면 인덱스는 2까지
    public int currentIndex;
    public Coroutine blinkCoroutine;
    private Color originalColor;
    public Material cctvMaterial;

    
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
        TurnOnOnlyOneByCurrentIndex();
        originalColor = cctvMaterial.color;
    }

    public void NextCCTV()
    {
        ClampIndex(1);
        StopCoroutine(blinkCoroutine);
        foreach(var cctv in cctvs)
        {
            cctv.CCTVIcon.img.color = originalColor;
        }
        TurnOnOnlyOneByCurrentIndex();
        
    }

    public void BeforeCCTV()
    {
        ClampIndex(-1);
        StopCoroutine(blinkCoroutine);
        foreach(var cctv in cctvs)
        {
            cctv.CCTVIcon.img.color = originalColor;
        }
        TurnOnOnlyOneByCurrentIndex();
    }
    public void PressedCCTV(CCTV cctv)
    {
        StopCoroutine(blinkCoroutine);
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

    public void TurnOnOnlyOneByCurrentIndex()
    {
        for (int i = 0; i < cctvs.Count; i++)
        {
            if(currentIndex == i)
            {
                cctvs[i].currentCamera.gameObject.SetActive(true);
                blinkCoroutine = StartCoroutine(BlinkMaterial(cctvs[i].CCTVIcon.img));
            }
            else
            {
                cctvs[i].currentCamera.gameObject.SetActive(false);
            }
        }
    }
    public void TurnOnOnlyOneByCCTV(CCTV cctv)
    {
        for (int i = 0; i < cctvs.Count; i++)
        {
            if(cctv == cctvs[i])
            {
                cctvs[i].currentCamera.gameObject.SetActive(true);
                currentIndex = i;
                blinkCoroutine = StartCoroutine(BlinkMaterial(cctvs[i].CCTVIcon.img));
            }
            else
            {
                cctvs[i].currentCamera.gameObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator BlinkMaterial(Image cctvMaterial)
    {
        float duration = 0.3f;
        while(true)
        {
            Color originalColor = cctvMaterial.color;
            Color blinkColor = Color.black;
            yield return new WaitForSeconds(duration);

            // Debug.Log($"{cctvMaterial.color} BlinkMaterial");
            cctvMaterial.color = blinkColor;
            yield return new WaitForSeconds(duration);
            cctvMaterial.color = originalColor; 
            // Debug.Log($"{cctvMaterial.color} BlinkMaterial2");
        }
    }
   
}

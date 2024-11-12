using System.Collections.Generic;
using UnityEngine;

public class CCTVManager : MonoBehaviour
{
    public static CCTVManager instance = null;
    public List<CCTV> cctvs; // ÃÑ 3°³¶ó¸é ÀÎµ¦½º´Â 2±îÁö
    public int currentIndex;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        cctvs = new List<CCTV>();
        currentIndex = 0;
    }

    public void NextCCTV()
    {
        ClampIndex(1);
        //var tempCctv = cctvs[currentIndex];
        TurnOnOnlyOne();
    }

    public void BeforeCCTV()
    {
        ClampIndex(-1);
        //var tempCctv = cctvs[currentIndex];
        TurnOnOnlyOne();
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

    public void TurnOnOnlyOne()
    {
        for (int i = 0; i < cctvs.Count; i++)
        {
            if(currentIndex == i)
            {
                cctvs[i].currentCamera.gameObject.SetActive(true);
            }
            else
            {
                cctvs[i].currentCamera.gameObject.SetActive(false);
            }
        }
    }

   
}

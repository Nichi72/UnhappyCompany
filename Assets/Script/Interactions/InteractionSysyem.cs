using System.Collections.Generic;
using UnityEngine;

public class InteractionSysyem : MonoBehaviour
{
    
    private List<GameObject> interactionObjs;
    public GameObject CenterText;

    private void Awake()
    {
        interactionObjs = new List<GameObject>();
        interactionObjs.Add(CenterText);
    }

    // Update is called once per frame
    void Update()
    {
        // 처음 모든 오브젝트 비활성화 
        interactionObjs.ForEach(obj => obj.SetActive(false));
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // RaycastUtility의 PerformRaycastWithTag 사용 예시
        if (UtilityRaycast.PerformRaycastWithTag(ray, out hit, Tag.RaycastHit.ToString()))
        {
            var hitEvent = hit.transform.GetComponent<InteractionF>();
            //Debug.Log($"{hit.transform.name} 오브젝트를 성공적으로 검출했습니다!");
            CenterText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                hitEvent.HitEvent();
            }
        }
        else
        {
            Debug.Log("TargetTag 오브젝트를 찾지 못했습니다.");
        }
    }
}

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
        // ó�� ��� ������Ʈ ��Ȱ��ȭ 
        interactionObjs.ForEach(obj => obj.SetActive(false));
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // RaycastUtility�� PerformRaycastWithTag ��� ����
        if (UtilityRaycast.PerformRaycastWithTag(ray, out hit, Tag.RaycastHit.ToString()))
        {
            var hitEvent = hit.transform.GetComponent<InteractionF>();
            //Debug.Log($"{hit.transform.name} ������Ʈ�� ���������� �����߽��ϴ�!");
            CenterText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                hitEvent.HitEvent();
            }
        }
        else
        {
            Debug.Log("TargetTag ������Ʈ�� ã�� ���߽��ϴ�.");
        }
    }
}

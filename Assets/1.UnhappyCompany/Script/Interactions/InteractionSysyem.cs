using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public GameObject CenterText;
    public float raycastMaxDistance = 5f; // Raycast의 최대 거리
    public LayerMask interactionLayer; // 레이어 마스크 설정
    private List<GameObject> interactionObjs;

    Player player;

    private void Awake()
    {
        interactionObjs = new List<GameObject>();
        interactionObjs.Add(CenterText);
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        // 최소 하나의 오브젝트 비활성화
        interactionObjs.ForEach(obj => obj.SetActive(false));

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 레이어 마스크 Raycast 실행
        if (Physics.Raycast(ray, out hit, raycastMaxDistance, interactionLayer))
        {
            var hitEvent = hit.transform.GetComponent<IInteractable>();
            CenterText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                if(hitEvent != null)
                {
                    hitEvent.HitEventInteractionF(player);
                }
            }
        }
        else
        {
            //Debug.Log("TargetLayer 오브젝트를 찾지 못했습니다.");
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public TextMeshProUGUI CenterText;
    public float raycastMaxDistance = 5f; // Raycast의 최대 거리
    public LayerMask interactionLayer; // 레이어 마스크 설정
    private List<GameObject> interactionObjs;

    Player player;

    private void Awake()
    {
        interactionObjs = new List<GameObject>();
        interactionObjs.Add(CenterText.transform.parent.gameObject);
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
            if(hitEvent != null)
            {
                CenterText.transform.parent.gameObject.SetActive(true);
                CenterText.text = hitEvent.InteractionText;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    hitEvent.HitEventInteractionF(player);
                    Debug.Log(hitEvent.InteractionText);
                }
            }
        }
        else
        {
            //Debug.Log("TargetLayer 오브젝트를 찾지 못했습니다.");
        }
    }
}

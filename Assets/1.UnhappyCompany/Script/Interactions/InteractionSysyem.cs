using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem instance;
    public TextMeshProUGUI CenterText;
    public TextMeshProUGUI CenterTextE;
    public float raycastMaxDistance = 5f; // Raycast의 최대 거리
    public LayerMask interactionLayer; // 레이어 마스크 설정
    private List<GameObject> interactionObjs;
    public bool isInteraction = false;

    Player player;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        interactionObjs = new List<GameObject>();
        interactionObjs.Add(CenterText.transform.parent.gameObject);
        interactionObjs.Add(CenterTextE.transform.parent.gameObject);

        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isInteraction == false)
        {
            return;
        }
        // 최소 하나의 오브젝트 비활성화
        interactionObjs.ForEach(obj => obj.SetActive(false));

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 레이어 마스크 Raycast 실행
        if (Physics.Raycast(ray, out hit, raycastMaxDistance, interactionLayer))
        {
            var hitEventF = hit.transform.GetComponent<IInteractableF>();
            if(hitEventF != null)
            {
                if(hitEventF.IgnoreInteractionF == true)
                {
                    return;
                }
                CenterText.transform.parent.gameObject.SetActive(true);
                CenterText.text = hitEventF.InteractionTextF;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    hitEventF.HitEventInteractionF(player);
                    Debug.Log(hitEventF.InteractionTextF);
                }
               
            }

            var hitEventE = hit.transform.GetComponent<IInteractableE>();
            if(hitEventE != null)
            {
                if(hitEventE.IgnoreInteractionE == true)
                {
                    return;
                }
                CenterTextE.transform.parent.gameObject.SetActive(true);
                CenterTextE.text = hitEventE.InteractionTextE;
                 if (Input.GetKeyDown(KeyCode.E))
                {
                    hitEventE.HitEventInteractionE(player);
                    Debug.Log(hitEventE.InteractionTextE);
                }
                
            }
        }
        else
        {
            //Debug.Log("TargetLayer 오브젝트를 찾지 못했습니다.");
        }
    }
}

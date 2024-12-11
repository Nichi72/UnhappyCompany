using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public GameObject CenterText;
    public float raycastMaxDistance = 5f; // Raycast�� �ִ� ����
    public LayerMask interactionLayer; // ���̾ ������ ����
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
        // �ּ� �ϳ��� ������Ʈ ��Ȱ��ȭ
        interactionObjs.ForEach(obj => obj.SetActive(false));

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ���̾ ���� Raycast ����
        if (Physics.Raycast(ray, out hit, raycastMaxDistance, interactionLayer))
        {
            var hitEvent = hit.transform.GetComponent<IInteractable>();
            CenterText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                hitEvent.HitEventInteractionF(player);
            }
        }
        else
        {
            //Debug.Log("TargetLayer ������Ʈ�� ã�� ���߽��ϴ�.");
        }

       
    }
}

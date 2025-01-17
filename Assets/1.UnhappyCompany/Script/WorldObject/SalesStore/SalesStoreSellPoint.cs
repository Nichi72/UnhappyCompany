using System.Collections;
using UnityEngine;

public class SalesStoreSellPoint : MonoBehaviour , IInteractable
{
    [SerializeField] private GameObject slider;
    [SerializeField] private bool isOpen = false;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private SalesStore salesStore;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // slider.transform.localPosition = targetPosition;
    }

    private bool isToggling = false;

    public IEnumerator ToggleDoor()
    {
        Debug.Log("ToggleDoor");
        if (isToggling) yield break; // 이미 실행 중이면 중단

        isToggling = true; // 실행 중으로 설정
        Debug.Log("ToggleDoor2");

        float targetZ;
        if (isOpen)
        {
            targetZ = 0f;
        }
        else
        {
            targetZ = -1.6f;
        }

        float speed = 7f; // 문이 열리고 닫히는 속도
        float step = speed * Time.deltaTime;

        float elapsedTime = 0f;
        Vector3 startingPos = slider.transform.localPosition;
        Vector3 targetPos = new Vector3(slider.transform.localPosition.x, slider.transform.localPosition.y, targetZ);
        float duration = 0.3f; // 보간에 걸리는 시간

        while (elapsedTime < duration)
        {
            slider.transform.localPosition = Vector3.Lerp(startingPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        slider.transform.localPosition = targetPos; // 정확한 목표 위치로 설정

        isOpen = !isOpen;
        isToggling = false; // 실행 완료 후 해제
    }

    public void HitEventInteractionF(Player rayOrigin)
    {
        // StartCoroutine(ToggleDoor());
        var temp = rayOrigin.quickSlotSystem.DropItem();
        if (temp != null)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-slider.transform.localScale.x / 2, slider.transform.localScale.x / 2),
                Random.Range(-slider.transform.localScale.y / 2, slider.transform.localScale.y / 2),
                Random.Range(-slider.transform.localScale.z / 2, slider.transform.localScale.z / 2)
            );

            temp.transform.SetParent(slider.transform);
            temp.transform.localPosition = randomPosition;
            salesStore.AddSellObject(temp);
            
            
        }
       
    }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     if(other.tag != Tag.Item.ToString())
    //     {
    //         Debug.Log($"OnTriggerEnter {other.name} ");
    //         return;
    //     }
    //     var temp = other.GetComponent<Item>();
    //     if (temp == null)
    //     {
    //         Debug.LogError("Item�� ���������ʽ��ϴ�.");
    //         return;
    //     }

    //     GameManager.instance.totalGold += temp.itemData.SellPrice;
    //     Destroy(other.gameObject);
    // }
}

using UnityEngine;

public class DoorTrapReloadBottle : MonoBehaviour, IInteractableF
{
    public GameObject bottle;
    public Transform bottlePos;
    [SerializeField] private DoorTrapBtnType doorTrapBtnType;
    

    // 사출 관련 설정
    [Header("병 사출 설정")]
    [SerializeField] private float ejectForce = 5f;           // 사출 힘
    [SerializeField] private float ejectUpwardForce = 2f;     // 위쪽 힘 (포물선 궤도)
    [SerializeField] private float randomSideForce = 1f;      // 좌우 랜덤 힘
    [SerializeField] private float rotationForce = 300f;      // 회전력
    [SerializeField] private float destroyDelay = 3f;        // 병 제거 딜레이

    public bool IgnoreInteractionF { get; set; } = false;
    public string InteractionTextF { get; set; } = "통 장착하기";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(bottle == null)
        {
            bottle.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void HitEventInteractionF(Player rayOrigin)
    {
        Debug.Log("DoorTrapReloadBottle HitEventInteractionF");
        // 장착
        if(bottle == null)
        {
            var currentItemObject = rayOrigin.quickSlotSystem.currentItemObject;
            if(currentItemObject == null)
            {
                Debug.Log("Bottle을 장착 해야합니다.");
                return;
            }
            if(doorTrapBtnType == DoorTrapBtnType.Fire)
            {
                ItemBottleFire itemBottleFire = currentItemObject.GetComponent<ItemBottleFire>();
                if(itemBottleFire != null)
                {
                    itemBottleFire = rayOrigin.quickSlotSystem.DropItem().GetComponent<ItemBottleFire>();
                    SetBottle(currentItemObject);
                    itemBottleFire.enabled = false;
                }
                else
                {
                    Debug.Log("BottleFire을 장착 해야합니다.");
                    return;
                }
                
            }
            else if(doorTrapBtnType == DoorTrapBtnType.Water)
            {
                ItemBottleWater itemBottleWater = currentItemObject.GetComponent<ItemBottleWater>();
                if(itemBottleWater != null)
                {
                    itemBottleWater = rayOrigin.quickSlotSystem.DropItem().GetComponent<ItemBottleWater>();
                    SetBottle(currentItemObject);
                    itemBottleWater.enabled = false;
                }
                else
                {
                    Debug.Log("BottleWater을 장착 해야합니다.");
                    return;
                }
            }
            else if(doorTrapBtnType == DoorTrapBtnType.Hammer)
            {
                ItemBottleGas itemBottleGas = currentItemObject.GetComponent<ItemBottleGas>();
                if(itemBottleGas != null)
                {
                    itemBottleGas = rayOrigin.quickSlotSystem.DropItem().GetComponent<ItemBottleGas>();
                    SetBottle(currentItemObject);
                    itemBottleGas.enabled = false;
                }
                else
                {
                    Debug.Log("Bottle을 장착 해야합니다.");
                    return;
                }
            }
        }
        // 사출
        else if (bottle != null)
        {
            EjectBottle();
        }
        void SetBottle(GameObject itemBottleFire)
        {
            itemBottleFire.transform.SetParent(transform);
            itemBottleFire.transform.localPosition = Vector3.zero;
            itemBottleFire.transform.localRotation = Quaternion.identity;
            bottle = itemBottleFire;
            itemBottleFire.GetComponent<Collider>().enabled = false;
            itemBottleFire.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void EjectBottle()
    {
        if (bottle == null) return;
        bottle.layer = LayerMask.NameToLayer("Default"); // 인터렉션 불가능하게 처리.
        // Rigidbody 컴포넌트 확인 및 추가
        Rigidbody bottleRb = bottle.GetComponent<Rigidbody>();
        if (bottleRb == null)
        {
            bottleRb = bottle.AddComponent<Rigidbody>();
        }
        
        bottleRb.isKinematic = false;
        bottle.GetComponent<Collider>().enabled = true;
        // 충돌 비활성화

        // 병을 부모에서 분리 (독립적으로 움직이게)
        bottle.transform.SetParent(null);

        // 사출 방향 계산 (뒤쪽 + 위쪽 + 랜덤 좌우)
        Vector3 ejectDirection = -transform.forward * ejectForce;  // Z축 마이너스 방향으로 변경
        ejectDirection += Vector3.up * ejectUpwardForce;
        ejectDirection += transform.right * Random.Range(-randomSideForce, randomSideForce);

        // 힘 적용
        bottleRb.AddForce(ejectDirection, ForceMode.Impulse);

        // 랜덤 회전력 적용 (탄피처럼 회전하며 날아가게)
        Vector3 randomRotation = new Vector3(
            Random.Range(-rotationForce, rotationForce),
            Random.Range(-rotationForce, rotationForce),
            Random.Range(-rotationForce, rotationForce)
        );
        bottleRb.AddTorque(randomRotation);


        // 일정 시간 후 병 제거
        Destroy(bottle, destroyDelay);

        // 병 참조 해제
        bottle = null;
    }
}

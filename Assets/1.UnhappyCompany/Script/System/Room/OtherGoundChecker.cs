using Unity.VisualScripting;
using UnityEngine;

public class OtherGoundChecker : MonoBehaviour
{
    public bool flagCheck = false;
    public bool isOverlap = false;
    private Rigidbody rb;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        // 물리 업데이트마다 속도를 0으로 리셋
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    void OnCollisionStay(Collision other)
    {
        // Debug.Log("OnCollisionEnter");
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Debug.Log("OnTriggerEnter");
 
            isOverlap = true;
            if(IsSameParent(other.gameObject)) // 같은 부모면 오버랩 아님
            {
                isOverlap = false;
            }
        }
        flagCheck = true;
    }
    private bool IsSameParent(GameObject other)
    {
        Transform otherParent = other.transform;
        Transform thisParent = transform;

        // 최상위 부모를 찾을 때까지 반복
        while (otherParent.parent != null)
        {
            otherParent = otherParent.parent;
        }
        while (thisParent.parent != null) 
        {
            thisParent = thisParent.parent;
        }

        // 최상위 부모가 같은지 확인
        if (otherParent == thisParent)
        {
            return true;
        }
        return false;
    }
}

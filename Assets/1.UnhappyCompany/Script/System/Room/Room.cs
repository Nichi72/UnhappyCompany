using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
public class Room : MonoBehaviour
{
    public List<OtherGoundChecker> groundList = new List<OtherGoundChecker>();
    public bool isOverlap = false;
    public int depth = 0;

    
    void Awake()
    {
       
    }
    void Update()
    {
        isOverlap = false;
        foreach (var ground in groundList)
        {
            // 하나라도 오버랩이 있으면 오버랩임
            if (ground.isOverlap)
            {
                isOverlap = true;
                break;
            }
        }
    }
    void OnDrawGizmos()
    {
        // Ground 레이어를 가진 모든 자식 오브젝트 찾기
        foreach (Transform child in transform)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                // 콜라이더 컴포넌트 가져오기
                Collider collider = child.GetComponent<Collider>();
                if (collider != null)
                {
                    // 박스 콜라이더인 경우
                    if (collider is BoxCollider)
                    {
                        BoxCollider boxCollider = (BoxCollider)collider;
                        Gizmos.color = Color.red;
                        Matrix4x4 rotationMatrix = Matrix4x4.TRS(child.position, child.rotation, child.lossyScale);
                        Gizmos.matrix = rotationMatrix;
                        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                    }
                    // 메시 콜라이더인 경우
                    else if (collider is MeshCollider)
                    {
                        MeshCollider meshCollider = (MeshCollider)collider;
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireMesh(meshCollider.sharedMesh, child.position, child.rotation, child.lossyScale);
                    }
                }
            }
        }
    }
    [ContextMenu("Generate New Map")]
    public void SetGround()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                var otherGoundCheckerTemp = child.AddComponent<OtherGoundChecker>();
                Rigidbody rb = child.AddComponent<Rigidbody>(); 
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.isKinematic = false;

                groundList.Add(otherGoundCheckerTemp);
            }
        }
    }
}

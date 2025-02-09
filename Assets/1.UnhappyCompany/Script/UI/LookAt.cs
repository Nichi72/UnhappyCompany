using UnityEngine;

public class LookAt : MonoBehaviour
{
    // 바라볼 대상 오브젝트의 Transform
    public Transform target;

    // 추가적인 회전 보정을 위한 변수
    public Vector3 rotationOffset;

    void Start()
    {
        if(target == null)
        {
            target = MobileManager.instance.mobileCamera.transform;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // 모든 축이 대상 오브젝트를 바라보도록 설정
            transform.LookAt(target.position, Vector3.up);

            // 추가적인 회전 보정 적용
            transform.rotation = transform.rotation * Quaternion.Euler(rotationOffset);
        }
    }
}

using UnityEngine;

public class ForTest : MonoBehaviour
{
    public float addforcePower = 100;

    public Transform target;  // ť�갡 �̵��� ��ǥ ����
    public float moveForce = 10f;  // ť�꿡 ���� ���� ũ��
    public float maxVelocity = 5f; // ť���� �ִ� �ӷ�
    private Rigidbody rb;
    void Start()
    {
        // Rigidbody ������Ʈ ��������
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (target != null)
            {
                MoveTowardsTarget();
            }
        }
      
    }
   
    void MoveTowardsTarget()
    {
        // ��ǥ ���������� ���� ���
        Vector3 direction = (target.position - transform.position).normalized;

        // ���� �ӵ��� �ִ� �ӵ��� ���� �ʵ��� ����
        if (rb.GetPointVelocity(transform.position).sqrMagnitude < maxVelocity * maxVelocity)
        {
            // ť�꿡 ���� ���� �̵���Ű��
            rb.AddForce(direction * moveForce);
        }
    }
}

using UnityEngine;

public class Fortest : MonoBehaviour
{
    public Transform player;  // �÷��̾ ���󰡱� ���� �ʿ�
    public float forceStrength = 10f;  // ť�긦 ������ ���� ����
    public float maxSpeed = 5f;  // ť���� �ִ� �ӵ�
    public float restTime = 1f;  // ���� �� ���� �ð�
    public float chargeForce = 50f;  // ������ ���� ��
    public float chargeDelay = 5f;  // ���� ���� �ð�
    public float rotationForce = 20f;  // ���� �� ȸ���ϴ� ���� ����
    public AudioClip collisionSound;  // �浹 ȿ����
    public AudioSource audioSource;  // ����� �ҽ� ������Ʈ
    public float rayLength = 1f;  // ����ĳ��Ʈ ����
    public Vector3 raycastOffset = Vector3.zero;  // ����ĳ��Ʈ ���� ��ġ ������ ������
    public float minPitch = 0.8f;  // ȿ������ �ּ� ��ġ
    public float maxPitch = 1.2f;  // ȿ������ �ִ� ��ġ

    private Rigidbody rb;
    private bool isResting = false;
    private float restTimer = 0f;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private bool[] rayHits = new bool[6];

    void Start()
    {
        // Rigidbody ������Ʈ�� �����ɴϴ�.
        rb = GetComponent<Rigidbody>();

        // ����� �ҽ� ������Ʈ Ȯ��
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void FixedUpdate()
    {
        if (player != null && !isResting && !isCharging)
        {
            // �÷��̾� ��ġ�� ť�� ��ġ ���� ������ ����մϴ�.
            Vector3 direction = (player.position - transform.position).normalized;

            // ���� �ӵ� ������ ���� �ӵ� ���͸� Ȯ���մϴ�.
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                // ���� �߰��� ť�긦 �����ϴ�.
                rb.AddForce(direction * forceStrength);
            }
            else
            {
                // �ִ� �ӵ��� �����ϸ� ���� Ÿ�̸Ӹ� �����մϴ�.
                isResting = true;
                restTimer = restTime;
            }

            // ���� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
            chargeTimer -= Time.fixedDeltaTime;
            if (chargeTimer <= 0f)
            {
                // ���� ����
                isCharging = true;
                chargeTimer = chargeDelay;
            }
        }
        else if (isResting)
        {
            // ���� �ð��� ó���մϴ�.
            restTimer -= Time.fixedDeltaTime;
            if (restTimer <= 0f)
            {
                isResting = false;
            }
        }
        else if (isCharging)
        {
            // �÷��̾ ���� ���ϰ� �����մϴ�.
            Vector3 chargeDirection = (player.position - transform.position).normalized;
            rb.AddForce(chargeDirection * chargeForce, ForceMode.Impulse);

            // ȸ�� ȿ�� �߰�
            rb.AddTorque(Random.onUnitSphere * rotationForce);

            // ���� �� ���� Ÿ�̸Ӹ� �����մϴ�.
            isCharging = false;
            isResting = true;
            restTimer = restTime;
        }
    }

    void Update()
    {
       
    }

    void AudioCheckRaycast()
    {
        // �� �鿡�� ����ĳ��Ʈ�� �߻��Ͽ� ���鿡 ��Ҵ��� Ȯ���մϴ�.
        RaycastHit hit;
        Vector3[] rayDirections = { Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 rayOrigin = transform.position + transform.rotation * raycastOffset;
            if (Physics.Raycast(rayOrigin, transform.rotation * rayDirections[i], out hit, rayLength))
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject && !rayHits[i])
                {
                    // ���鿡 ����� �� ȿ������ ����մϴ�.
                    if (collisionSound != null && audioSource != null)
                    {
                        // ��ġ�� �����ϰ� �����Ͽ� �Ҹ��� ���ݾ� �ٸ��� �鸮���� ��
                        audioSource.pitch = Random.Range(minPitch, maxPitch);
                        audioSource.PlayOneShot(collisionSound);
                    }
                    rayHits[i] = true;
                }
            }
            else
            {
                rayHits[i] = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        // ������ ����ĳ��Ʈ�� �ð�ȭ�մϴ�.
        DrawGizmosForAudioCheckRaycast();

    }

    void DrawGizmosForAudioCheckRaycast()
    {
        Gizmos.color = Color.red;
        Vector3[] rayDirections = { Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        foreach (Vector3 direction in rayDirections)
        {
            Vector3 rayOrigin = transform.position + transform.rotation * raycastOffset;
            Gizmos.DrawRay(rayOrigin, transform.rotation * direction * rayLength);
        }
    }
}
using UnityEngine;

public class Fortest : MonoBehaviour
{
    public Transform player;  // 플레이어를 따라가기 위해 필요
    public float forceStrength = 10f;  // 큐브를 굴리는 힘의 세기
    public float maxSpeed = 5f;  // 큐브의 최대 속도
    public float restTime = 1f;  // 굴린 후 쉬는 시간
    public float chargeForce = 50f;  // 돌진할 때의 힘
    public float chargeDelay = 5f;  // 돌진 간격 시간
    public float rotationForce = 20f;  // 돌진 시 회전하는 힘의 세기
    public AudioClip collisionSound;  // 충돌 효과음
    public AudioSource audioSource;  // 오디오 소스 컴포넌트
    public float rayLength = 1f;  // 레이캐스트 길이
    public Vector3 raycastOffset = Vector3.zero;  // 레이캐스트 시작 위치 조절용 오프셋
    public float minPitch = 0.8f;  // 효과음의 최소 피치
    public float maxPitch = 1.2f;  // 효과음의 최대 피치

    private Rigidbody rb;
    private bool isResting = false;
    private float restTimer = 0f;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private bool[] rayHits = new bool[6];

    void Start()
    {
        // Rigidbody 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody>();

        // 오디오 소스 컴포넌트 확인
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void FixedUpdate()
    {
        if (player != null && !isResting && !isCharging)
        {
            // 플레이어 위치와 큐브 위치 간의 방향을 계산합니다.
            Vector3 direction = (player.position - transform.position).normalized;

            // 현재 속도 제한을 위해 속도 벡터를 확인합니다.
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                // 힘을 추가해 큐브를 굴립니다.
                rb.AddForce(direction * forceStrength);
            }
            else
            {
                // 최대 속도에 도달하면 쉬는 타이머를 시작합니다.
                isResting = true;
                restTimer = restTime;
            }

            // 돌진 타이머를 감소시킵니다.
            chargeTimer -= Time.fixedDeltaTime;
            if (chargeTimer <= 0f)
            {
                // 돌진 시작
                isCharging = true;
                chargeTimer = chargeDelay;
            }
        }
        else if (isResting)
        {
            // 쉬는 시간을 처리합니다.
            restTimer -= Time.fixedDeltaTime;
            if (restTimer <= 0f)
            {
                isResting = false;
            }
        }
        else if (isCharging)
        {
            // 플레이어를 향해 강하게 돌진합니다.
            Vector3 chargeDirection = (player.position - transform.position).normalized;
            rb.AddForce(chargeDirection * chargeForce, ForceMode.Impulse);

            // 회전 효과 추가
            rb.AddTorque(Random.onUnitSphere * rotationForce);

            // 돌진 후 쉬는 타이머를 시작합니다.
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
        // 각 면에서 레이캐스트를 발사하여 지면에 닿았는지 확인합니다.
        RaycastHit hit;
        Vector3[] rayDirections = { Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 rayOrigin = transform.position + transform.rotation * raycastOffset;
            if (Physics.Raycast(rayOrigin, transform.rotation * rayDirections[i], out hit, rayLength))
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject && !rayHits[i])
                {
                    // 지면에 닿았을 때 효과음을 재생합니다.
                    if (collisionSound != null && audioSource != null)
                    {
                        // 피치를 랜덤하게 설정하여 소리가 조금씩 다르게 들리도록 함
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
        // 기즈모로 레이캐스트를 시각화합니다.
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
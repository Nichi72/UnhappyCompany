using UnityEngine;

public class Fortest : MonoBehaviour
{
    public Transform player;  // ÷̾ 󰡱 ʿ
    public float forceStrength = 10f;  // ť긦   ӵ
    public float maxSpeed = 5f;  // ťִ ӵ
    public float restTime = 1f;  //  ð
    public float chargeForce = 50f;  //   ӵ
    public float chargeDelay = 5f;  //  ð
    public float rotationForce = 20f;  //  ȸϴ  ӵ
    public AudioClip collisionSound;  // 浹 ȿ
    public AudioSource audioSource;  // ҽ Ʈ
    public float rayLength = 1f;  // ĳƮ 
    public Vector3 raycastOffset = Vector3.zero;  // ĳƮ ġ  
    public float minPitch = 0.8f;  // ȿּ ġ
    public float maxPitch = 1.2f;  // ȿִ ġ 테스트

    private Rigidbody rb;
    private bool isResting = false;
    private float restTimer = 0f;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private bool[] rayHits = new bool[6];

    void Start()
    {
        // Rigidbody Ʈɴϴ.
        rb = GetComponent<Rigidbody>();

        // ҽ Ʈ Ȯ
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void FixedUpdate()
    {
        if (player != null && !isResting && !isCharging)
        {
            // ÷̾ ġ ť ġ մϴ.
            Vector3 direction = (player.position - transform.position).normalized;

            // ӵ ӵ ͸ Ȯմϴ.
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                // ť긦 ϴ.
                rb.AddForce(direction * forceStrength);
            }
            else
            {
                // ִ ӵϸ Ÿ̸Ӹ մϴ.
                isResting = true;
                restTimer = restTime;
            }

            // Ÿ̸Ӹ ҽŵϴ.
            chargeTimer -= Time.fixedDeltaTime;
            if (chargeTimer <= 0f)
            {
                // 
                isCharging = true;
                chargeTimer = chargeDelay;
            }
        }
        else if (isResting)
        {
            // ð óմϴ.
            restTimer -= Time.fixedDeltaTime;
            if (restTimer <= 0f)
            {
                isResting = false;
            }
        }
        else if (isCharging)
        {
            // ÷̾ ϰ մϴ.
            Vector3 chargeDirection = (player.position - transform.position).normalized;
            rb.AddForce(chargeDirection * chargeForce, ForceMode.Impulse);

            // ȸ ȿ ߰
            rb.AddTorque(Random.onUnitSphere * rotationForce);

            //  ȸ̸Ӹ մϴ.
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
        // 鿡 ĳƮ߻Ͽ 鿡 Ҵ Ȯմϴ.
        RaycastHit hit;
        Vector3[] rayDirections = { Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 rayOrigin = transform.position + transform.rotation * raycastOffset;
            if (Physics.Raycast(rayOrigin, transform.rotation * rayDirections[i], out hit, rayLength))
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject && !rayHits[i])
                {
                    // 鿡  ȿ մϴ.
                    if (collisionSound != null && audioSource != null)
                    {
                        // ġ ϰ Ͽ Ҹ ݾ ٸ 鸮 
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
        // ĳƮðȭմϴ.
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
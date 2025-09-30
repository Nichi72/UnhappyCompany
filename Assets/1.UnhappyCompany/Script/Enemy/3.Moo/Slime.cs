using StarterAssets;
using UnityEngine;

public class Slime : MonoBehaviour
{
    public float duration = 5f; // 점액의 지속 시간
    public float slowAmount = 0.5f; // 이동 속도 감소 비율
    private bool playerInSlime = false;
    private FirstPersonController playerController;

    private void Start()
    {
        // 점액이 일정 시간 후 사라지도록 설정
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<Player>().firstPersonController;
            if (playerController != null)
            {
                playerController.ModifySpeed(slowAmount);
                playerInSlime = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInSlime)
        {
            ResetPlayerSpeed();
        }
    }

    private void OnDestroy()
    {
        if (playerInSlime)
        {
            ResetPlayerSpeed();
        }
    }

    private void ResetPlayerSpeed()
    {
        if (playerController != null)
        {
            playerController.ResetSpeed();
            playerInSlime = false;
        }
    }
} 
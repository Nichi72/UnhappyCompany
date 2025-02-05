using System.Collections;
using UnityEngine;
using MyUtility;

public class NormalDoor : MonoBehaviour , IInteractable
{
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openSpeed = 90f; // 문이 열리는 속도
    [SerializeField] private float closeSpeed = 90f; // 문이 닫히는 속도
    
    private bool isOpen = false;
    private bool isMoving = false; // 문이 움직이는 중인지 체크하는 변수

    public string InteractionText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "NormalDoor_ITR"); set => InteractionText = value; }

    public void HitEventInteractionF(Player rayOrigin)
    {
        if (isMoving) return; // 문이 움직이는 중이면 실행하지 않음
        
        if (!isOpen)
        {
            StartCoroutine(DoorCoroutine(true));
        }
        else
        {
            StartCoroutine(DoorCoroutine(false));
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OpenDoor()
    {
        if (!isMoving) // 문이 움직이는 중이 아닐 때만 실행
        {
            StartCoroutine(DoorCoroutine(true));
        }
    }

    private IEnumerator DoorCoroutine(bool open)
    {
        isMoving = true; // 문 움직임 시작
        float targetRotation = open ? 90f : 0f;
        float currentRotation = doorPivot.localEulerAngles.y;
        float rotationSpeed = open ? openSpeed : -closeSpeed;

        // 현재 회전값을 0-360에서 -180~180 범위로 변환
        if (currentRotation > 180f)
            currentRotation -= 360f;

        while (Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation)) > 0.1f)
        {
            float step = rotationSpeed * Time.deltaTime;
            currentRotation = Mathf.MoveTowards(currentRotation, targetRotation, Mathf.Abs(step));
            
            doorPivot.localEulerAngles = new Vector3(0, currentRotation, 0);
            
            yield return null;
        }

        // 정확한 목표 각도로 설정
        doorPivot.localEulerAngles = new Vector3(0, targetRotation, 0);
        
        isOpen = open;
        isMoving = false; // 문 움직임 종료
    }
}

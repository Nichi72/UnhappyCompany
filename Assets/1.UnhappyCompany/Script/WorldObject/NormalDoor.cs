using System.Collections;
using UnityEngine;
using MyUtility;

public class NormalDoor : MonoBehaviour , IInteractable
{
    public enum DoorType
    {
        For1,
        For2
    }
    public DoorType doorType = DoorType.For1;
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openSpeed = 90f; // 문이 열리는 속도
    [SerializeField] private float closeSpeed = 90f; // 문이 닫히는속도
    private float openAngleFor1 = 90f;
    private float openAngleFor2 = 270f;
    private float closeAngleFor1 = 0f;
    private float closeAngleFor2 = 180f;
    
    [ReadOnly][SerializeField] private bool isOpen = false;
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

    private void OpenDoor()
    {
        if (!isMoving) // 문이 움직이는 중이 아닐 때만 실행
        {
            StartCoroutine(DoorCoroutine(true));
        }
    }

    private IEnumerator DoorCoroutine(bool open)
    {
        isMoving = true;
        
        float openAngle = doorType == DoorType.For1 ? openAngleFor1 : openAngleFor2;
        float closeAngle = doorType == DoorType.For1 ? closeAngleFor1 : closeAngleFor2;
        float targetRotation = open ? openAngle : closeAngle;
        
        // 현재 각도를 0~360 범위로 정규화
        float currentRotation = doorPivot.localEulerAngles.y;
        
        // 회전 방향 결정
        float direction;
        if (open)
        {
            // 열 때는 항상 시계방향으로 회전
            direction = 1f;
        }
        else
        {
            // 닫을 때는 항상 반시계방향으로 회전
            direction = -1f;
        }
        
        float rotationSpeed = open ? openSpeed : closeSpeed;

        while (Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation)) > 0.1f)
        {
            float step = rotationSpeed * Time.deltaTime * direction;
            currentRotation = Mathf.MoveTowards(currentRotation, targetRotation, Mathf.Abs(step));
            
            // 각도를 0~360 범위로 유지
            if (currentRotation < 0) currentRotation += 360f;
            if (currentRotation >= 360f) currentRotation -= 360f;
            
            doorPivot.localEulerAngles = new Vector3(0, currentRotation, 0);
            yield return null;
        }

        // 최종 각도 설정
        doorPivot.localEulerAngles = new Vector3(0, targetRotation, 0);
        
        isOpen = open;
        isMoving = false;
    }
}

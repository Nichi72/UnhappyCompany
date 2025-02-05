using System.Collections;
using UnityEngine;
using MyUtility;

public class Door : CentralBatteryConsumer
{
    [SerializeField] private float doorBatteryDrainPerSecond = 3f;

    public override float BatteryDrainPerSecond
    {
        get => doorBatteryDrainPerSecond;
        set => doorBatteryDrainPerSecond = value;
    }

    public enum DoorState
    {
        Open,
        Close
    }

    public enum DoorOpening
    {
        Ing,
        End
    }

    public Transform closeTransform;
    public Transform openTransform;
    public float lerpSpeed = 1.0f;
    private Coroutine lerpCoroutine;
    public DoorState currentDoorState;
    public DoorOpening currentDoorOpening;

    void Start()
    {
        currentDoorState = DoorState.Open;
        currentDoorOpening = DoorOpening.End;
        transform.position = openTransform.position;
        CentralBatterySystem.Instance.RegisterConsumer(this); // 중앙 배터리 시스템에 등록
    }

    public override void DrainBattery()
    {
        // 닫혔을 때만 배터리 소모
        if (currentDoorState == DoorState.Close)
        {
            base.DrainBattery(); // 기본 배터리 소모 기능 사용
        }
    }

    public void OpenCloseDoor()
    {
        if (currentDoorOpening == DoorOpening.Ing)
        {
            Debug.Log("현재 열리는 중입니다.");
            return;
        }

        lerpCoroutine = StartCoroutine(LerpMovement());
    }

    IEnumerator LerpMovement()
    {
        if (closeTransform != null && openTransform != null)
        {
            float lerpTime = 0f;
            Vector3 startPoint;
            Vector3 endPoint;

            if (currentDoorState == DoorState.Close)
            {
                transform.position = closeTransform.position;
                // 열기
                startPoint = closeTransform.position;
                endPoint = openTransform.position;
            }
            else // 현재 열림 상태
            {
                transform.position = openTransform.position;
                // 닫기
                startPoint = openTransform.position;
                endPoint = closeTransform.position;
            }

            currentDoorOpening = DoorOpening.Ing;

            while (lerpTime < 1f)
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                lerpTime = Mathf.Clamp01(lerpTime);

                Vector3 newPosition = VectorLerpUtility.FastToSlowLerp(startPoint, endPoint, lerpTime);
                transform.position = newPosition;

                yield return null;
            }

            // 상태 업데이트
            currentDoorOpening = DoorOpening.End;
            currentDoorState = currentDoorState == DoorState.Close ? DoorState.Open : DoorState.Close;
        }
        else
        {
            Debug.LogError("startTransform 또는 endTransform이 누락되었습니다.");
        }
    }
}

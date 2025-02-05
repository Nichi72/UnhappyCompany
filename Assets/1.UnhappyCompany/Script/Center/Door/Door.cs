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
        CentralBatterySystem.Instance.RegisterConsumer(this); // �߾� ���͸� �ý��ۿ� ���
    }

    public override void DrainBattery()
    {
        // ������ ���� ���͸� �Ҹ�
        if (currentDoorState == DoorState.Close)
        {
            base.DrainBattery(); // �⺻ ���͸� �Ҹ� ��� ���
        }
    }

    public void OpenCloseDoor()
    {
        if (currentDoorOpening == DoorOpening.Ing)
        {
            Debug.Log("���� ������ ���Դϴ�.");
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
                // ����
                startPoint = closeTransform.position;
                endPoint = openTransform.position;
            }
            else // ���� ���� ����
            {
                transform.position = openTransform.position;
                // �ݱ�
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

            // ���� ������Ʈ
            currentDoorOpening = DoorOpening.End;
            currentDoorState = currentDoorState == DoorState.Close ? DoorState.Open : DoorState.Close;
        }
        else
        {
            Debug.LogError("startTransform �Ǵ� endTransform�� �����Ǿ����ϴ�.");
        }
    }
}

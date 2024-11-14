using System.Collections;
using UnityEngine;
using MyUtility;

public class Door : CentralBatteryConsumer
{
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
        // ���������� ���͸� �Ҹ�
        if(currentDoorState == DoorState.Close)
        {
            base.DrainBattery(); // �⺻ ���͸� �Ҹ� ��� ���
        }
    }

    public void OpenCloseDoor()
    {
        if(currentDoorOpening == DoorOpening.Ing)
        {
            Debug.Log("���� ������ ���Դϴ�.");
            return;
        }

        lerpCoroutine = StartCoroutine(LerpMovement());
    }

    IEnumerator LerpMovement()
    {
        if(closeTransform != null && openTransform != null)
        {
            float lerpTime = 0f;
            Vector3 startPoiont;
            Vector3 endPoint;
            if (currentDoorState == DoorState.Close)
            {
                transform.position = closeTransform.position;
                // To Open
                startPoiont = closeTransform.position;
                endPoint = openTransform.position;
            }
            else // current Open
            {
                transform.position = openTransform.position;
                // To Close
                startPoiont = openTransform.position;
                endPoint = closeTransform.position;
            }


            while (lerpTime < 1f)
            {
                currentDoorOpening = DoorOpening.Ing;

                lerpTime += Time.deltaTime * lerpSpeed;
                lerpTime = Mathf.Clamp01(lerpTime);

                // VectorLerpUtility�� ����Ͽ� �� ���� ���̸� Lerp�� �̵�
                Vector3 newPosition = VectorLerpUtility.FastToSlowLerp(startPoiont, endPoint, lerpTime);
                transform.position = newPosition;

                yield return null;
            }

            // ��ó��
            currentDoorOpening = DoorOpening.End;
            if (currentDoorState == DoorState.Close)
            {
                currentDoorState = DoorState.Open;
            }
            else if (currentDoorState == DoorState.Open)
            {
                currentDoorState = DoorState.Close;
            }
        }
        else
        {
            Debug.LogError("startTransform endTransform �� ������ �ֽ��ϴ�.");
        }
        
    }
}

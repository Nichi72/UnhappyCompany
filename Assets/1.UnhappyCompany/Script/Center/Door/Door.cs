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
    [ReadOnly] public bool shouldGenerate = false;
    public DoorEdge doorEdge;
    public Transform closeTransform;
    public Transform openTransform;
    public float lerpSpeed = 1.0f;
    private Coroutine lerpCoroutine;
    
    [SerializeField] private DoorState _currentDoorState;
    public DoorState currentDoorState
    {
        get => _currentDoorState;
        set
        {
            if (_currentDoorState != value)
            {
                DoorState oldState = _currentDoorState;
                _currentDoorState = value;
                OnDoorStateChanged(oldState, _currentDoorState);
            }
        }
    }
    
    public DoorOpening currentDoorOpening;

    // 문 상태가 변경될 때 호출되는 함수
    private void OnDoorStateChanged(DoorState oldState, DoorState newState)
    {
        Debug.Log($"Door state changed from {oldState} to {newState}");
        
        // 새 상태에 따라 적절한 함수 호출
        if (newState == DoorState.Close)
        {
            OnDoorClosed();
        }
        else if (newState == DoorState.Open)
        {
            OnDoorOpened();
        }
    }
    
    // 문이 닫힐 때 호출되는 함수
    private void OnDoorClosed()
    {
        Debug.Log("문이 닫혔습니다.");
        if (shouldGenerate)
        {
            CentralBatterySystem.Instance.RegisterConsumer(this);
        }
    }
    
    // 문이 열릴 때 호출되는 함수
    private void OnDoorOpened()
    {
        Debug.Log("문이 열렸습니다.");
        CentralBatterySystem.Instance.UnregisterConsumer(this);
    }

    protected override void Start()
    {
        // ID 설정: Door_[DoorEdge ID]_[위치 해시]
        if (string.IsNullOrEmpty(id))
        {
            string doorEdgeId = doorEdge != null ? doorEdge.GetInstanceID().ToString() : "noDoorEdge";
            string positionHash = transform.position.GetHashCode().ToString();
            id = $"Door_{doorEdgeId}_{positionHash}";
            Debug.Log($"문 ID 할당: {id}");
        }

        currentDoorState = DoorState.Open;
        currentDoorOpening = DoorOpening.End;
        transform.position = openTransform.position;
        var temp = RoomManager.Instance.roomGenerator.doorGenerationSettings;
        foreach (var item in temp)
        {
            if(item.door == doorEdge)
            {
                shouldGenerate = item.shouldGenerate;
            }
        }
        
        if (shouldGenerate)
        {
            base.Start(); // 부모 클래스의 Start 메서드 호출 (배터리 소비자 등록)
        }
    }

    void Update()
    {
        if(shouldGenerate == false)
        {
            CentralBatterySystem.Instance.UnregisterConsumer(this);
            InstantCloseDoor();
        }
    }

    public override void DrainBattery()
    {
        var currentGameState = GameManager.instance.currentGameState;
        if (currentGameState == EGameState.Ready && currentGameState == EGameState.None)
        {
            return;
        }
        if (currentDoorState == DoorState.Close)
        {
            base.DrainBattery(); // 기본 소모량 소모
        }
    }

    public void OpenCloseDoor()
    {
        if (currentDoorOpening == DoorOpening.Ing)
        {
            Debug.Log("이미 문이 열리고 있습니다.");
            return;
        }

        lerpCoroutine = StartCoroutine(LerpMovement());
    }
    public void OpenDoor()
    {
        if(shouldGenerate == false)
        {
            Debug.Log($"{doorEdge.name} : shouldGenerate = false");
            return;
        }
        if (currentDoorState == DoorState.Close)
        {
            lerpCoroutine = StartCoroutine(LerpMovement());
        }
    }

    public void CloseDoor()
    {
        // if(shouldGenerate == false)
        // {
        //     Debug.Log($"{doorEdge.name} : shouldGenerate = false");
        //     return;
        // }
        Debug.Log($"currentDoorState : {currentDoorState}");
        if(currentDoorState == DoorState.Open)
        {
            Debug.Log("문이 닫힙니다.");
            lerpCoroutine = StartCoroutine(LerpMovement());
        }
    }

   

    private void InstantOpenDoor()
    {
        if (currentDoorState == DoorState.Close)
        {
            transform.position = openTransform.position;
            currentDoorState = DoorState.Open;
        }
    }

    public void InstantCloseDoor()
    {
        // if (currentDoorState == DoorState.Open)
        {
            transform.position = closeTransform.position;
            currentDoorState = DoorState.Close;
        }
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
                // 닫힌 위치
                startPoint = closeTransform.position;
                endPoint = openTransform.position;
            }
            else // 열린 위치
            {
                transform.position = openTransform.position;
                // 열린 위치
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

            // 문이 열리거나 닫힘
            currentDoorOpening = DoorOpening.End;
            currentDoorState = currentDoorState == DoorState.Close ? DoorState.Open : DoorState.Close;
        }
        else
        {
            Debug.LogError("startTransform 또는 endTransform이 설정되지 않았습니다.");
        }
    }
}

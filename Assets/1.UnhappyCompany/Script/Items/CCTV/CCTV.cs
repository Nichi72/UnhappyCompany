using UnityEngine;

public class CCTV : CentralBatteryConsumerItem, IMinimapTrackable , IInteractableE
{
    public Camera currentCamera;
    public bool isTurnOn = true;

    public GameObject minimapQuad;

    [SerializeField] private GameObject iconPrefab; // UI 아이콘 프리팹

    public GameObject CCTVIconPrefab => iconPrefab;

    public string InteractionTextE { get => isTurnOn ? "CCTV 끄기" : "CCTV 켜기"; set => InteractionTextE = value; }

    [ReadOnly] public MinimapUIBtnCCTV CCTVIcon;

    void Start()
    {
        InitCCTV();
        
        isTurnOn = false;
        OnTurnOff();
    }

    private void OnDestroy()
    {
        isTurnOn = false;
        try 
        {
            CentralBatterySystem.Instance.UnregisterConsumer(this);
            CCTVManager.instance.cctvs.Remove(this);
            if (CCTVIcon != null) 
            {
                UIManager.instance.RemoveCCTVButton(CCTVIcon.gameObject);
            }
            else
            {
                Debug.LogWarning("CCTVIcon이 null입니다.");
            }
            
            OnMinimapRemove();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("CCTV OnDestroy 오류: " + e.Message);
        }
    }
    

    // 배터리 소모 메서드 (기본 동작을 유지하거나 수정 가능)
    public override void DrainBattery()
    {
        if(isTurnOn == true)
        {
            base.DrainBattery(); // 기본 배터리 소모 기능 사용
                                 // 추가적인 행동이 필요하다면 여기에 작성
        }
    }

    public override void Use(Player player) 
    {
        player.buildSystem.StartPlacing(itemData.prefab.gameObject, this.gameObject); // 설치 모드 시작
    }

    private void InitCCTV()
    {
       
    }

    public void OnMinimapAdd()
    {
        if (CCTVIconPrefab != null)
        {
            // IconPrefab.GetComponent<MinimapUIBtnCCTV>().cctv = this;
            var icon = MinimapController.instance.AddMinimapIcon(transform, CCTVIconPrefab);
            icon.GetComponent<MinimapUIBtnCCTV>().cctv = this;
            CCTVIcon = icon.GetComponent<MinimapUIBtnCCTV>();
        }
        else
        {
            Debug.LogError("CCTVIconPrefab이 null입니다.");
        }
    }

    public void OnMinimapRemove()
    {
        if (MinimapController.instance != null && this != null && transform != null && gameObject != null && gameObject.activeInHierarchy)
        {
            try
            {
                MinimapController.instance.RemoveMinimapIcon(transform);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("CCTV OnMinimapRemove 오류: " + e.Message);
            }
        }
    }

    public override void HitEventInteractionF(Player player)
    {
        if(player.buildSystem.isPlacing == true)
        {
            Debug.Log("배치 중입니다. HitEventInteractionF를 실행하지 않습니다.");
            return;
        }
        base.HitEventInteractionF(player);
    }

    public void HitEventInteractionE(Player rayOrigin)
    {
        if(isTurnOn == true)
        {
            isTurnOn = false;
            OnTurnOff();
        }
        else
        {
            isTurnOn = true;
            OnTurnOn();
        }
    }

    /// <summary>
    /// CCTV가 켜질 때 실행되는 함수
    /// </summary>
    private void OnTurnOn()
    {
        Debug.Log("CCTV가 켜졌습니다.");
        // CCTV가 켜질 때 실행할 추가 코드 작성
        CentralBatterySystem.Instance.RegisterConsumer(this);
        CCTVManager.instance.cctvs.Add(this); // CCTV 매니저에 등록
        UIManager.instance.InitCCTVButton(); // UI 매니저에 미니맵 버튼 등록
        OnMinimapAdd();

    }

    /// <summary>
    /// CCTV가 꺼질 때 실행되는 함수
    /// </summary>
    private void OnTurnOff()
    {
        Debug.Log("CCTV가 꺼졌습니다.");
        // CCTV가 꺼질 때 실행할 추가 코드 작성
        CentralBatterySystem.Instance.UnregisterConsumer(this);
        currentCamera.enabled = false;
        CCTVManager.instance.cctvs.Remove(this); // CCTV 매니저에서 제거
        if (CCTVIcon != null) 
        {
            UIManager.instance.RemoveCCTVButton(CCTVIcon.gameObject); // UI 매니저에서 미니맵 버튼 제거
        }
    }
}

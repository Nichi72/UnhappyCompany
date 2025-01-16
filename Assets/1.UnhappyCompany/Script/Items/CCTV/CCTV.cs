using UnityEngine;

public class CCTV : CentralBatteryConsumerItem, IMinimapTrackable
{
    public Camera currentCamera;
    public bool isTurnOn = true;

    public GameObject minimapQuad;

    [SerializeField] private GameObject iconPrefab; // UI 아이콘 프리팹

    public GameObject CCTVIconPrefab => iconPrefab;
    [ReadOnly]public MinimapUIBtnCCTV CCTVIcon;

    void Start()
    {
        InitCCTV();
        OnMinimapAdd();
    }

    private void OnDestroy()
    {
        CCTVManager.instance.cctvs.Remove(this);
        CentralBatterySystem.Instance.UnregisterConsumer(this); // 중앙 배터리 시스템에서 등록 해제
        OnMinimapRemove();
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
        CCTVManager.instance.cctvs.Add(this); // CCTV 매니저에 등록
        UIManager.instance.InitCCTVButton(); // UI 매니저에 미니맵 버튼 등록
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
    }

    public void OnMinimapRemove()
    {
        MinimapController.instance.RemoveMinimapIcon(transform);
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 팝업 시스템 사용법을 보여주는 테스트 컨트롤러
/// </summary>
public class PopupTestController : MonoBehaviour
{
    [Header("테스트 버튼들")]
    [SerializeField] private Button confirmPopupButton;
    [SerializeField] private Button infoPopupButton;
    [SerializeField] private Button settingsPopupButton;
    [SerializeField] private Button closeAllButton;
    
    private void Start()
    {
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        // 확인 팝업 테스트
        if (confirmPopupButton != null)
        {
            confirmPopupButton.onClick.AddListener(() => {
                ShowConfirmPopupExample();
            });
        }
        
        // 정보 팝업 테스트
        if (infoPopupButton != null)
        {
            infoPopupButton.onClick.AddListener(() => {
                ShowInfoPopupExample();
            });
        }
        
        // 설정 팝업 테스트
        if (settingsPopupButton != null)
        {
            settingsPopupButton.onClick.AddListener(() => {
                ShowSettingsPopupExample();
            });
        }
        
        // 모든 팝업 닫기
        if (closeAllButton != null)
        {
            closeAllButton.onClick.AddListener(() => {
                PopupManager.Instance.CloseAllPopups();
            });
        }
    }
    
    #region 팝업 예제들
    
    /// <summary>
    /// 확인 팝업 예제
    /// </summary>
    private void ShowConfirmPopupExample()
    {
        ConfirmPopup.Show(
            "정말로 게임을 종료하시겠습니까?",
            onConfirm: () => {
                Debug.Log("게임 종료 확인됨");
                // Application.Quit(); // 실제 게임에서는 이렇게 사용
            },
            onCancel: () => {
                Debug.Log("게임 종료 취소됨");
            },
            title: "게임 종료"
        );
    }
    
    /// <summary>
    /// 정보 팝업 예제 (Resources에서 로드)
    /// </summary>
    private void ShowInfoPopupExample()
    {
        // Resources 폴더에서 팝업 프리팹 로드
        var infoPopup = PopupManager.Instance.OpenPopupFromPrefab<InfoPopup>("Popups/InfoPopup");
        if (infoPopup != null)
        {
            infoPopup.ShowInfo(
                "미션 완료!",
                "모든 목표를 성공적으로 달성했습니다.\n보상을 획득하였습니다.",
                null // 아이콘은 선택사항
            );
        }
    }
    
    /// <summary>
    /// 설정 팝업 예제 (직접 생성)
    /// </summary>
    private void ShowSettingsPopupExample()
    {
        // 설정 팝업이 이미 열려있는지 확인
        if (PopupManager.Instance.IsPopupOpen<SettingsPopup>())
        {
            Debug.Log("설정 팝업이 이미 열려있습니다!");
            return;
        }
        
        // 새로운 설정 팝업 열기
        var settingsPopup = PopupManager.Instance.OpenPopupFromPrefab<SettingsPopup>("Popups/SettingsPopup");
        if (settingsPopup != null)
        {
            Debug.Log("설정 팝업이 열렸습니다.");
        }
    }
    
    #endregion
    
    #region 고급 사용법 예제
    
    /// <summary>
    /// 여러 팝업 순차적으로 열기 예제
    /// </summary>
    [ContextMenu("Sequential Popups Test")]
    private void ShowSequentialPopups()
    {
        // 첫 번째 팝업
        ConfirmPopup.Show(
            "첫 번째 팝업입니다.",
            onConfirm: () => {
                // 두 번째 팝업
                ConfirmPopup.Show(
                    "두 번째 팝업입니다.",
                    onConfirm: () => {
                        // 세 번째 팝업
                        ConfirmPopup.Show(
                            "마지막 팝업입니다!",
                            onConfirm: () => Debug.Log("모든 팝업 완료!")
                        );
                    }
                );
            }
        );
    }
    
    /// <summary>
    /// 팝업 이벤트 리스닝 예제
    /// </summary>
    [ContextMenu("Listen to Popup Events")]
    private void ListenToPopupEvents()
    {
        // 팝업 열림/닫힘 이벤트 구독
        PopupManager.Instance.OnPopupOpened += OnPopupOpened;
        PopupManager.Instance.OnPopupClosed += OnPopupClosed;
        
        // 테스트 팝업 열기
        ConfirmPopup.Show("이벤트 테스트 팝업", () => Debug.Log("확인!"));
    }
    
    private void OnPopupOpened(BasePopup popup)
    {
        Debug.Log($"[이벤트] 팝업 열림: {popup.GetType().Name}");
    }
    
    private void OnPopupClosed(BasePopup popup)
    {
        Debug.Log($"[이벤트] 팝업 닫힘: {popup.GetType().Name}");
    }
    
    /// <summary>
    /// 비동기 팝업 열기 예제
    /// </summary>
    [ContextMenu("Async Popup Test")]
    private void ShowAsyncPopup()
    {
        PopupManager.Instance.OpenPopupAsync(() => {
            ConfirmPopup.Show("비동기로 열린 팝업입니다!", () => Debug.Log("비동기 팝업 확인!"));
        });
    }
    
    /// <summary>
    /// 특정 타입 팝업 닫기 예제
    /// </summary>
    [ContextMenu("Close Specific Popup Type")]
    private void CloseSpecificPopupType()
    {
        bool closed = PopupManager.Instance.ClosePopup<ConfirmPopup>();
        Debug.Log(closed ? "ConfirmPopup이 닫혔습니다." : "열린 ConfirmPopup이 없습니다.");
    }
    
    #endregion
    
    #region 유틸리티
    
    private void Update()
    {
        // F1키로 팝업 상태 확인
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"활성 팝업 수: {PopupManager.Instance.ActivePopupCount}");
            if (PopupManager.Instance.HasActivePopups)
            {
                var currentPopup = PopupManager.Instance.GetCurrentPopup();
                Debug.Log($"현재 팝업: {currentPopup.GetType().Name}");
            }
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 해제
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.OnPopupOpened -= OnPopupOpened;
            PopupManager.Instance.OnPopupClosed -= OnPopupClosed;
        }
    }
    
    #endregion
}

// 추가 예제 팝업들 (실제로는 별도 파일로 만들어야 함)

/// <summary>
/// 정보 표시 팝업 예제
/// </summary>
public class InfoPopup : BasePopup
{
    [Header("정보 팝업 UI")]
    [SerializeField] private TMPro.TextMeshProUGUI messageText;
    [SerializeField] private Image iconImage;
    
    public void ShowInfo(string title, string message, Sprite icon = null)
    {
        popupTitle = title;
        
        if (titleText != null)
            titleText.text = title;
            
        if (messageText != null)
            messageText.text = message;
            
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(icon != null);
            if (icon != null)
                iconImage.sprite = icon;
        }
        
        OpenPopup();
    }
}

/// <summary>
/// 설정 팝업 예제
/// </summary>
public class SettingsPopup : BasePopup
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        popupTitle = "설정";
        blockBackground = true;
        pauseGameWhenOpen = true; // 설정 중에는 게임 일시정지
    }
    
    protected override void OnPopupOpened()
    {
        base.OnPopupOpened();
        Debug.Log("설정 팝업이 열렸습니다. 게임이 일시정지됩니다.");
    }
    
    protected override void OnPopupClosed()
    {
        base.OnPopupClosed();
        Debug.Log("설정 팝업이 닫혔습니다. 게임이 재개됩니다.");
    }
} 
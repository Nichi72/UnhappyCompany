using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 확인/취소 다이얼로그 팝업 예제
/// BasePopup을 상속받아 구현
/// </summary>
public class ConfirmPopup : BasePopup
{
    [Header("확인 팝업 전용 UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    // 콜백 액션
    private System.Action onConfirmAction;
    private System.Action onCancelAction;
    
    #region 초기화
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        // 기본 설정
        popupTitle = "확인";
        blockBackground = true;
        canCloseWithEscape = true;
        
        // 버튼 이벤트 연결
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
    }
    
    #endregion
    
    #region 공개 메서드
    
    /// <summary>
    /// 확인 팝업 설정 및 열기
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="onConfirm">확인 버튼 클릭 시 콜백</param>
    /// <param name="onCancel">취소 버튼 클릭 시 콜백</param>
    /// <param name="title">팝업 제목 (선택사항)</param>
    public void ShowConfirm(string message, System.Action onConfirm, System.Action onCancel = null, string title = "확인")
    {
        // 설정 저장
        onConfirmAction = onConfirm;
        onCancelAction = onCancel;
        popupTitle = title;
        
        // 메시지 설정
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        // 제목 업데이트
        if (titleText != null)
        {
            titleText.text = popupTitle;
        }
        
        // 팝업 열기
        OpenPopup();
    }
    
    /// <summary>
    /// 빠른 확인 팝업 생성 (정적 메서드)
    /// </summary>
    public static void Show(string message, System.Action onConfirm, System.Action onCancel = null, string title = "확인")
    {
        // PopupManager를 통해 팝업 생성
        var popup = PopupManager.Instance.OpenPopupFromPrefab<ConfirmPopup>("Popups/ConfirmPopup");
        if (popup != null)
        {
            popup.ShowConfirm(message, onConfirm, onCancel, title);
        }
    }
    
    #endregion
    
    #region 이벤트 처리
    
    /// <summary>
    /// 확인 버튼 클릭 처리
    /// </summary>
    private void OnConfirmClicked()
    {
        onConfirmAction?.Invoke();
        ClosePopup();
    }
    
    /// <summary>
    /// 취소 버튼 클릭 처리
    /// </summary>
    private void OnCancelClicked()
    {
        onCancelAction?.Invoke();
        ClosePopup();
    }
    
    #endregion
    
    #region 팝업 이벤트 오버라이드
    
    protected override void OnPopupOpened()
    {
        base.OnPopupOpened();
        
        // 확인 버튼에 포커스 설정
        if (confirmButton != null)
        {
            confirmButton.Select();
        }
    }
    
    protected override void OnPopupClosed()
    {
        base.OnPopupClosed();
        
        // 콜백 초기화
        onConfirmAction = null;
        onCancelAction = null;
    }
    
    #endregion
} 
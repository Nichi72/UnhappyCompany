using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class BasePopup : MonoBehaviour
{
    [Header("팝업 기본 설정")]
    [SerializeField] protected string popupTitle = "팝업";
    [SerializeField] protected bool blockBackground = true;
    [SerializeField] protected bool pauseGameWhenOpen = false;
    [SerializeField] protected bool canCloseWithEscape = true;
    [SerializeField] protected bool destroyOnClose = false;
    
    [Header("UI 요소")]
    [SerializeField] protected Button closeButton;
    [SerializeField] protected TextMeshProUGUI titleText;
    [SerializeField] protected Transform contentParent;
    [SerializeField] protected CanvasGroup canvasGroup;
    
    [Header("애니메이션 설정")]
    [SerializeField] protected PopupAnimationType animationType = PopupAnimationType.Scale;
    [SerializeField] protected float animationDuration = 0.3f;
    [SerializeField] protected AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected Vector3 startScale = Vector3.zero;
    [SerializeField] protected Vector3 endScale = Vector3.one;
    
    [Header("사운드")]
    [SerializeField] protected AudioClip openSound;
    [SerializeField] protected AudioClip closeSound;
    
    // 상태 관리
    private bool isOpen = false;
    private bool isAnimating = false;
    private bool isPaused = false;
    
    // 이벤트
    public System.Action OnPopupOpenComplete;
    public System.Action OnPopupCloseComplete;
    public System.Action<BasePopup> OnPopupDestroyed;
    
    // 프로퍼티
    public string PopupTitle => popupTitle;
    public bool BlockBackground => blockBackground;
    public bool CanCloseWithEscape => canCloseWithEscape && !isAnimating;
    public bool IsOpen => isOpen;
    public bool IsAnimating => isAnimating;
    public bool IsPaused => isPaused;
    
    #region Unity 생명주기
    
    protected virtual void Awake()
    {
        InitializePopup();
    }
    
    protected virtual void Start()
    {
        SetupUI();
    }
    
    protected virtual void OnDestroy()
    {
        OnPopupDestroyed?.Invoke(this);
        
        // PopupManager에서 제거
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.NotifyPopupClosed(this);
        }
    }
    
    #endregion
    
    #region 초기화
    
    /// <summary>
    /// 팝업 기본 초기화
    /// </summary>
    private void InitializePopup()
    {
        // CanvasGroup 자동 설정
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // 초기 상태 설정
        if (animationType == PopupAnimationType.Scale)
        {
            transform.localScale = startScale;
        }
        else if (animationType == PopupAnimationType.Fade)
        {
            canvasGroup.alpha = 0f;
        }
        
        // 버튼 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => {
                // 클릭 사운드 재생
                if (AudioManager.instance != null && FMODEvents.instance != null)
                {
                    AudioManager.instance.PlayUISound(FMODEvents.instance.computerCursorClick, "컴퓨터 커서 클릭");
                }
                ClosePopup();
            });
        }
        
        OnInitialize();
    }
    
    /// <summary>
    /// UI 요소 설정
    /// </summary>
    private void SetupUI()
    {
        // 제목 설정
        if (titleText != null)
        {
            titleText.text = popupTitle;
        }
        
        OnSetupUI();
    }
    
    #endregion
    
    #region 팝업 제어
    
    /// <summary>
    /// 팝업 열기
    /// </summary>
    public virtual void OpenPopup()
    {
        if (isOpen || isAnimating) return;
        
        gameObject.SetActive(true);
        isOpen = true;
        
        // PopupManager에 등록
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.OpenPopup(this);
            PopupManager.Instance.NotifyPopupOpened(this);
        }
        
        // 게임 일시정지
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 0f;
        }
        
        // 사운드 재생
        PlayOpenSound();
        
        // 애니메이션 재생
        PlayOpenAnimation();
        
        OnPopupOpened();
    }
    
    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public virtual void ClosePopup()
    {
        if (!isOpen || isAnimating) return;
        
        OnPopupClosing();
        
        // 애니메이션 재생
        PlayCloseAnimation(() => {
            CompleteClose();
        });
    }
    
    /// <summary>
    /// 강제로 팝업 닫기 (애니메이션 없음)
    /// </summary>
    public virtual void ForceClose()
    {
        if (!isOpen) return;
        
        OnPopupClosing();
        CompleteClose();
    }
    
    /// <summary>
    /// 팝업 닫기 완료 처리
    /// </summary>
    private void CompleteClose()
    {
        isOpen = false;
        isPaused = false;
        
        // 게임 재개
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 1f;
        }
        
        // 사운드 재생
        PlayCloseSound();
        
        // PopupManager에서 제거
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.CloseCurrentPopup();
        }
        
        OnPopupClosed();
        OnPopupCloseComplete?.Invoke();
        
        // 파괴 또는 비활성화
        if (destroyOnClose)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    #endregion
    
    #region 상태 관리
    
    /// <summary>
    /// 팝업 일시정지 (다른 팝업이 열렸을 때)
    /// </summary>
    public virtual void OnPause()
    {
        isPaused = true;
        canvasGroup.interactable = false;
        OnPopupPaused();
    }
    
    /// <summary>
    /// 팝업 재개 (위에 있던 팝업이 닫혔을 때)
    /// </summary>
    public virtual void OnResume()
    {
        isPaused = false;
        canvasGroup.interactable = true;
        OnPopupResumed();
    }
    
    #endregion
    
    #region 애니메이션
    
    /// <summary>
    /// 열림 애니메이션 재생
    /// </summary>
    protected virtual void PlayOpenAnimation()
    {
        if (isAnimating) return;
        
        isAnimating = true;
        
        switch (animationType)
        {
            case PopupAnimationType.Scale:
                PlayScaleAnimation(startScale, endScale, () => {
                    isAnimating = false;
                    OnPopupOpenComplete?.Invoke();
                });
                break;
                
            case PopupAnimationType.Fade:
                PlayFadeAnimation(0f, 1f, () => {
                    isAnimating = false;
                    OnPopupOpenComplete?.Invoke();
                });
                break;
                
            case PopupAnimationType.Slide:
                PlaySlideAnimation(true, () => {
                    isAnimating = false;
                    OnPopupOpenComplete?.Invoke();
                });
                break;
                
            case PopupAnimationType.Custom:
                PlayCustomOpenAnimation(() => {
                    isAnimating = false;
                    OnPopupOpenComplete?.Invoke();
                });
                break;
                
            default:
                isAnimating = false;
                OnPopupOpenComplete?.Invoke();
                break;
        }
    }
    
    /// <summary>
    /// 닫힘 애니메이션 재생
    /// </summary>
    protected virtual void PlayCloseAnimation(System.Action onComplete)
    {
        if (isAnimating) return;
        
        isAnimating = true;
        
        switch (animationType)
        {
            case PopupAnimationType.Scale:
                PlayScaleAnimation(endScale, startScale, () => {
                    isAnimating = false;
                    onComplete?.Invoke();
                });
                break;
                
            case PopupAnimationType.Fade:
                PlayFadeAnimation(1f, 0f, () => {
                    isAnimating = false;
                    onComplete?.Invoke();
                });
                break;
                
            case PopupAnimationType.Slide:
                PlaySlideAnimation(false, () => {
                    isAnimating = false;
                    onComplete?.Invoke();
                });
                break;
                
            case PopupAnimationType.Custom:
                PlayCustomCloseAnimation(() => {
                    isAnimating = false;
                    onComplete?.Invoke();
                });
                break;
                
            default:
                isAnimating = false;
                onComplete?.Invoke();
                break;
        }
    }
    
    /// <summary>
    /// 스케일 애니메이션
    /// </summary>
    protected void PlayScaleAnimation(Vector3 from, Vector3 to, System.Action onComplete)
    {
        StartCoroutine(ScaleAnimationCoroutine(from, to, onComplete));
    }
    
    /// <summary>
    /// 페이드 애니메이션
    /// </summary>
    protected void PlayFadeAnimation(float from, float to, System.Action onComplete)
    {
        StartCoroutine(FadeAnimationCoroutine(from, to, onComplete));
    }
    
    /// <summary>
    /// 슬라이드 애니메이션
    /// </summary>
    protected void PlaySlideAnimation(bool isOpening, System.Action onComplete)
    {
        StartCoroutine(SlideAnimationCoroutine(isOpening, onComplete));
    }
    
    #endregion
    
    #region 애니메이션 코루틴
    
    private IEnumerator ScaleAnimationCoroutine(Vector3 from, Vector3 to, System.Action onComplete)
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += pauseGameWhenOpen ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(t);
            
            transform.localScale = Vector3.Lerp(from, to, curveValue);
            
            yield return null;
        }
        
        transform.localScale = to;
        onComplete?.Invoke();
    }
    
    private IEnumerator FadeAnimationCoroutine(float from, float to, System.Action onComplete)
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += pauseGameWhenOpen ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(t);
            
            canvasGroup.alpha = Mathf.Lerp(from, to, curveValue);
            
            yield return null;
        }
        
        canvasGroup.alpha = to;
        onComplete?.Invoke();
    }
    
    private IEnumerator SlideAnimationCoroutine(bool isOpening, System.Action onComplete)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            onComplete?.Invoke();
            yield break;
        }
        
        Vector3 startPos = isOpening ? new Vector3(0, -Screen.height, 0) : Vector3.zero;
        Vector3 endPos = isOpening ? Vector3.zero : new Vector3(0, -Screen.height, 0);
        
        if (isOpening)
        {
            rectTransform.anchoredPosition = startPos;
        }
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += pauseGameWhenOpen ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(t);
            
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPos;
        onComplete?.Invoke();
    }
    
    #endregion
    
    #region 유틸리티
    
    /// <summary>
    /// 팝업 열림 사운드 재생
    /// </summary>
    protected virtual void PlayOpenSound()
    {
        if (AudioManager.instance != null && FMODEvents.instance != null)
        {
            AudioManager.instance.PlayUISound(FMODEvents.instance.ComputerPopupOpen, "팝업 열림");
        }
    }
    
    /// <summary>
    /// 팝업 닫힘 사운드 재생
    /// </summary>
    protected virtual void PlayCloseSound()
    {
        if (AudioManager.instance != null && FMODEvents.instance != null)
        {
            AudioManager.instance.PlayUISound(FMODEvents.instance.computerScreenClose, "팝업 닫힘");
        }
    }
    
    /// <summary>
    /// 사운드 재생 (레거시 - 하위 호환성 유지)
    /// </summary>
    protected void PlaySound(AudioClip clip)
    {
        // AudioClip 파라미터는 더 이상 사용하지 않음
        // 기존 코드 호환성을 위해 메서드는 유지
    }
    
    #endregion
    
    #region 가상 메서드 (상속받는 클래스에서 재정의)
    
    /// <summary>
    /// 팝업 초기화 시 호출 (Awake에서 호출)
    /// </summary>
    protected virtual void OnInitialize() { }
    
    /// <summary>
    /// UI 설정 시 호출 (Start에서 호출)
    /// </summary>
    protected virtual void OnSetupUI() { }
    
    /// <summary>
    /// 팝업이 열렸을 때 호출
    /// </summary>
    protected virtual void OnPopupOpened() { }
    
    /// <summary>
    /// 팝업이 닫히기 시작할 때 호출
    /// </summary>
    protected virtual void OnPopupClosing() { }
    
    /// <summary>
    /// 팝업이 완전히 닫혔을 때 호출
    /// </summary>
    protected virtual void OnPopupClosed() { }
    
    /// <summary>
    /// 팝업이 일시정지되었을 때 호출
    /// </summary>
    protected virtual void OnPopupPaused() { }
    
    /// <summary>
    /// 팝업이 재개되었을 때 호출
    /// </summary>
    protected virtual void OnPopupResumed() { }
    
    /// <summary>
    /// 커스텀 열림 애니메이션 (animationType이 Custom일 때)
    /// </summary>
    protected virtual void PlayCustomOpenAnimation(System.Action onComplete) 
    { 
        onComplete?.Invoke(); 
    }
    
    /// <summary>
    /// 커스텀 닫힘 애니메이션 (animationType이 Custom일 때)
    /// </summary>
    protected virtual void PlayCustomCloseAnimation(System.Action onComplete) 
    { 
        onComplete?.Invoke(); 
    }
    
    #endregion
}

/// <summary>
/// 팝업 애니메이션 타입
/// </summary>
public enum PopupAnimationType
{
    None,       // 애니메이션 없음
    Scale,      // 스케일 애니메이션
    Fade,       // 페이드 애니메이션
    Slide,      // 슬라이드 애니메이션
    Custom      // 커스텀 애니메이션
} 
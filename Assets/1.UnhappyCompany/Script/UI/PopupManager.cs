using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }
    
    [Header("팝업 시스템 설정")]
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private GameObject backgroundBlocker;
    [SerializeField] private Transform popupParent;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float defaultAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve defaultAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // 팝업 스택 관리
    private Stack<BasePopup> activePopups = new Stack<BasePopup>();
    private Queue<System.Action> popupQueue = new Queue<System.Action>();
    private bool isProcessingQueue = false;
    
    // 프로퍼티
    public bool HasActivePopups => activePopups.Count > 0;
    public int ActivePopupCount => activePopups.Count;
    public float DefaultAnimationDuration => defaultAnimationDuration;
    public AnimationCurve DefaultAnimationCurve => defaultAnimationCurve;
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePopupSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePopupSystem()
    {
        // Canvas 자동 설정
        if (popupCanvas == null)
        {
            popupCanvas = GetComponentInChildren<Canvas>();
            if (popupCanvas == null)
            {
                Debug.LogError("[PopupManager] Canvas를 찾을 수 없습니다!");
                return;
            }
        }
        
        // 팝업 부모 설정
        if (popupParent == null)
        {
            popupParent = popupCanvas.transform;
        }
        
        // 배경 블록 초기화
        if (backgroundBlocker != null)
        {
            backgroundBlocker.SetActive(false);
        }
        
        // ESC 키 입력 처리
        StartCoroutine(HandleEscapeInput());
        
        LogDebug("PopupManager 초기화 완료");
    }
    
    private void Update()
    {
        // 큐 처리
        if (!isProcessingQueue && popupQueue.Count > 0)
        {
            ProcessPopupQueue();
        }
    }
    
    #region 팝업 열기/닫기
    
    /// <summary>
    /// 팝업을 열고 스택에 추가
    /// </summary>
    public void OpenPopup(BasePopup popup)
    {
        if (popup == null)
        {
            Debug.LogError("[PopupManager] 팝업이 null입니다!");
            return;
        }
        
        // 이전 팝업 비활성화
        if (activePopups.Count > 0)
        {
            var previousPopup = activePopups.Peek();
            previousPopup.OnPause();
        }
        
        // 새 팝업 추가
        activePopups.Push(popup);
        popup.transform.SetParent(popupParent, false);
        popup.transform.SetAsLastSibling();
        
        // 배경 블록 표시
        UpdateBackgroundBlocker();
        
        LogDebug($"팝업 열림: {popup.GetType().Name} (스택 크기: {activePopups.Count})");
    }
    
    /// <summary>
    /// 현재 팝업을 닫고 스택에서 제거
    /// </summary>
    public void CloseCurrentPopup()
    {
        if (activePopups.Count == 0)
        {
            LogDebug("닫을 팝업이 없습니다.");
            return;
        }
        
        var currentPopup = activePopups.Pop();
        
        // 이전 팝업 다시 활성화
        if (activePopups.Count > 0)
        {
            var previousPopup = activePopups.Peek();
            previousPopup.OnResume();
        }
        
        // 배경 블록 업데이트
        UpdateBackgroundBlocker();
        
        LogDebug($"팝업 닫힘: {currentPopup.GetType().Name} (스택 크기: {activePopups.Count})");
    }
    
    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        LogDebug("모든 팝업 닫기 시작");
        
        while (activePopups.Count > 0)
        {
            var popup = activePopups.Pop();
            popup.ForceClose();
        }
        
        UpdateBackgroundBlocker();
        LogDebug("모든 팝업 닫기 완료");
    }
    
    /// <summary>
    /// 특정 타입의 팝업 닫기
    /// </summary>
    public bool ClosePopup<T>() where T : BasePopup
    {
        var popupsToReopen = new List<BasePopup>();
        bool found = false;
        
        // 해당 타입의 팝업을 찾을 때까지 스택에서 제거
        while (activePopups.Count > 0)
        {
            var currentPopup = activePopups.Pop();
            
            if (currentPopup is T)
            {
                currentPopup.ForceClose();
                found = true;
                break;
            }
            else
            {
                popupsToReopen.Add(currentPopup);
            }
        }
        
        // 제거했던 팝업들을 다시 스택에 추가 (역순)
        for (int i = popupsToReopen.Count - 1; i >= 0; i--)
        {
            activePopups.Push(popupsToReopen[i]);
        }
        
        // 현재 팝업 재개
        if (activePopups.Count > 0)
        {
            activePopups.Peek().OnResume();
        }
        
        UpdateBackgroundBlocker();
        return found;
    }
    
    #endregion
    
    #region 팝업 생성 및 로드
    
    /// <summary>
    /// 프리팹에서 팝업 생성 및 열기
    /// </summary>
    public T OpenPopupFromPrefab<T>(string prefabPath) where T : BasePopup
    {
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[PopupManager] 팝업 프리팹을 찾을 수 없습니다: {prefabPath}");
            return null;
        }
        
        var popupObject = Instantiate(prefab);
        var popup = popupObject.GetComponent<T>();
        
        if (popup == null)
        {
            Debug.LogError($"[PopupManager] {typeof(T).Name} 컴포넌트를 찾을 수 없습니다!");
            Destroy(popupObject);
            return null;
        }
        
        popup.OpenPopup();
        return popup;
    }
    
    /// <summary>
    /// 비동기로 팝업 열기 (큐 사용)
    /// </summary>
    public void OpenPopupAsync(System.Action openAction)
    {
        popupQueue.Enqueue(openAction);
    }
    
    #endregion
    
    #region 유틸리티
    
    /// <summary>
    /// 현재 팝업 가져오기
    /// </summary>
    public BasePopup GetCurrentPopup()
    {
        return activePopups.Count > 0 ? activePopups.Peek() : null;
    }
    
    /// <summary>
    /// 특정 타입의 팝업이 열려있는지 확인
    /// </summary>
    public bool IsPopupOpen<T>() where T : BasePopup
    {
        foreach (var popup in activePopups)
        {
            if (popup is T)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 배경 블록 업데이트
    /// </summary>
    private void UpdateBackgroundBlocker()
    {
        if (backgroundBlocker != null)
        {
            bool shouldShow = activePopups.Count > 0 && activePopups.Peek().BlockBackground;
            backgroundBlocker.SetActive(shouldShow);
            
            if (shouldShow)
            {
                backgroundBlocker.transform.SetSiblingIndex(activePopups.Peek().transform.GetSiblingIndex() - 1);
            }
        }
    }
    
    /// <summary>
    /// ESC 키 입력 처리
    /// </summary>
    private IEnumerator HandleEscapeInput()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && activePopups.Count > 0)
            {
                var currentPopup = activePopups.Peek();
                if (currentPopup.CanCloseWithEscape)
                {
                    currentPopup.ClosePopup();
                }
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// 팝업 큐 처리
    /// </summary>
    private void ProcessPopupQueue()
    {
        if (popupQueue.Count == 0) return;
        
        isProcessingQueue = true;
        var action = popupQueue.Dequeue();
        action?.Invoke();
        isProcessingQueue = false;
    }
    
    /// <summary>
    /// 디버그 로그
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PopupManager] {message}");
        }
    }
    
    #endregion
    
    #region 이벤트
    
    /// <summary>
    /// 팝업이 열렸을 때 호출되는 이벤트
    /// </summary>
    public System.Action<BasePopup> OnPopupOpened;
    
    /// <summary>
    /// 팝업이 닫혔을 때 호출되는 이벤트
    /// </summary>
    public System.Action<BasePopup> OnPopupClosed;
    
    /// <summary>
    /// 팝업 열림 이벤트 발생
    /// </summary>
    public void NotifyPopupOpened(BasePopup popup)
    {
        OnPopupOpened?.Invoke(popup);
    }
    
    /// <summary>
    /// 팝업 닫힘 이벤트 발생
    /// </summary>
    public void NotifyPopupClosed(BasePopup popup)
    {
        OnPopupClosed?.Invoke(popup);
    }
    
    #endregion
} 
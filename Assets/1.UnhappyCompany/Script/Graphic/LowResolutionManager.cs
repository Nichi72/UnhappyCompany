using UnityEngine;
using UnityEngine.UI;

public class LowResolutionManager : MonoBehaviour
{
    [Header("Target Resolution")]
    public int targetWidth = 860;
    public int targetHeight = 520;
    
    [Header("Rendering Settings")]
    public Camera mainCamera;
    public Canvas uiCanvas;
    
    [Header("RenderTexture Assignment")]
    [Tooltip("미리 생성된 RenderTexture를 할당하세요. 비어있으면 자동으로 생성됩니다.")]
    public RenderTexture lowResTexture;
    
    [Header("Multiple Resolution Support")]
    [Tooltip("여러 해상도의 RenderTexture를 미리 준비할 수 있습니다.")]
    public RenderTexturePreset[] renderTexturePresets;
    
    [Header("Options")]
    public bool applyOnStart = true;
    public FilterMode filterMode = FilterMode.Point; // Point for pixelated, Bilinear for smooth
    
    private RawImage displayImage;
    private GameObject canvasObject;
    private Camera originalCamera;
    private bool isUsingCustomTexture = false;
    private int currentPresetIndex = -1;
    
    [System.Serializable]
    public class RenderTexturePreset
    {
        public string name;
        public RenderTexture renderTexture;
        public int width;
        public int height;
        
        public bool IsValid => renderTexture != null && renderTexture.width > 0 && renderTexture.height > 0;
    }
    
    void Start()
    {
        if (applyOnStart)
        {
            SetupLowResolution();
        }
    }
    
    public void SetupLowResolution()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found! Please assign a camera.");
                return;
            }
        }
        
        // RenderTexture 준비
        PrepareRenderTexture();
        
        // UI 캔버스 설정
        SetupDisplayCanvas();
        
        // 카메라 설정
        SetupCamera();
        
        Debug.Log($"Low resolution mode activated: {targetWidth}x{targetHeight} (Custom Texture: {isUsingCustomTexture})");
    }
    
    void PrepareRenderTexture()
    {
        // 이미 할당된 RenderTexture가 있는지 확인
        if (lowResTexture != null && lowResTexture.IsCreated())
        {
            isUsingCustomTexture = true;
            targetWidth = lowResTexture.width;
            targetHeight = lowResTexture.height;
            
            // 필터 모드 적용
            lowResTexture.filterMode = filterMode;
            
            Debug.Log($"Using pre-assigned RenderTexture: {targetWidth}x{targetHeight}");
            return;
        }
        
        // 프리셋에서 적절한 RenderTexture 찾기
        var preset = FindBestPreset(targetWidth, targetHeight);
        if (preset != null && preset.IsValid)
        {
            lowResTexture = preset.renderTexture;
            isUsingCustomTexture = true;
            
            // 프리셋의 실제 해상도로 업데이트
            targetWidth = preset.width > 0 ? preset.width : preset.renderTexture.width;
            targetHeight = preset.height > 0 ? preset.height : preset.renderTexture.height;
            
            lowResTexture.filterMode = filterMode;
            
            Debug.Log($"Using preset RenderTexture '{preset.name}': {targetWidth}x{targetHeight}");
            return;
        }
        
        // 마지막 수단: 동적 생성 (하지만 경고 표시)
        Debug.LogWarning("No pre-assigned RenderTexture found. Creating one dynamically. Consider assigning a RenderTexture for better performance.");
        CreateRenderTextureDynamically();
    }
    
    void CreateRenderTextureDynamically()
    {
        // 기존 동적 생성된 텍스처가 있다면 해제
        if (lowResTexture != null && !isUsingCustomTexture)
        {
            if (Application.isPlaying)
                Destroy(lowResTexture);
            else
                DestroyImmediate(lowResTexture);
        }
        
        // 새로운 RenderTexture 생성
        lowResTexture = new RenderTexture(targetWidth, targetHeight, 24, RenderTextureFormat.ARGB32);
        lowResTexture.filterMode = filterMode;
        lowResTexture.name = $"LowRes_{targetWidth}x{targetHeight}";
        lowResTexture.Create();
        
        isUsingCustomTexture = false;
    }
    
    RenderTexturePreset FindBestPreset(int width, int height)
    {
        if (renderTexturePresets == null || renderTexturePresets.Length == 0)
            return null;
            
        // 정확히 일치하는 프리셋 찾기
        foreach (var preset in renderTexturePresets)
        {
            if (preset.IsValid)
            {
                int presetWidth = preset.width > 0 ? preset.width : preset.renderTexture.width;
                int presetHeight = preset.height > 0 ? preset.height : preset.renderTexture.height;
                
                if (presetWidth == width && presetHeight == height)
                {
                    return preset;
                }
            }
        }
        
        // 일치하는 것이 없으면 첫 번째 유효한 프리셋 반환
        foreach (var preset in renderTexturePresets)
        {
            if (preset.IsValid)
                return preset;
        }
        
        return null;
    }
    
    void SetupDisplayCanvas()
    {
        // 화면 전체를 덮는 캔버스 생성
        canvasObject = new GameObject("LowResolution Display Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // 최상위에 표시
        
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        
        // RawImage로 저해상도 텍스처 표시
        GameObject imageObject = new GameObject("Low Resolution Display");
        imageObject.transform.SetParent(canvasObject.transform, false);
        
        displayImage = imageObject.AddComponent<RawImage>();
        displayImage.texture = lowResTexture;
        
        // 전체 화면을 덮도록 설정
        RectTransform rectTransform = displayImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    void SetupCamera()
    {
        // 카메라가 저해상도 텍스처로 렌더링하도록 설정
        mainCamera.targetTexture = lowResTexture;
        
        // 기존 UI 캔버스가 있다면 카메라 모드 조정
        if (uiCanvas != null && uiCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            uiCanvas.worldCamera = mainCamera;
        }
    }
    
    public void DisableLowResolution()
    {
        if (mainCamera != null)
        {
            mainCamera.targetTexture = null;
        }
        
        if (canvasObject != null)
        {
            if (Application.isPlaying)
                Destroy(canvasObject);
            else
                DestroyImmediate(canvasObject);
        }
        
        // 동적으로 생성된 RenderTexture만 해제
        if (lowResTexture != null && !isUsingCustomTexture)
        {
            lowResTexture.Release();
            if (Application.isPlaying)
                Destroy(lowResTexture);
            else
                DestroyImmediate(lowResTexture);
            lowResTexture = null;
        }
        
        Debug.Log("Low resolution mode disabled");
    }
    
    public void ToggleLowResolution()
    {
        if (lowResTexture != null && (isUsingCustomTexture || lowResTexture.IsCreated()))
        {
            DisableLowResolution();
        }
        else
        {
            SetupLowResolution();
        }
    }
    
    public void ChangeResolution(int width, int height)
    {
        targetWidth = width;
        targetHeight = height;
        
        if (lowResTexture != null && (isUsingCustomTexture || lowResTexture.IsCreated()))
        {
            DisableLowResolution();
            SetupLowResolution();
        }
    }
    
    public void UsePreset(int presetIndex)
    {
        if (renderTexturePresets == null || presetIndex < 0 || presetIndex >= renderTexturePresets.Length)
        {
            Debug.LogError($"Invalid preset index: {presetIndex}");
            return;
        }
        
        var preset = renderTexturePresets[presetIndex];
        if (!preset.IsValid)
        {
            Debug.LogError($"Preset '{preset.name}' has invalid RenderTexture");
            return;
        }
        
        currentPresetIndex = presetIndex;
        
        // 현재 설정 해제
        if (lowResTexture != null && (isUsingCustomTexture || lowResTexture.IsCreated()))
        {
            DisableLowResolution();
        }
        
        // 프리셋의 RenderTexture 사용
        lowResTexture = preset.renderTexture;
        targetWidth = preset.width > 0 ? preset.width : preset.renderTexture.width;
        targetHeight = preset.height > 0 ? preset.height : preset.renderTexture.height;
        
        SetupLowResolution();
        
        Debug.Log($"Switched to preset: {preset.name} ({targetWidth}x{targetHeight})");
    }
    
    public void SetFilterMode(FilterMode mode)
    {
        filterMode = mode;
        if (lowResTexture != null)
        {
            lowResTexture.filterMode = mode;
        }
    }
    
    void OnDestroy()
    {
        DisableLowResolution();
    }
    
    void OnApplicationQuit()
    {
        DisableLowResolution();
    }
    
    // 디버그용 정보
    public string GetCurrentInfo()
    {
        if (lowResTexture == null)
            return "No RenderTexture assigned";
            
        return $"Resolution: {targetWidth}x{targetHeight}, Custom: {isUsingCustomTexture}, " +
               $"Filter: {filterMode}, Preset: {(currentPresetIndex >= 0 ? renderTexturePresets[currentPresetIndex].name : "None")}";
    }
}
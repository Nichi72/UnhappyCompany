using UnityEngine;

[System.Serializable]
public class ResolutionPreset
{
    public string name;
    public int width;
    public int height;
    
    public ResolutionPreset(string name, int width, int height)
    {
        this.name = name;
        this.width = width;
        this.height = height;
    }
}

public class LowResolutionController : MonoBehaviour
{
    [Header("Resolution Manager")]
    public LowResolutionManager resolutionManager;
    
    [Header("Presets")]
    public ResolutionPreset[] resolutionPresets = new ResolutionPreset[]
    {
        new ResolutionPreset("Very Low (160x90)", 640/4, 360/4),
        new ResolutionPreset("Low (860x520)", 860, 520),
        new ResolutionPreset("Medium Low (1024x576)", 1024, 576),
        new ResolutionPreset("Medium (1280x720)", 1280, 720)
    };
    
    [Header("RenderTexture Preset Control")]
    [Tooltip("Use RenderTexture presets from LowResolutionManager instead of resolution values")]
    public bool useRenderTexturePresets = true;
    
    [Header("Controls")]
    [SerializeField] private int currentPresetIndex = 1; // Default to 860x520
    [SerializeField] private bool isLowResActive = false;
    
    void Start()
    {
        if (resolutionManager == null)
        {
            resolutionManager = FindObjectOfType<LowResolutionManager>();
            if (resolutionManager == null)
            {
                Debug.LogError("LowResolutionManager not found! Please add one to the scene.");
                return;
            }
        }
        
        // 시작 시 기본 해상도 설정
        ApplyCurrentPreset();
    }
    
    void Update()
    {
        // 키보드 단축키로 제어
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleLowResolution();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            CycleThroughPresets();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ToggleFilterMode();
        }
        
        // 추가 단축키들
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TogglePresetMode();
        }
    }
    
    public void ToggleLowResolution()
    {
        if (resolutionManager == null) return;
        
        resolutionManager.ToggleLowResolution();
        isLowResActive = !isLowResActive;
        
        Debug.Log($"Low Resolution: {(isLowResActive ? "ON" : "OFF")}");
    }
    
    public void CycleThroughPresets()
    {
        if (useRenderTexturePresets && resolutionManager.renderTexturePresets != null)
        {
            // RenderTexture 프리셋 사용
            int presetCount = resolutionManager.renderTexturePresets.Length;
            if (presetCount > 0)
            {
                currentPresetIndex = (currentPresetIndex + 1) % presetCount;
                resolutionManager.UsePreset(currentPresetIndex);
                
                var preset = resolutionManager.renderTexturePresets[currentPresetIndex];
                Debug.Log($"RenderTexture preset changed to: {preset.name}");
            }
        }
        else
        {
            // 기존 해상도 프리셋 사용
            currentPresetIndex = (currentPresetIndex + 1) % resolutionPresets.Length;
            ApplyCurrentPreset();
            
            var preset = resolutionPresets[currentPresetIndex];
            Debug.Log($"Resolution changed to: {preset.name} ({preset.width}x{preset.height})");
        }
    }
    
    public void SetPreset(int index)
    {
        if (useRenderTexturePresets && resolutionManager.renderTexturePresets != null)
        {
            if (index >= 0 && index < resolutionManager.renderTexturePresets.Length)
            {
                currentPresetIndex = index;
                resolutionManager.UsePreset(index);
            }
        }
        else
        {
            if (index >= 0 && index < resolutionPresets.Length)
            {
                currentPresetIndex = index;
                ApplyCurrentPreset();
            }
        }
    }
    
    public void ApplyCurrentPreset()
    {
        if (resolutionManager == null) return;
        
        if (useRenderTexturePresets && resolutionManager.renderTexturePresets != null)
        {
            resolutionManager.UsePreset(currentPresetIndex);
        }
        else
        {
            var preset = resolutionPresets[currentPresetIndex];
            resolutionManager.ChangeResolution(preset.width, preset.height);
        }
    }
    
    public void ToggleFilterMode()
    {
        if (resolutionManager == null) return;
        
        FilterMode newMode = resolutionManager.filterMode == FilterMode.Point ? FilterMode.Bilinear : FilterMode.Point;
        resolutionManager.SetFilterMode(newMode);
        
        Debug.Log($"Filter Mode: {newMode} ({(newMode == FilterMode.Point ? "Pixelated" : "Smooth")})");
    }
    
    public void TogglePresetMode()
    {
        useRenderTexturePresets = !useRenderTexturePresets;
        Debug.Log($"Preset Mode: {(useRenderTexturePresets ? "RenderTexture Presets" : "Resolution Values")}");
        
        // 현재 프리셋 다시 적용
        ApplyCurrentPreset();
    }
    
    public void SetCustomResolution(int width, int height)
    {
        if (resolutionManager != null)
        {
            resolutionManager.ChangeResolution(width, height);
            Debug.Log($"Custom resolution set: {width}x{height}");
        }
    }
    
    // UI에서 호출할 수 있는 메서드들
    public void Enable860x520()
    {
        SetCustomResolution(860, 520);
    }
    
    public void Enable640x360()
    {
        SetCustomResolution(640, 360);
    }
    
    public void Enable160x90()
    {
        SetCustomResolution(160, 90);
    }
    
    public void Enable1024x576()
    {
        SetCustomResolution(1024, 576);
    }
    
    // RenderTexture 프리셋 관련 메서드들
    public void UseRenderTexturePreset(int index)
    {
        if (resolutionManager != null)
        {
            resolutionManager.UsePreset(index);
            currentPresetIndex = index;
        }
    }
    
    public void ShowAvailablePresets()
    {
        if (resolutionManager == null) return;
        
        Debug.Log("=== Available Presets ===");
        
        if (useRenderTexturePresets && resolutionManager.renderTexturePresets != null)
        {
            Debug.Log("RenderTexture Presets:");
            for (int i = 0; i < resolutionManager.renderTexturePresets.Length; i++)
            {
                var preset = resolutionManager.renderTexturePresets[i];
                string status = preset.IsValid ? "✓" : "✗";
                Debug.Log($"  {i}: {preset.name} {status}");
            }
        }
        else
        {
            Debug.Log("Resolution Presets:");
            for (int i = 0; i < resolutionPresets.Length; i++)
            {
                var preset = resolutionPresets[i];
                Debug.Log($"  {i}: {preset.name} ({preset.width}x{preset.height})");
            }
        }
    }
    
    void OnGUI()
    {
        // 간단한 디버그 UI
        if (resolutionManager == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 350, 250));
        GUILayout.Label("Low Resolution Controls (Debug)");
        
        // 현재 상태 표시
        if (useRenderTexturePresets && resolutionManager.renderTexturePresets != null && 
            currentPresetIndex >= 0 && currentPresetIndex < resolutionManager.renderTexturePresets.Length)
        {
            var preset = resolutionManager.renderTexturePresets[currentPresetIndex];
            GUILayout.Label($"Current RT Preset: {preset.name}");
        }
        else if (currentPresetIndex >= 0 && currentPresetIndex < resolutionPresets.Length)
        {
            var preset = resolutionPresets[currentPresetIndex];
            GUILayout.Label($"Current: {preset.name}");
        }
        
        GUILayout.Label($"Active: {isLowResActive}");
        GUILayout.Label($"Filter: {resolutionManager.filterMode}");
        GUILayout.Label($"Mode: {(useRenderTexturePresets ? "RenderTexture" : "Dynamic")}");
        
        // 상세 정보
        GUILayout.Label($"Info: {resolutionManager.GetCurrentInfo()}");
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("F1 - Toggle Low Resolution");
        GUILayout.Label("F2 - Cycle Presets");
        GUILayout.Label("F3 - Toggle Filter Mode");
        GUILayout.Label("F4 - Toggle Preset Mode");
        
        GUILayout.EndArea();
    }
} 
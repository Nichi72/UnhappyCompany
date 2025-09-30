using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "LowResolutionEffect", menuName = "Rendering/Custom Post Process/Low Resolution")]
public class LowResolutionEffect : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Header("Low Resolution Settings")]
    [Range(0.1f, 1.0f)]
    public FloatParameter resolutionScale = new FloatParameter(0.6f); // 860/1440 ≈ 0.6 for 1440p base
    public BoolParameter pixelatedLook = new BoolParameter(true);
    public BoolParameter enableEffect = new BoolParameter(true);
    
    public bool IsActive() => enableEffect.value && resolutionScale.value < 1f;
    
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
    
    public override void Setup()
    {
        // 셰이더 없이도 작동하도록 설정
    }
    
    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (!enableEffect.value || resolutionScale.value >= 1f)
        {
            // 효과가 비활성화되면 원본을 그대로 복사
            HDUtils.BlitCameraTexture(cmd, source, destination);
            return;
        }
        
        // 타겟 해상도 계산 (860x520 기준)
        int targetWidth = 860;
        int targetHeight = 520;
        
        // 또는 스케일 기반으로 계산
        int lowWidth = Mathf.Max(1, Mathf.RoundToInt(camera.actualWidth * resolutionScale.value));
        int lowHeight = Mathf.Max(1, Mathf.RoundToInt(camera.actualHeight * resolutionScale.value));
        
        // 사용자가 지정한 특정 해상도 사용 여부
        bool useFixedResolution = true; // 860x520 고정 사용
        
        if (useFixedResolution)
        {
            lowWidth = targetWidth;
            lowHeight = targetHeight;
        }
        
        // 임시 저해상도 텍스처 생성
        var lowResRT = RTHandles.Alloc(
            lowWidth, 
            lowHeight, 
            filterMode: pixelatedLook.value ? FilterMode.Point : FilterMode.Bilinear
        );
        
        try
        {
            // 1단계: 다운스케일 렌더링
            HDUtils.BlitCameraTexture(cmd, source, lowResRT);
            
            // 2단계: 다시 원래 크기로 업스케일
            HDUtils.BlitCameraTexture(cmd, lowResRT, destination);
        }
        finally
        {
            // 메모리 정리
            RTHandles.Release(lowResRT);
        }
    }
    
    public override void Cleanup()
    {
        // 정리 작업 (필요시)
    }
    
    // 런타임에서 해상도 변경을 위한 메서드들
    public void SetResolutionScale(float scale)
    {
        resolutionScale.value = Mathf.Clamp(scale, 0.1f, 1.0f);
    }
    
    public void SetPixelatedMode(bool pixelated)
    {
        pixelatedLook.value = pixelated;
    }
    
    public void SetFixedResolution(int width, int height)
    {
        // 이 기능을 사용하려면 위의 useFixedResolution 로직을 수정해야 함
        // 또는 추가 파라미터를 만들어서 관리
    }
}
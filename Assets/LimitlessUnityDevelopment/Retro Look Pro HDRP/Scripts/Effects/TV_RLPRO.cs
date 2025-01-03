using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;
using UnityEngine.UIElements;

[Serializable]
public sealed class WarpModeParameter : VolumeParameter<WarpMode> { };

[Serializable, VolumeComponentMenu(" Retro Look Pro/TV Effect")]
public sealed class TV_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
	[Range(0f, 1f), Tooltip("Effect fade.")]
	public ClampedFloatParameter fade = new ClampedFloatParameter(1,0,1);
	[Range(0f, 2f), Tooltip("Dark areas adjustment.")]
	public ClampedFloatParameter maskDark = new ClampedFloatParameter(0.5f, 0, 2f);
	[Range(0f, 2f), Tooltip("Light areas adjustment.")]
	public ClampedFloatParameter maskLight = new ClampedFloatParameter(1.5f,0,2f);
	[Range(-8f, -16f), Tooltip("Dark areas fine tune.")]
	public ClampedFloatParameter hardScan = new ClampedFloatParameter(-8f,-8f,16f);
	[Space]
    [Tooltip("Correct effect resolution, depending on screen resolution")]
    public BoolParameter ScaleWithActualScreenSize = new BoolParameter(false);
    [Range(1f, 16f), Tooltip("Effect resolution.")]
	public ClampedFloatParameter resScale = new ClampedFloatParameter(4f,1f,16f);
	[Space]
	[Range(-3f, 1f), Tooltip("pixels sharpness.")]
	public ClampedFloatParameter hardPix = new ClampedFloatParameter(-3f,-3f,1f);
	[Tooltip("Warp mode.")]
	public WarpModeParameter warpMode = new WarpModeParameter { };
	[Tooltip("Warp picture.")]
	public Vector2Parameter warp = new Vector2Parameter (new Vector2(0f, 0f) );
	public FloatParameter scale = new FloatParameter (0.5f);
	[Space]
	[Tooltip("Mask texture")]
	public TextureParameter mask = new TextureParameter(null);
	public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;
    float scaler;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/TV_RLPRO_HDRP") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/TV_RLPRO_HDRP"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetFloat("_Intensity", intensity.value);
		m_Material.SetFloat("fade",  fade.value);
		m_Material.SetFloat("scale",  scale.value);
		m_Material.SetFloat("hardScan",  hardScan.value);
		m_Material.SetFloat("hardPix",  hardPix.value);
        if (ScaleWithActualScreenSize.value)
            scaler = resScale.value * (Screen.height * (Screen.width / Screen.height) / 1000f);
		else
            scaler = resScale.value;

        if (mask.value != null)
        {
            m_Material.SetTexture(_Mask, mask.value);
            m_Material.SetFloat(_FadeMultiplier, 1);
            ParamSwitch(m_Material, maskChannel.value == maskChannelMode.alphaChannel ? true : false, "ALPHA_CHANNEL");
        }
        else
        {
            m_Material.SetFloat(_FadeMultiplier, 0);
        }

        m_Material.SetFloat("resScale", scaler);
		m_Material.SetFloat("maskDark",  maskDark.value);
		m_Material.SetFloat("maskLight",  maskLight.value);
		m_Material.SetVector("warp",  warp.value);
        cmd.Blit(source, destination, m_Material, warpMode == WarpMode.SimpleWarp ? 0 : 1);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
    private void ParamSwitch(Material mat, bool paramValue, string paramName)
    {
        if (paramValue) mat.EnableKeyword(paramName);
        else mat.DisableKeyword(paramName);
    }

}

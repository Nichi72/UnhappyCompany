using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/LowRes")]
public sealed class LowRes_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
	[Tooltip("Controls the intensity of the effect.")]
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
	[Tooltip("Lower value = more downscale.")]
	public ClampedFloatParameter downscale = new ClampedFloatParameter(0f, 0.1f, 1f);
	[Space]
	[Tooltip("Mask texture")]
	public TextureParameter mask = new TextureParameter(null);
	public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
	static readonly int _Mask = Shader.PropertyToID("_Mask");
	static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

	public bool IsActive() => m_Material != null && intensity.value > 0f;
	//
	Material m_Material;
	RTHandle lowresTexture;
	RTHandle highresTexture;
	float m_PrevValue;
	public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

	public override void Setup()
	{
		if (Shader.Find("Hidden/Shader/LowResolution_RLPRO") != null)
			m_Material = new Material(Shader.Find("Hidden/Shader/LowResolution_RLPRO"));
		lowresTexture = RTHandles.Alloc(Vector2.one * downscale.value, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "lowresTexture");
		highresTexture = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "highresTexture");
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;
		if (m_PrevValue != downscale.value)
		{
			RTHandles.Release(highresTexture);
			RTHandles.Release(lowresTexture);
			lowresTexture = RTHandles.Alloc(Vector2.one * downscale.value, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "lowresTexture");
			highresTexture = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "highresTexture");
			m_PrevValue = downscale.value;
		}
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


		m_Material.SetFloat("_Intensity", intensity.value);
		cmd.Blit(source, lowresTexture, m_Material, 0);
		m_Material.SetTexture("_InputTexture2", lowresTexture);
		m_Material.SetFloat("downsample", downscale.value);
		cmd.Blit(lowresTexture, highresTexture, m_Material, 1);
		m_Material.SetTexture("_InputTexture3", highresTexture);
		m_Material.SetFloat("downsample", downscale.value);
		cmd.Blit(source, destination, m_Material, 2);
	}
	private void ParamSwitch(Material mat, bool paramValue, string paramName)
	{
		if (paramValue) mat.EnableKeyword(paramName);
		else mat.DisableKeyword(paramName);
	}

	public override void Cleanup()
	{
		CoreUtils.Destroy(m_Material);
		RTHandles.Release(highresTexture);
		RTHandles.Release(lowresTexture);
	}
}

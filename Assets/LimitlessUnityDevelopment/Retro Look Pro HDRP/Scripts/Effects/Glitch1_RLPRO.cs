﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Glitch1")]
public sealed class Glitch1_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
	[Tooltip("Effect amount.")]
	public ClampedFloatParameter amount = new ClampedFloatParameter(0.6f, 0f, 20f);
	[Space]
	[Tooltip("Effect stretch.")]
	public ClampedFloatParameter stretch = new ClampedFloatParameter(0.06f, 0f, 4f);
	[Tooltip("Effect speed.")]
	public ClampedFloatParameter speed = new ClampedFloatParameter(0.5f, 0f, 1f);
	[Tooltip("Effect Fade.")]
	public ClampedFloatParameter fade = new ClampedFloatParameter(0.5f, 0f, 1f);
	[Space]
	[Tooltip("Red color offset  muliplier.")]
	public ClampedFloatParameter rMultiplier = new ClampedFloatParameter(1f, -1f, 2f);
	[Tooltip("Green color offset  muliplier.")]
	public ClampedFloatParameter gMultiplier = new ClampedFloatParameter(1f, -1f, 2f);
	[Tooltip("Blue color offset  muliplier.")]
	public ClampedFloatParameter bMultiplier = new ClampedFloatParameter(1f, -1f, 2f);
	[Space]
	[Tooltip("X parameter of random value on noise texture.")]
	public ClampedFloatParameter x = new ClampedFloatParameter(127.1f, -2, 127.1f);
	[Tooltip("Y parameter of random value on noise texture.")]
	public ClampedFloatParameter y = new ClampedFloatParameter(43758.5453123f, -2, 43758.5453123f);
	[Tooltip("Angle Y parameter of random value on noise texture.")]
	public ClampedFloatParameter angleY = new ClampedFloatParameter(311.7f, -2f, 311.7f);
	[Space]
	[Tooltip("Mask texture")]
	public TextureParameter mask = new TextureParameter(null);
	public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
	static readonly int _Mask = Shader.PropertyToID("_Mask");
	static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

	//
	private float T;
	Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/Glitch1Effect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/Glitch1Effect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
		T += Time.deltaTime;
		if (T > 100) T = 0;

		m_Material.SetFloat("Strength", amount.value);

		m_Material.SetFloat("x", x.value);
		m_Material.SetFloat("y", y.value);
		m_Material.SetFloat("angleY", angleY.value);
		m_Material.SetFloat("Stretch", stretch.value);
		m_Material.SetFloat("Speed", speed.value);
		m_Material.SetFloat("mR", rMultiplier.value);
		m_Material.SetFloat("mG", gMultiplier.value);
		m_Material.SetFloat("mB", bMultiplier.value);
		m_Material.SetFloat("Fade", fade.value);
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

		m_Material.SetFloat("T", T);
		m_Material.SetFloat("_Intensity", intensity.value);

        cmd.Blit(source, destination, m_Material, 0);
    }
    private void ParamSwitch(Material mat, bool paramValue, string paramName)
	{
		if (paramValue) mat.EnableKeyword(paramName);
		else mat.DisableKeyword(paramName);
	}

	public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}

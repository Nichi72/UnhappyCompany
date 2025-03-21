Shader "Hidden/Shader/ColormapPaletteEffect_RLPRO"
{
	 Properties
    {
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }
	
    TEXTURE2D_X(_MainTex);
	TEXTURE2D_X(_InputTexture2);
	TEXTURE2D_X(_InputTexture3);
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;
	#pragma shader_feature ALPHA_CHANNEL

    float _Intensity;
	float4 downsample;
	sampler3D _Colormap;
	float4 _Colormap_TexelSize;
	sampler2D _Palette;
	sampler2D _BlueNoise;
	float4 _BlueNoise_TexelSize;
	float _Opacity;
	float _Dither;
	float2 Resolution;
	half CalcLuminance(float3 color)
	{
		return dot(color, float3(0.299f, 0.587f, 0.114f));
	}

	float4 Frag0(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 UV = i.texcoord;
        float2 positionSS = UV;

		float4 inputColor = SAMPLE_TEXTURE2D_X(_MainTex,s_point_clamp_sampler, positionSS );
		inputColor = saturate(inputColor);
		float4 colorInColormap = tex3D(_Colormap, inputColor.rgb);
		float random = tex2D(_BlueNoise, i.positionCS.xy/ _BlueNoise_TexelSize.z).r;
		random = saturate(random);
		if (CalcLuminance(colorInColormap.r) > CalcLuminance(colorInColormap.g))
		{
			random = 1 - random;
		}
		float paletteIndex;
		float blend = colorInColormap.b;
		float threshold = saturate((1 / _Dither) * (blend - 0.5 + (_Dither / 2)));
		if (random < threshold)
		{
			paletteIndex = colorInColormap.g;
		}
		else
		{
			paletteIndex = colorInColormap.r;
		}
		float4 result = tex2D(_Palette, float2(paletteIndex, 0));
		result.a = inputColor.a;
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).r);
#endif
			_Opacity *= alpha_Mask;
		}

		result = lerp(inputColor, result, _Opacity);
		return result;
	}

	float4 Frag2(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 UV = i.texcoord;
        float2 positionSS = UV ;
		float4 col = LOAD_TEXTURE2D_X(_MainTex,positionSS * 0.5);
	
		float4 inputColor = LOAD_TEXTURE2D_X(_MainTex, positionSS);

		return inputColor;
	}
	
	float4 Frag3(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float4 col = LOAD_TEXTURE2D_X(_InputTexture3,i.texcoord * Resolution);
		return col;
	}
    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#NAME#"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment Frag0
                #pragma vertex Vert
            ENDHLSL
        }
			Pass
		{
			Name "#NAME#"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment Frag2
				#pragma vertex Vert
			ENDHLSL
		}

			Pass
		{
			Name "#NAME#"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment Frag3
				#pragma vertex Vert
			ENDHLSL
		}

    }
    Fallback Off
}
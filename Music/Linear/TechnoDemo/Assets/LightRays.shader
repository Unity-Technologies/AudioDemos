Shader "Hidden/LightRays"
{
Properties
{
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_AmpScale ("Amp Scale", float) = 0.9
	_DirScale ("Dir Scale", float) = 0.5
}

SubShader
{
	Pass
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 

#include "UnityCG.cginc"

uniform sampler2D _MainTex;

uniform float4 _MainTex_ST;

uniform float4 _MainTex_TexelSize;
uniform float4 _LightRayParams;

uniform float _AmpScale = 0.9;
uniform float _DirScale = 0.5;

struct v2f {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

v2f vert (appdata_img v)
{
	v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex); //MultiplyUV (UNITY_MATRIX_TEXTURE0, uv);
	return o;
}

float4 frag (v2f i) : COLOR
{
	int steps = 20;
	float4 sum = float4(0.0, 0.0, 0.0, 0.0);
	float2 p = _LightRayParams.xy;
	float2 d = _DirScale * (i.uv - p) * _LightRayParams.w / steps;
	float amp = 1.0;
	for(int n = 0; n < steps; n++)
	{
		sum += tex2D(_MainTex, p) * amp;
		p += d;
		amp *= _AmpScale;
	}
	float4 s = tex2D(_MainTex, i.uv);
	return s + (sum * 0.05 - s) * _LightRayParams.z;
}
ENDCG

	}
}

Fallback off

}
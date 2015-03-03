Shader "Hidden/TeeVeeNoise"
{
Properties
{
	_MainTex ("Base (RGB)", 2D) = "white" {}
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
uniform float4 _Distortion;

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

float DistortFunc(float x)
{
	return sin(1.0 / (0.01 + x * x * 4.0 * _Distortion.w));	
}

float4 frag (v2f i) : COLOR
{
	float y = (1.0 - i.uv.y) * _Distortion.y;
	float4 p1 = tex2D(_MainTex, float2(i.uv.x - _Distortion.x * DistortFunc(y * 33.0), i.uv.y)) * float4(0.8, 0.2, 0.2, 1.0);
	float4 p2 = tex2D(_MainTex, float2(i.uv.x - _Distortion.x * DistortFunc(y * 53.0), i.uv.y)) * float4(0.2, 0.8, 0.2, 1.0);
	float4 p3 = tex2D(_MainTex, float2(i.uv.x - _Distortion.x * DistortFunc(y * 59.0), i.uv.y)) * float4(0.2, 0.2, 0.8, 1.0);
	return pow(p1 + p2 + p3, _Distortion.z);
}
ENDCG

	}
}

Fallback off

}
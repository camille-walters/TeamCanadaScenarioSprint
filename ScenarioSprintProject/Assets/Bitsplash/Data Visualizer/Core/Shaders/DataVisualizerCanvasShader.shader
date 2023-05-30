// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// this shader created a basic gradient between an initial color to an


 
Shader "DataVisualizer/Canvas/Solid"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_Extrusion("Extrusion", Float) = 0
		_LocalScale("LocalScale", Vector) = (1,1,1,1)
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	} 

		SubShader{

		Tags{ "Queue" = "Transparent"
		"RenderType" = "Transparent" }

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		ColorMask[_ColorMask]

		Pass{
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			LOD 100

		CGPROGRAM
#pragma vertex lerpVertex  
#pragma fragment sampleFragment
#include "UnityCG.cginc"
#include "UnityUI.cginc"
	fixed4 _Color;
	fixed _Extrusion;
	fixed4 _LocalScale;
	fixed4 _TextureSampleAdd;
	uniform float4x4 _DirTransform;
	sampler2D _MainTex;
	uniform float4 _MainTex_ST;
	uniform float4 _ClipRect;

	struct inputVertex
	{
		float4 vertex : POSITION;
		float4 texcoord : TEXCOORD0;
		float4 tangent : TANGENT;
		fixed4 color : COLOR;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	struct vertexData
	{
		float2 uv : TEXCOORD0;
		float2 worldPosition : TEXCOORD1;
		float4 pos : SV_POSITION;
		fixed4 color : COLOR; 
	};
	
	vertexData lerpVertex(inputVertex v)
	{
		vertexData res;
		float factor = v.tangent.w;
		float s = sign (factor);
		float size = v.tangent.z;
		float4 tangent = float4(v.tangent.x / _LocalScale.x, v.tangent.y / _LocalScale.y, 0, 0);	
		float4 dir = float4(s * tangent.y ,s * -tangent.x ,0,0);	// make the tangent perpendicular so we get the normal
		float4 toNorm = lerp(tangent,dir, abs(factor));
		toNorm.w = !any(toNorm); // avoid zero vector by setting the w vector to 1.0 if the vector is all zeros. otherwise it will be set to 0.0. This takes leverage of arithemtic functions so no branching is introduced
		dir = mul(_DirTransform,normalize(toNorm));
		res.pos = v.vertex + (dir * _Extrusion * size);
		res.worldPosition = v.vertex;
		res.pos = UnityObjectToClipPos(res.pos);
		res.uv = TRANSFORM_TEX(v.texcoord, _MainTex);		
		res.color = _Color * v.color;
		return res;
	}

	float4 sampleFragment(vertexData v) : COLOR
	{
		fixed4 texData = v.color * (_TextureSampleAdd + tex2D(_MainTex, v.uv));
		texData.a *= UnityGet2DClipping(v.worldPosition.xy, _ClipRect);
		return texData;
	}
		ENDCG
	}
	}
}
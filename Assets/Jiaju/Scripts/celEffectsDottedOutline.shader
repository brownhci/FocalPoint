Shader "Custom/CelEffectsDottedOutline"
{
	Properties
	{
	    _MainTex("Base (RGB)", 2D) = "white" {}
		_UseBodyColor("Use Body Color", Float) = 0
		_BodyColor("Body Color", Color) = (1,1,1,1)
		_OutlineFactor("Outline Factor", Range(0, 1)) = 0
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
		_OutlineWidth("Outline Width", Range(0, 10)) = 0.005
		_BodyAlpha("Body Alpha", Range(0, 1)) = 1

        _OutlineDot("Outline Dot", float) = 0.25
		_OutlineDot2("Outline Dot Distance", float) = 0.5
		_OutlineSpeed("Outline Dot Speed", float) = 50.0
		_SourcePos("Source Position", vector) = (0, 0, 0, 0)

		_Stencil("Stencil ID", Int) = 8

		[HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
	}

	SubShader {
		Tags { 
			"RenderType" = "Transparent"
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True"
		}
		LOD 200

		Pass {
			Name "VerticsOutline_Outline_Stencil"

			Cull Off
			ZWrite Off
			ZTest Always
			ColorMask 0

			Stencil {
				Ref [_Stencil]
				Comp Always
				Pass Replace
				ZFail Replace
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
			}

			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 2.0
	#pragma multi_compile_fog

	#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform float _UseBodyColor;
			uniform fixed4 _BodyColor;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o, o.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;

				if(_UseBodyColor == 0) {
					col = tex2D(_MainTex, i.texcoord);
				}
				else {
					col = _BodyColor;
				}

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);

				return col;
			}

			ENDCG
		}

		Pass {
			Name "VerticsOutline_Outline_Face1"

			Cull Off
			ZWrite On
			ZTest Always
			ColorMask RGBA
			
			Stencil {
				Ref [_Stencil]
				Comp NotEqual
				Pass Keep
				ZFail Keep
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform fixed4 _OutlineColor;
			uniform float _OutlineWidth;
			uniform float _OutlineFactor;

            float  _OutlineDot;
			float  _OutlineDot2;
			float  _OutlineSpeed;
			float4 _SourcePos;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 caculateVec = lerp(normalize(v.vertex.xyz), normalize(v.normal), _OutlineFactor);

				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, caculateVec);
				float2 offset = TransformViewToProjection(norm.xy);
				o.vertex.xy += offset * o.vertex.z * _OutlineWidth;

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

                float2 pos = i.vertex.xy;
                float skip = sin(_OutlineDot*abs(distance(_SourcePos.xy, pos))) + _OutlineDot2;
				clip(skip); // stops rendering a pixel if 'skip' is negative

				fixed4 col = _OutlineColor;

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}

			ENDCG
		}

		Pass {
			Name "VerticsOutline_Outline_Face2"

			Cull Off
			ZWrite On
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil {
				Ref [_Stencil]
				Comp NotEqual
				Pass Keep
				ZFail Keep
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        #pragma target 2.0
	        #pragma multi_compile_fog

	        #include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform fixed4 _OutlineColor;
			uniform float _OutlineWidth;
			uniform float _OutlineFactor;

			float  _OutlineDot;
			float  _OutlineDot2;
			float  _OutlineSpeed;
			float4 _SourcePos;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 caculateVec = -lerp(normalize(v.vertex.xyz), normalize(v.normal), _OutlineFactor);

				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, caculateVec);
				float2 offset = TransformViewToProjection(norm.xy);
				o.vertex.xy += offset * o.vertex.z * _OutlineWidth;

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

                float2 pos = i.vertex.xy;
                float skip = sin(_OutlineDot*abs(distance(_SourcePos.xy, pos))) + _OutlineDot2;
				clip(skip); // stops rendering a pixel if 'skip' is negative

				fixed4 col = _OutlineColor;

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}

			ENDCG
		}

		Pass {
			Name "VerticsOutline_Body"

			Cull Back
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGBA

			Stencil {
				Ref[_Stencil]
				Comp Always
				Pass Replace
				ZFail Replace
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 2.0
	#pragma multi_compile_fog

	#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
					UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float _BodyAlpha;

			uniform float _UseBodyColor;
			uniform fixed4 _BodyColor;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;

				if(_UseBodyColor == 0) {
					col = tex2D(_MainTex, i.texcoord);
				}
				else {
					col = _BodyColor;
				}
				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);

				return fixed4(col.rgb, _BodyAlpha);
			}

			ENDCG
		}
	}
}

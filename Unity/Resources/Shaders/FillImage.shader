Shader "OpenGET/FillImage"
{
    Properties
	{
		_MainTex("Base Texture", 2D) = "white" {}
		_BaseColor("Base Color", Color) = (1, 1, 1, 1)
		_FillTex("Fill Texture", 2D) = "white" {}
		_FillColor("FillColor", Color) = (1, 1, 1, 1)
		_FillAmount("Fill Amount", Range(0.0, 1.0)) = 0.75

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
		_RectUV("UV Rect", Vector) = (0, 0, 1, 1)

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
		Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        // No culling or depth
        Cull Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]
		
		Tags { "Queue"="Transparent" }
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile VERTICAL_FILL_ON VERTICAL_FILL_OFF
			#pragma multi_compile FLIP_FILL_OFF FLIP_FILL_ON
			#pragma multi_compile GRAYSCALE_OFF GRAYSCALE_ON
			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
			
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 worldPosition : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
				o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
				o.uv = v.uv;
                return o;
            }

			// Base (unfilled) texture
            sampler2D _MainTex;

			// Color tint for the base texture
			fixed4 _BaseColor;

			// Fill (filled) texture
			sampler2D _FillTex;

			// Color tint for the fill texture
			fixed4 _FillColor;

			// Percentage filled
			float _FillAmount;

			// UI clipping rect
			float4 _ClipRect;

			// UV rect with min and max UV coordinates
			float4 _RectUV;

            // Map UVs from rect to texture UVs.
			float2 MapRectUV(float2 uv)
            {
                float4 res;
				res.x = (uv.x - _RectUV.x) / (_RectUV.z - _RectUV.x);
				res.y = (uv.y - _RectUV.y) / (_RectUV.w - _RectUV.y);
				return res;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;

				float2 texUV = MapRectUV(i.uv);
#ifdef VERTICAL_FILL_ON
	#ifdef FLIP_FILL_ON
				if (_FillAmount >= 1 - texUV.y) {
	#else
				if (_FillAmount >= texUV.y) {
	#endif
#else
	#ifdef FLIP_FILL_ON
				if (_FillAmount >= 1 - texUV.x) {
	#else
				if (_FillAmount >= texUV.x) {
	#endif
#endif
					col = tex2D(_FillTex, i.uv) * _FillColor;
				}
				else {
					col = tex2D(_MainTex, i.uv) * _BaseColor;
				}

				
               #ifdef UNITY_UI_CLIP_RECT
                   col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
               #endif

               #ifdef UNITY_UI_ALPHACLIP
                   clip(col.a - 0.001);
               #endif

                return col;
            }
            ENDCG
        }
    }
}

Shader "OpenGET/FillImage"
{
    Properties
	{
		_MainTex("Base Texture", 2D) = "white" {}
		_BaseColor("Base Color", Color) = (1, 1, 1, 1)
		_FillTex("Fill Texture", 2D) = "white" {}
		_FillColor("FillColor", Color) = (1, 1, 1, 1)
		_FillAmount("Fill Amount", Range(0.0, 1.0)) = 0.75
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		
		Tags { "Queue"="Transparent" }
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile VERTICAL_FILL_ON VERTICAL_FILL_OFF
			#pragma multi_compile FLIP_FILL_OFF FLIP_FILL_ON
			#pragma multi_compile GRAYSCALE_OFF GRAYSCALE_ON

            #include "UnityCG.cginc"
			
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
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

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;
#ifdef VERTICAL_FILL_ON
	#ifdef FLIP_FILL_ON
				if (_FillAmount >= 1 - i.uv.y) {
	#else
				if (_FillAmount >= i.uv.y) {
	#endif
#else
	#ifdef FLIP_FILL_ON
				if (_FillAmount >= 1 - i.uv.x) {
	#else
				if (_FillAmount >= i.uv.x) {
	#endif
#endif
					col = tex2D(_FillTex, i.uv) * _FillColor;
				}
				else {
					col = tex2D(_MainTex, i.uv) * _BaseColor;
				}
                return col;
            }
            ENDCG
        }
    }
}

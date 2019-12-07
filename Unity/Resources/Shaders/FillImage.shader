Shader "TSDoors/FillImage"
{
    Properties
	{
		_BaseTex("Base Texture", 2D) = "white" {}
		_FillTex("Fill Texture", 2D) = "white" {}
		_FillAmount("Fill Amount", Range(0.0, 1.0)) = 0.75
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile VERTICAL_FILL_ON VERTICAL_FILL_OFF
			#pragma multi_compile INVERT_FILL_OFF INVERT_FILL_ON
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

            sampler2D _BaseTex;
			sampler2D _FillTex;
			float _FillAmount;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;
#ifdef VERTICAL_FILL_ON
	#ifdef INVERT_FILL_ON
				if (_FillAmount >= 1 - i.uv.y) {
	#else
				if (_FillAmount >= i.uv.y) {
	#endif
					col = tex2D(_FillTex, i.uv);
				}
				else {
					col = tex2D(_BaseTex, i.uv);
				}
#else
	#ifdef INVERT_FILL_ON
				if (_FillAmount >= 1 - i.uv.x) {
	#else
				if (_FillAmount >= i.uv.x) {
	#endif
					col = tex2D(_FillTex, i.uv);
				}
				else {
					col = tex2D(_BaseTex, i.uv);
				}
#endif // VERTICAL_FILL_ON
                return col;
            }
            ENDCG
        }
    }
}

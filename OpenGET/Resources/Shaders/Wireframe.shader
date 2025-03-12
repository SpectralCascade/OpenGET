Shader "OpenGET/Unlit/Wireframe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WireframeColor("Colour of the polys", color) = (1.0, 1.0, 1.0, 1.0)
        _WireframeThickness("Percentage of the poly to draw as wireframe", float) = 0.05
        _FillColor("Optional flat fill colour", color) = (0.0, 0.0, 0.0, 0.0)
        _Simplify("Simplify the wireframe", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 barycentric: TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // Vertex program
            v2f vert (appdata v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            int _Simplify;

            // Geometry program
            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triangles) {
                float3 lengths = float3(
                    length(IN[1].vertex - IN[2].vertex),
                    length(IN[0].vertex - IN[2].vertex),
                    length(IN[0].vertex - IN[1].vertex)
                );
                float3 mod = float3(0.0, 0.0, 0.0);

                if (_Simplify == 0)
                {
                }
                else if ((lengths[0] > lengths[1]) && (lengths[0] > lengths[2]))
                {
                    mod[0] = 1.0;
                }
                else if ((lengths[1] > lengths[0]) && (lengths[1] > lengths[2]))
                {
                    mod[1] = 1.0;
                }
                else if ((lengths[2] > lengths[0]) && (lengths[2] > lengths[1]))
                {
                    mod[2] = 1.0;
                }

                g2f o;
                o.barycentric = float3(1.0, 0.0, 0.0) + mod;
                o.pos = UnityObjectToClipPos(IN[0].vertex);

                triangles.Append(o);
                o.barycentric = float3(0.0, 1.0, 0.0) + mod;
                o.pos = UnityObjectToClipPos(IN[1].vertex);

                triangles.Append(o);
                o.barycentric = float3(0.0, 0.0, 1.0) + mod;
                o.pos = UnityObjectToClipPos(IN[2].vertex);

                triangles.Append(o);
            }

            fixed4 _WireframeColor;
            fixed4 _FillColor;
            float _WireframeThickness;

            // Fragment program
            fixed4 frag (g2f i) : SV_Target
            {
                // Determine whether on an edge or not, and how much to blend the fill colour
                float3 edge = smoothstep(float3(0.0, 0.0, 0.0), fwidth(i.barycentric) * _WireframeThickness, i.barycentric);

                // When values in the edge vector == 1, indicates a solid edge
                // When values in the edge vector == 0, indicates no edge
                // When values inbetween, indicates blending amount between fill and edge colours
                float edgeyness = 1 - min(edge.x, min(edge.y, edge.z));
                float inverse = 1 - edgeyness;

                return fixed4(
                    (edgeyness * _WireframeColor.r) + (inverse * _FillColor.r),
                    (edgeyness * _WireframeColor.g) + (inverse * _FillColor.g),
                    (edgeyness * _WireframeColor.b) + (inverse * _FillColor.b),
                    (edgeyness * _WireframeColor.a) + (inverse * _FillColor.a)
                );
            }

            ENDCG
        }
    }
}

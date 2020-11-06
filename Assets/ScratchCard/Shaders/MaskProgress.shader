// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ScratchCard/MaskProgress" {
    Properties {
        _MainTex ("Main", 2D) = "white" {}
    }

    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            struct app2vert
            {
                float4 position: POSITION;
                half4 color: COLOR;
                float2 texcoord: TEXCOORD0;
            };

            struct vert2frag
            {
                float4 position: POSITION;
                half4 color: COLOR;
                float2 texcoord: TEXCOORD0;
            };

            vert2frag vert(app2vert input)
            {
                vert2frag output;
                output.position = UnityObjectToClipPos(input.position);
				output.color = input.color;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                return output;
            }

            fixed4 frag(vert2frag input) : COLOR
            {
            	fixed4 col = input.color;
                fixed count = 0.0f;
                fixed texWidth = 16;
                fixed texHeight = 16;
                for	(int i = 0; i < texWidth; i++)
                {
                	for	(int j = 0; j < texHeight; j++)
                	{
                		fixed2 newTexcoord = fixed2(i / (texWidth - 1.0f), j / (texHeight - 1.0f));
                		count += tex2D(_MainTex, newTexcoord).a;
                	}
                }

                count /= texWidth * texHeight;
                return fixed4(count, count, count, 1.0f) * col;
            }
            ENDCG
        }
    }
}
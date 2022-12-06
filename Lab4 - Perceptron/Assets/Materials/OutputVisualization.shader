Shader "Unlit/OutputVisualization"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ColorNegative ("Negative Color", Color) = (0,0,0,1)
		_ColorPositive ("Positive Color", Color) = (1,1,1,1)
		_ColorZero ("Zero Color", Color) = (0.5,0.5,0.5,1)
		_ZeroWidth ("Zero Width", Float) = 0.05
		_Smoothing ("Smoothing", Float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			
			float4 _ColorNegative;
			float4 _ColorPositive;
			float4 _ColorZero;
			float _ZeroWidth;
			float _Smoothing;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float ssSize = _Smoothing;
				
                float x = tex2D(_MainTex, i.uv).r;
				float z = _ZeroWidth - abs(x);
				
				float4 c = lerp(_ColorNegative, _ColorPositive, step(0, x));
				float4 col = lerp(c, _ColorZero, smoothstep(-ssSize, ssSize, z));
                
                return col;
            }
            ENDCG
        }
    }
}

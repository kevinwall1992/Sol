Shader "Unlit/ImageSpaceColoredTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

			struct VertexData
			{
				float4 position : POSITION;
			};

			struct FragmentData
			{
				float4 screen_position : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			fixed4 _Color;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;

				fragment_data.screen_position =
					UnityObjectToClipPos(vertex_data.position);

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				return tex2D(_MainTex, fragment_data.screen_position.xy * _MainTex_TexelSize.xy) *
					   _Color;
			}
            ENDCG
        }
    }
}

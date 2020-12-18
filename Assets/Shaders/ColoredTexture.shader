Shader "Unlit/ColoredTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct VertexData
			{
				float4 position : POSITION;
				float2 texture_coordinates : TEXCOORD0;
			};

			struct FragmentData
			{
				float4 screen_position : SV_POSITION;
				float2 texture_coordinates : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;

				fragment_data.screen_position = 
					UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = 
					TRANSFORM_TEX(vertex_data.texture_coordinates, _MainTex);

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				return tex2D(_MainTex, fragment_data.texture_coordinates) * 
					   _Color;
			}
            ENDCG
        }
    }
}

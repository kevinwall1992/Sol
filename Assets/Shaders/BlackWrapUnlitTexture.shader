Shader "Unlit/BlackWrapUnlitTexture"
{
    Properties
    {
		[HideInInspector]
        _MainTex("Texture", 2D) = "white" {}
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

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				if (fragment_data.texture_coordinates.x < 0 || fragment_data.texture_coordinates.x > 1 ||
				   fragment_data.texture_coordinates.y < 0 || fragment_data.texture_coordinates.y > 1)
					return fixed4(0, 0, 0, 1);

				return tex2D(_MainTex, fragment_data.texture_coordinates);
			}
            ENDCG
        }
    }
}
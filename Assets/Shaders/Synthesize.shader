Shader "Unlit/Sythesize"
{
    Properties
    {
        Pixelized("Pixelized", 2D) = "white" {}
		Diffused("Diffused", 2D) = "white" {}
		Ambient_("Ambient", 2D) = "white" {}

		PixelizedWeight("PixelizedWeight", Range(0, 2)) = 1
		DiffusedWeight("DiffusedWeight", Range(0, 2)) = 1
		AmbientWeight("AmbientWeight", Range(0, 2)) = 1
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


			sampler2D Pixelized;
			sampler2D Diffused;
			sampler2D Ambient_;

			float PixelizedWeight;
			float DiffusedWeight;
			float AmbientWeight;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				return PixelizedWeight * tex2D(Pixelized, fragment_data.texture_coordinates) +
					   DiffusedWeight * tex2D(Diffused, fragment_data.texture_coordinates) +
					   AmbientWeight * tex2D(Ambient_, fragment_data.texture_coordinates);
			}
            ENDCG
        }
    }
}

Shader "Unlit/Sythesize"
{
    Properties
    {
		BlankScreen("BlankScreen", 2D) = "white" {}
        Pixelized("Pixelized", 2D) = "white" {}
		Diffused("Diffused", 2D) = "white" {}
		Ambient_("Ambient", 2D) = "white" {}

		BlankScreenWeight("BlankScreenWeight", Range(0, 2)) = 1
		PixelizedWeight("PixelizedWeight", Range(0, 2)) = 1
		DiffusedWeight("DiffusedWeight", Range(0, 2)) = 1
		AmbientWeight("AmbientWeight", Range(0, 2)) = 1

		EdgeColor("EdgeColor", Color) = (0, 0, 0, 1)
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

			sampler2D BlankScreen;
			sampler2D Pixelized;
			sampler2D Diffused;
			sampler2D Ambient_;

			float BlankScreenWeight;
			float PixelizedWeight;
			float DiffusedWeight;
			float AmbientWeight;

			int MonitorResolutionX;
			int MonitorResolutionY;

			fixed4 EdgeColor;

			float relative_image_size;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				fixed4 blank_screen_color =
					tex2D(BlankScreen, (fragment_data.texture_coordinates + (1 - relative_image_size) / 2) *
									   float2(MonitorResolutionX, MonitorResolutionY) / 
									   relative_image_size);

				if ((fragment_data.texture_coordinates.x < (1 - relative_image_size) / 2) ||
				    (fragment_data.texture_coordinates.x > 1 - (1 - relative_image_size) / 2) ||
				    (fragment_data.texture_coordinates.y < (1 - relative_image_size) / 2) ||
				    (fragment_data.texture_coordinates.y > 1 - (1 - relative_image_size) / 2))
					blank_screen_color = EdgeColor;

				return BlankScreenWeight * blank_screen_color +
					   PixelizedWeight * tex2D(Pixelized, fragment_data.texture_coordinates) +
					   DiffusedWeight * tex2D(Diffused, fragment_data.texture_coordinates) +
					   AmbientWeight * tex2D(Ambient_, fragment_data.texture_coordinates);
			}
            ENDCG
        }
    }
}

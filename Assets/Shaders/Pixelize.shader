Shader "Unlit/Pixelize"
{
    Properties
    {
        Image ("Image", 2D) = "white" {}
		PixelTexture("PixelTexture", 2D) = "white" {}
		MonitorResolutionX("MonitorResolutionX", Int) = 455
		MonitorResolutionY("MonitorResolutionY", Int) = 256
		PixelSize("PixelSize", Float) = 2
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


			sampler2D Image;
			sampler2D PixelTexture;

			int MonitorResolutionX;
			int MonitorResolutionY;
			float PixelSize;

			static int pixel_offsets[10] = { 0,  0,
										     -1,  0,
											  1,  0,
											  0, -1,
											  0,  1 };

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				float2 normalized_pixel_size = float2(PixelSize / MonitorResolutionX, 
													  PixelSize / MonitorResolutionY);

				int nearest_pixel_x = int(fragment_data.texture_coordinates.x * MonitorResolutionX);
				int nearest_pixel_y = int(fragment_data.texture_coordinates.y * MonitorResolutionY);

				float2 image_space_pixel_coordinates[5];

				for (int i = 0; i < 5; i++)
				{
					image_space_pixel_coordinates[i] = float2(
						(nearest_pixel_x + pixel_offsets[i * 2 + 0] + 0.5f) / float(MonitorResolutionX),
						(nearest_pixel_y + pixel_offsets[i * 2 + 1] + 0.5f) / float(MonitorResolutionY));
				}

				float4 total_light = float4(0, 0, 0, 0);

				for (i = 0; i < 5; i++)
				{
					float2 pixel_texture_coordinates = 
						(fragment_data.texture_coordinates - image_space_pixel_coordinates[i]) /
						normalized_pixel_size + 
						float2(0.5f, 0.5f);

					total_light += tex2D(Image, image_space_pixel_coordinates[i]) *
								   tex2D(PixelTexture, pixel_texture_coordinates);
				}

				return 2.0f * total_light / 1.0f;
			}
            ENDCG
        }
    }
}

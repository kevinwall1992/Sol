Shader "Unlit/Pixelize"
{
    Properties
    {
		[HideInInspector]
		_MainTex("Texture", 2D) = "white" {}

		PixelTexture("PixelTexture", 2D) = "white" {}

		MonitorResolutionX("MonitorResolutionX", Int) = 455
		MonitorResolutionY("MonitorResolutionY", Int) = 256
		RelativeImageSize("RelativeImageSize", Range(0.7, 1)) = 0.9
		PixelScale("PixelSize", Range(0.5, 5)) = 2

		DarkPixelSizeModifier("DarkPixelSizeModifier", Range(0.001, 1)) = 0.5
		Brightness("Brightness", Range(0.1, 2)) = 1
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
			sampler2D PixelTexture;

			int MonitorResolutionX;
			int MonitorResolutionY;
			float RelativeImageSize;
			float PixelScale;

			float DarkPixelSizeModifier;

			float Brightness;

			static int pixel_offsets[18] = { -1, -1,
											 -1,  0,
											 -1,  1,
											  0, -1,
											  0,  0,
											  0,  1,
											  1, -1,
											  1,  0,
											  1,  1 };

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates * 1 / RelativeImageSize - 
													(float2(1, 1) / RelativeImageSize - float2(1, 1)) / 2 ;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				float2 normalized_pixel_size = float2(PixelScale / MonitorResolutionX, 
													  PixelScale / MonitorResolutionY);

				int nearest_pixel_x = int(fragment_data.texture_coordinates.x * MonitorResolutionX);
				int nearest_pixel_y = int(fragment_data.texture_coordinates.y * MonitorResolutionY);

				float2 pixel_coordinates[9];

				for (int i = 0; i < 9; i++)
				{
					pixel_coordinates[i] = float2(
						(nearest_pixel_x + pixel_offsets[i * 2 + 0] + 0.5f) / float(MonitorResolutionX),
						(nearest_pixel_y + pixel_offsets[i * 2 + 1] + 0.5f) / float(MonitorResolutionY));
				}

				float4 total_light = float4(0, 0, 0, 0);

				for (i = 0; i < 9; i++)
				{
					if (pixel_coordinates[i].x < 0 || pixel_coordinates[i].x > 1 ||
						pixel_coordinates[i].y < 0 || pixel_coordinates[i].y > 1)
						continue;

					fixed4 sample_color = tex2D(_MainTex, pixel_coordinates[i]);
					
					float shrinkage = (max(max(sample_color.r, sample_color.g), sample_color.b) + 
									  (1 / DarkPixelSizeModifier - 1)) * 
								      DarkPixelSizeModifier;

					float2 pixel_texture_coordinates = 
						(fragment_data.texture_coordinates - pixel_coordinates[i]) /
						(normalized_pixel_size * shrinkage) +
						float2(0.5f, 0.5f);

					total_light += sample_color *
						tex2D(PixelTexture, pixel_texture_coordinates) /
						(shrinkage * shrinkage);
				}

				return Brightness * total_light;
			}
            ENDCG
        }
    }
}

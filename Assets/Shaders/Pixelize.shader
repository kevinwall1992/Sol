Shader "Unlit/Pixelize"
{
    Properties
    {
		[HideInInspector]
		_MainTex("Texture", 2D) = "white" {}

		PixelTexture("PixelTexture", 2D) = "white" {}

		RelativeImageSize("RelativeImageSize", Range(0.7, 1)) = 0.9
		PixelScale("PixelSize", Range(0.5, 5)) = 2

		DarkPixelSizeModifier("DarkPixelSizeModifier", Range(0.001, 1)) = 0.5
		Brightness("Brightness", Range(0.1, 2)) = 1
		Saturation("Saturation", Range(0, 1)) = 1
		WhiteShift("WhiteShift", Range(0, 1)) = 0
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
			float Saturation;
			float WhiteShift;

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
													(float2(1, 1) / RelativeImageSize - float2(1, 1)) / 2;

				return fragment_data;
			}

			float GetIntensity(float4 color)
			{
				return 0.213 * color.r +
					   0.715 * color.g +
					   0.072 * color.b;
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

				float total_sample_intensity = 0;
				float total_sample_intensity_weight = 0;

				for (i = 0; i < 9; i++)
				{
					if (pixel_coordinates[i].x < 0 || pixel_coordinates[i].x > 1 ||
						pixel_coordinates[i].y < 0 || pixel_coordinates[i].y > 1)
						continue;

					float4 sample_color = tex2D(_MainTex, pixel_coordinates[i]);

					//Saturation

					float sample_intensity = GetIntensity(sample_color);
					sample_color = lerp(sample_color / sample_intensity,
										float4(1, 1, 1, 1), 
										1 - Saturation) * 
								   sample_intensity;


					//"White Shift"
					//Extremely pure colors are shifted towards white.

					float mean = (sample_color.r + sample_color.g + sample_color.b) / 3;
					float deviation = (
						pow((sample_color.r - mean), 2) * 0.6 +
						pow((sample_color.g - mean), 2) * 2.2 +
						pow((sample_color.b - mean), 2) * 0.2) / 0.666;
					sample_color = lerp(sample_color, 
										float4(1, 1, 1, 1), 
										deviation * WhiteShift);

					sample_intensity = GetIntensity(sample_color);
					
					float shrinkage = (max(max(sample_color.r, sample_color.g), sample_color.b) + 
									  (1 / DarkPixelSizeModifier - 1)) * 
								      DarkPixelSizeModifier;

					float2 pixel_texture_coordinates = 
						(fragment_data.texture_coordinates - pixel_coordinates[i]) /
						(normalized_pixel_size * shrinkage) +
						float2(0.5f, 0.5f);

					float4 light = sample_color *
						tex2D(PixelTexture, pixel_texture_coordinates) /
						(shrinkage * shrinkage);

					float weight = GetIntensity(light);

					total_light += light * weight;

					total_sample_intensity += weight * sample_intensity;
					total_sample_intensity_weight += weight;
				}

				//The idea here is to adjust hue towards light samples with higher 
				//intensity, hoping to improve legibility of white-on-bright-color 
				//thin text. This was not really observed to significant extent, 
				//however this change did have observed effect of smoothing out 
				//the appearence of rows of darker pixels (esp. grays). So it 
				//has been kept as is. 
				//It also has the side effect of reducing "total"_light to more of
				//a weighted average, which is theoretically not really sound because
				//the next calculation is designed to adjust the results of summing
				//up adjacent pixels... But it seems to work, so...

				total_light /= total_sample_intensity_weight;


				//What happens here is we compute the maximum intensity allowed as 
				//the intensity of underlying image weighted by its "light sample's"
				//(the resulting color sampled from the pixel texture)
				//intensity. 
				
				//This means that (in theory) two adjacent pixels of the same color 
				//do not add up to be brightest halfway between eachother, because
				//the brightness cannot exceed the brightness of the underlying color.

				//After computing the new intensity, we normalize total_light and then
				//multiply it by the new intensity.

				float max_intensity = total_sample_intensity / 
								      total_sample_intensity_weight;
				float intensity = GetIntensity(total_light);
				float adjusted_intensity = min(intensity, max_intensity);

				return float4((
					total_light / intensity * 
					adjusted_intensity * 
					Brightness).rgb, 1);
			}
            ENDCG
        }
    }
}

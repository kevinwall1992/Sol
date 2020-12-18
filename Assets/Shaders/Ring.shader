Shader "Unlit/Ring"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		GeometryTexture("GeometryTexture", 2D) = "white" {}

		WireFrameColor("WireFrameColor", Color) = (0, 0, 0, 1)
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
				float2 polar_coordinates : TEXCOORD1;
				float3 position : TEXCOORD2;
			};

			#define PI 3.14159 

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D GeometryTexture;

			fixed4 WireFrameColor;

			sampler2D_float WingIndices;
			sampler2D_float WingPositions;
			sampler2D_float WingWidths;
			sampler2D WingColors;

			float4 WingIndices_TexelSize;
			float4 WingPositions_TexelSize;
			float4 WingColors_TexelSize;

			float FloorIndices[256];
			float FloorRadii[128];
			float FloorHeights[128];

			float OuterRadius;
			float InnerRadius;
			float RingDepth;

			float Linearity;
			float WingVisibility;
			float ApplyWidthCorrection;
			float WireframeVisibility;

			float FloorDivisor;
			float InterstitialSpaceThickness;

			float WingDivisor;
			float WallThickness;
			float WingDepth;

			FragmentData vert(VertexData vertex_data)
			{
				float4 geometry_data = tex2Dlod(
					GeometryTexture,
					float4(vertex_data.texture_coordinates, 0, 0));

				float radians = geometry_data.r * (2 * PI);
				
				float radius = 
					geometry_data.g * 
					(OuterRadius - InnerRadius) +
					InnerRadius;

				float linearized_radians = radians * (1 - Linearity);

				float linearized_outer_radius = OuterRadius / (1 - Linearity);
				float linearized_inner_radius = (linearized_outer_radius - OuterRadius) + InnerRadius;
				float linearized_radius = (linearized_outer_radius - OuterRadius) + radius;

				float3 position = float3(
					linearized_radius * sin(linearized_radians),
					linearized_radius * cos(linearized_radians) -
						(linearized_outer_radius - OuterRadius),
					geometry_data.b * RingDepth);


				FragmentData fragment_data;

				fragment_data.screen_position = 
					UnityObjectToClipPos(position);
				fragment_data.texture_coordinates = 
					TRANSFORM_TEX(vertex_data.texture_coordinates, _MainTex);
				fragment_data.polar_coordinates =
					float2(radius, radians);
				fragment_data.position =
					position;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				fixed4 wireframe_color = round(tex2D(_MainTex, fragment_data.texture_coordinates)) * 
										 WireFrameColor;
				if (fragment_data.position.z > 0)
					return wireframe_color;

				if (WireframeVisibility > 0.5 && fragment_data.position.z < WingDepth)
					discard;

				float radius = fragment_data.polar_coordinates[0];
				float radians = fragment_data.polar_coordinates[1];

				int floor_slot_index = floor((OuterRadius - radius) / FloorDivisor);
				int floor_index = round(FloorIndices[floor_slot_index]);
				float is_wing = 1;
				if (floor_index < 0)
					is_wing = 0;

				float floor_radius = FloorRadii[floor_index];
				float floor_height = FloorHeights[floor_index];
				float floor_length = floor_radius * 2 * PI;
				float meters = floor_length * radians / (2 * PI);
				float distance_above_floor = floor_radius - radius;

				float linearized_floor_length = lerp(
					floor_length,
					OuterRadius * 2 * PI,
					Linearity * ApplyWidthCorrection);
				float floor_length_ratio = linearized_floor_length / floor_length;

				int wing_slot_index = floor(meters / WingDivisor);
				int wing_index = round(tex2D(
					WingIndices, 
					float2(wing_slot_index / 4.0, floor_index) * 
					WingIndices_TexelSize)[(uint)wing_slot_index % 4]);
				
				if (wing_index < 0)
					is_wing = 0;

				float wing_color_index = (wing_index) * WingColors_TexelSize.x;
				float2 wing_coordinates = float2(wing_color_index % 1, 
												 floor(wing_color_index) * WingColors_TexelSize.y);
				fixed4 wing_color = tex2D(WingColors, wing_coordinates);

				float wing_position_index = (wing_index / 4.0) * WingPositions_TexelSize.x;
				wing_coordinates = float2(wing_position_index % 1, 
										  floor(wing_position_index) * WingPositions_TexelSize.y);
				float wing_position = tex2D(WingPositions, wing_coordinates)[(uint)wing_index % 4];
				float wing_length = tex2D(WingWidths, wing_coordinates)[(uint)wing_index % 4];
				
				float wing_space = wing_length * floor_length_ratio;
				float initial_empty_length = (wing_space - wing_length + WallThickness) / 2;
				float wing_meters = meters - wing_position;

				if (wing_meters < initial_empty_length / floor_length_ratio ||
					wing_meters > (initial_empty_length + wing_length - WallThickness) / floor_length_ratio)
					is_wing = 0;
				if (distance_above_floor < InterstitialSpaceThickness / 2 ||
					distance_above_floor > (floor_height - InterstitialSpaceThickness / 2))
					is_wing = 0;

				return lerp(wireframe_color, 
							fixed4(wing_color.rgb * is_wing, 1), 
							WingVisibility);
			}
            ENDCG
        }
    }
}

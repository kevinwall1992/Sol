Shader "Unlit/Ring"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		GeometryTexture("GeometryTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		Cull Off

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
			};

			#define PI 3.14159 

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D GeometryTexture;

			sampler2D_float RoomIndices;
			sampler2D_float RoomPositions;
			sampler2D_float RoomLengths;
			sampler2D RoomColors;

			float4 RoomIndices_TexelSize;
			float4 RoomPositions_TexelSize;
			float4 RoomColors_TexelSize;

			float FloorIndices[256];
			float FloorRadii[128];
			float FloorHeights[128];

			float OuterRadius;
			float InnerRadius;
			float Thickness;

			float Linearity;
			float Rationality;
			float Coloration;

			float UnitFloorHeight;
			float FloorMargin;

			float UnitRoomLength;
			float RoomMargin;

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
					geometry_data.b * Thickness);


				FragmentData fragment_data;

				fragment_data.screen_position = 
					UnityObjectToClipPos(position);
				fragment_data.texture_coordinates = 
					TRANSFORM_TEX(vertex_data.texture_coordinates, _MainTex);
				fragment_data.polar_coordinates =
					float2(radius, radians);

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				fixed4 wireframe_color = tex2D(_MainTex, fragment_data.texture_coordinates);

				float radius = fragment_data.polar_coordinates[0];
				float radians = fragment_data.polar_coordinates[1];

				int floor_slot_index = floor((OuterRadius - radius) / UnitFloorHeight);
				int floor_index = FloorIndices[floor_slot_index];
				float is_room = 1;
				if (floor_index < 0)
					is_room = 0;

				float floor_radius = FloorRadii[floor_index];
				float floor_height = FloorHeights[floor_index] * UnitFloorHeight;
				float floor_length = floor_radius * 2 * PI;
				float meters = floor_length * radians / (2 * PI);
				float distance_above_floor = floor_radius - radius;

				float linearized_floor_length = lerp(
					floor_length,
					OuterRadius * 2 * PI,
					Linearity * (1 - Rationality));
				float floor_length_ratio = linearized_floor_length / floor_length;

				int room_slot_index = floor(meters / UnitRoomLength);
				int room_index = round(tex2D(
					RoomIndices, 
					float2(room_slot_index / 4.0, floor_index) * 
					RoomIndices_TexelSize)[(uint)room_slot_index % 4]);
				
				if (room_index < 0)
					is_room = 0;

				float room_color_index = (room_index) * RoomColors_TexelSize.x;
				float2 room_coordinates = float2(room_color_index % 1, 
												 floor(room_color_index) * RoomColors_TexelSize.y);
				fixed4 room_color = tex2D(RoomColors, room_coordinates);

				float room_position_index = (room_index / 4.0) * RoomPositions_TexelSize.x;
				room_coordinates = float2(room_position_index % 1, 
										  floor(room_position_index) * RoomPositions_TexelSize.y);
				float room_position = tex2D(RoomPositions, room_coordinates)[(uint)room_index % 4];
				float room_length = tex2D(RoomLengths, room_coordinates)[(uint)room_index % 4] *
									UnitRoomLength;
				
				float room_space = room_length * floor_length_ratio;
				float initial_empty_length = (room_space - room_length + RoomMargin) / 2;
				float room_meters = meters - room_position;

				if (room_meters < initial_empty_length / floor_length_ratio ||
					room_meters > (initial_empty_length + room_length - RoomMargin) / floor_length_ratio)
					is_room = 0;
				if (distance_above_floor < FloorMargin / 2 ||
					distance_above_floor > (floor_height - FloorMargin / 2))
					is_room = 0;

				return lerp(wireframe_color, 
							room_color * is_room, 
							Coloration);
			}
            ENDCG
        }
    }
}

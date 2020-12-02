Shader "Unlit/Ring"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		GeometryTexture("GeometryTexture", 2D) = "white" {}

		OuterRadius("OuterRadius", Float) = 1
		Thickness("Thickness", Float) = 1

		Linearity("Linearity", Range(0, 0.9999)) = 0
		Coloration("Coloration", Range(0, 1)) = 0
		
		RoomLength("RoomLength", Float) = 1
		RoomMargin("RoomMargin", Float) = 0.1
		FloorCount("FloorCount", Int) = 4
		FloorHeight("FloorHeight", Int) = 4
		FloorMargin("FloorMargin", Float) = 0.05
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

			float OuterRadius;
			float Thickness;

			float Linearity;
			float Coloration;

			float RoomLength;
			float RoomMargin;
			int FloorCount;
			float FloorHeight;
			float FloorMargin;

			FragmentData vert(VertexData vertex_data)
			{
				float4 geometry_data = tex2Dlod(
					GeometryTexture,
					float4(vertex_data.texture_coordinates, 0, 0));

				float inner_radius = OuterRadius - FloorCount * FloorHeight;
				float linearized_outer_radius = OuterRadius / (1 - Linearity);
				float linearized_inner_radius = inner_radius / (1 - Linearity);

				float radians = geometry_data.r * (2 * PI);
				float linearized_radians = radians * (1 - Linearity);

				float radius = 
					geometry_data.g * 
					(OuterRadius - inner_radius) +
					inner_radius;
				float linearized_radius =
					geometry_data.g * 
					(linearized_outer_radius - linearized_inner_radius) +
					linearized_inner_radius;

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
				fixed4 room_color = fixed4(1, 0, 0, 1);

				float floor_index = clamp(floor((OuterRadius - radius) / FloorHeight), 0, FloorCount - 1);
				float floor_radius = OuterRadius - floor_index * FloorHeight;
				float floor_length = floor_radius * 2 * PI;
				float linearized_floor_length = lerp(floor_length, OuterRadius * 2 * PI, Linearity);

				float room_count = floor_length / RoomLength;
				float room_pitch = linearized_floor_length / room_count;

				float meters = linearized_floor_length * radians / (2 * PI);

				float is_room = (1 - floor(meters % room_pitch / (RoomLength - RoomMargin))) *
								(1 - floor(meters / (floor(room_count) * room_pitch))) *
								(1 - floor((OuterRadius - radius) * 1.000001 % FloorHeight / (FloorHeight - FloorMargin)));
				is_room = clamp(is_room, 0, 1);

				return lerp(wireframe_color, 
							room_color * is_room, Coloration);
			}
            ENDCG
        }
    }
}

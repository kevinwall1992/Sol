Shader "Unlit/Ring"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		GeometryTexture("GeometryTexture", 2D) = "white" {}

		OuterRadius("OuterRadius", Float) = 1
		InnerRadius("InnerRadius", Float) = 0.5
		Thickness("Thickness", Float) = 1

		Linearity("Linearity", Range(0, 0.999)) = 0
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
			};


			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D GeometryTexture;

			float OuterRadius;
			float InnerRadius;
			float Thickness;
			float Linearity;

			FragmentData vert(VertexData vertex_data)
			{
				float imaginary_inner_radius = InnerRadius / (1 - Linearity);
				float difference = imaginary_inner_radius - InnerRadius;
				float imaginary_outer_radius = difference + OuterRadius;

				float4 geometry_data = tex2Dlod(
					GeometryTexture, 
					float4(vertex_data.texture_coordinates, 0, 0));

				float radians = geometry_data.r * (2 * 3.14) * (1 - Linearity);
				float radius = 
					geometry_data.g * 
					(imaginary_outer_radius - imaginary_inner_radius) + 
					imaginary_inner_radius;

				float3 world_position = float3(
					radius * sin(radians),
					radius * cos(radians) - difference,
					geometry_data.b * Thickness);


				FragmentData fragment_data;

				fragment_data.screen_position = 
					UnityObjectToClipPos(world_position);
				fragment_data.texture_coordinates = 
					TRANSFORM_TEX(vertex_data.texture_coordinates, _MainTex);

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				return tex2D(_MainTex, fragment_data.texture_coordinates);
			}
            ENDCG
        }
    }
}

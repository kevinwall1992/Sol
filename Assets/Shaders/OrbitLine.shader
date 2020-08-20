Shader "Unlit/OrbitLine"
{
    Properties
    {
        _MainTex ("Color (RGB) Alpha (A)", 2D) = "white" {}

		[PerRendererData]
		PathLength("PathLength", float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

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
			float4 _MainTex_ST;

			float UnitsPerRepetition;
			float PathLength;
	
			float2 MaskMin;
			float2 MaskMax;

			float3 ObjectPosition;
			float ObjectSize;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = TRANSFORM_TEX(vertex_data.texture_coordinates, _MainTex);

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				if (MaskMin.x > fragment_data.screen_position.x ||
					MaskMin.y > fragment_data.screen_position.y ||
					MaskMax.x < fragment_data.screen_position.x ||
					MaskMax.y < fragment_data.screen_position.y)
					discard;

				float distance_to_object = 
					length(fragment_data.screen_position.xy - ObjectPosition.xy);
				if (distance_to_object < ObjectSize * 0.6f)
					discard;

				return tex2D(_MainTex, float2(
					fragment_data.texture_coordinates.x * PathLength,
					fragment_data.texture_coordinates.y));
			}
            ENDCG
        }
    }
}

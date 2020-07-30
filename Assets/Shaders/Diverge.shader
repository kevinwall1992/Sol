Shader "Unlit/Diverge"
{
    Properties
    {
		[HideInInspector]
		_MainTex("Texture", 2D) = "white" {}

		RedConvergence("RedConvergence", Range(0.99, 1.01)) = 1
		GreenConvergence("GreenConvergence", Range(0.99, 1.01)) = 1
		BlueConvergence("BlueConvergence", Range(0.99, 1.01)) = 1

		ConvergenceModifierX("ConvergenceModifierX", Range(0, 1)) = 1
		ConvergenceModifierY("ConvergenceModifierY", Range(0, 1)) = 1
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

			float RedConvergence;
			float GreenConvergence;
			float BlueConvergence;

			float ConvergenceModifierX;
			float ConvergenceModifierY;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				float2 red_texture_coordinates = 
					(fragment_data.texture_coordinates - float2(0.5f, 0.5f)) * 
					float2(lerp(RedConvergence, 1, 1 - ConvergenceModifierX), 
						   lerp(RedConvergence, 1, 1 - ConvergenceModifierY)) + 
					float2(0.5f, 0.5f);

				float2 green_texture_coordinates = 
					(fragment_data.texture_coordinates - float2(0.5f, 0.5f)) * 
					float2(lerp(GreenConvergence, 1, 1 - ConvergenceModifierX), 
						   lerp(GreenConvergence, 1, 1 - ConvergenceModifierY)) + 
					float2(0.5f, 0.5f);

				float2 blue_texture_coordinates =
					(fragment_data.texture_coordinates - float2(0.5f, 0.5f)) *
					float2(lerp(BlueConvergence, 1, 1 - ConvergenceModifierX),
						lerp(BlueConvergence, 1, 1 - ConvergenceModifierY)) +
					float2(0.5f, 0.5f);

				return fixed4(tex2D(_MainTex, red_texture_coordinates).r,
							  tex2D(_MainTex, green_texture_coordinates).g,
							  tex2D(_MainTex, blue_texture_coordinates).b,
							  1);
			}
            ENDCG
        }
    }
}

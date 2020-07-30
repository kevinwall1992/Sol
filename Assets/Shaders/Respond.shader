Shader "Unlit/Respond"
{
    Properties
    {
		[HideInInspector]
		_MainTex("Texture", 2D) = "white" {}

		Signal("Signal", 2D) = "white" {}

		RedResponseTime("RedResponseTime", Range(0, 200)) = 0
		GreenResponseTime("GreenResponseTime", Range(0, 200)) = 0
		BlueResponseTime("BlueResponseTime", Range(0, 200)) = 0

		RedDecayTime("RedDecayTime", Range(0, 200)) = 0
		GreenDecayTime("GreenDecayTime", Range(0, 200)) = 0
		BlueDecayTime("BlueDecayTime", Range(0, 200)) = 0
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
			sampler2D Signal;

			float RedResponseTime;
			float GreenResponseTime;
			float BlueResponseTime;

			float RedDecayTime;
			float GreenDecayTime;
			float BlueDecayTime;

			float time_delta;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				fixed4 signal = tex2D(Signal, fragment_data.texture_coordinates);
				fixed4 previous_response = tex2D(_MainTex, fragment_data.texture_coordinates);

				fixed4 response = fixed4(0, 0, 0, 1);

				if (signal.r > previous_response.r)
					response.r = lerp(previous_response.r, signal.r, 1000 * time_delta / RedResponseTime);
				else
					response.r = lerp(previous_response.r, signal.r, 1000 * time_delta / RedDecayTime);

				if (signal.g > previous_response.g)
					response.g = lerp(previous_response.g, signal.g, 1000 * time_delta / GreenResponseTime);
				else
					response.g = lerp(previous_response.g, signal.g, 1000 * time_delta / GreenDecayTime);

				if (signal.b > previous_response.b)
					response.b = lerp(previous_response.b, signal.b, 1000 * time_delta / BlueResponseTime);
				else
					response.b = lerp(previous_response.b, signal.b, 1000 * time_delta / BlueDecayTime);

				return response;
			}
            ENDCG
        }
    }
}

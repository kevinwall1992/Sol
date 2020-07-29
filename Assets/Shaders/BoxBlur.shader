Shader "Unlit/BoxBlur"
{
    Properties
    {
		[HideInInspector]
        _MainTex("Texture", 2D) = "white" {}
		
		KernelSize("KernelSize", Int) = 3
		IsHorizontal("IsHorizontal", Int) = 0
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


			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			int KernelSize;
			bool IsHorizontal;

			FragmentData vert(VertexData vertex_data)
			{
				FragmentData fragment_data;
				fragment_data.screen_position = UnityObjectToClipPos(vertex_data.position);
				fragment_data.texture_coordinates = vertex_data.texture_coordinates;

				return fragment_data;
			}

			fixed4 frag(FragmentData fragment_data) : SV_Target
			{
				float4 sum = float4(0, 0, 0, 0);

				for (int i = 0; i < KernelSize; i++)
					if (IsHorizontal)
						sum += tex2D(_MainTex, fragment_data.texture_coordinates + float2((i - KernelSize / 2) * _MainTex_TexelSize.x, 0));
					else
						sum += tex2D(_MainTex, fragment_data.texture_coordinates + float2(0, (i - KernelSize / 2) * _MainTex_TexelSize.y));
						
				return fixed4(sum / KernelSize);
			}
            ENDCG
        }
    }
}

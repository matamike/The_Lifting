Shader "Unlit/Painter"
{
    Properties
    {
        _Tex ("InputTex", 2D) = "white" {}
        _Corners ("Corners", Vector) = (0.5, 1, 1, 0.5)
        _CirclePosition ("Center", Vector) = (0, 0, 0, 0)
        _Radius ("Radius", Float) = 1.0
		_DirtRate ("DirtRate",Float) = 0.1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag alpha:fade
            

            #include "UnityCG.cginc"
            
            sampler2D _Tex;
            float4 _Corners;
			float _DirtRate;
			float3 _CirclePosition;
			float _Radius;
			
            float random (float2 uv)
            {
                return frac(sin(_Time.y * 3 * dot(uv,float2(12.9898,78.233)))*43758.5453123);
            }
            
            float4 frag (v2f_customrendertexture IN) : COLOR
            {
                float4 col = tex2D(_Tex, IN.globalTexcoord.xy);
                //if (IN.globalTexcoord.x < _Corners.x && IN.globalTexcoord.y < _Corners.y && IN.globalTexcoord.x > _Corners.z && IN.globalTexcoord.y > _Corners.w)
                if (distance(IN.globalTexcoord.xy, _CirclePosition.xy) < _Radius)
                {
					col.a = 0;
                    return col;
                }
                
                float r = random(IN.globalTexcoord.xy); 
                
                if (r > _DirtRate)
                {
                    col.a = 1;
                }
				//col.a = max(col.a, step(random(IN.globalTexcoord.xy), _DirtRate));
				return col;
            }
            ENDCG
        }
    }
}

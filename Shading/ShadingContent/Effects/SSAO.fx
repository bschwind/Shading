#include <HelperFunctions.fxh>

float4x4 World;
float4x4 View;
float4x4 Projection;

texture depthMap;
texture normalMap;

sampler DepthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler NormalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 PixelShaderFunction(PostProcessVSOutput input) : COLOR0
{
    float depth = tex2D(DepthSampler, input.TexCoord);
	float3 normal = tex2D(NormalSampler, input.TexCoord);
	float3 pos = PositionFromDepth(depth, input.TexCoord);
	float fun = distance(pos, float3(0,0,-1));
	return float4(fun, fun, fun, 1);
	//return float4(0, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

#include <HelperFunctions.fxh>

float4x4 World;
float4x4 View;
float4x4 Projection;

texture depthMap;
texture normalMap;

#define SAMPLE_COUNT 16
float3 Samples[SAMPLE_COUNT];
float radius = 1.0;

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
	float3 normal = 2 * tex2D(NormalSampler, input.TexCoord) - 1;
	float3 pos = PositionFromDepth(depth, input.TexCoord);

	float3 tangent = normalize(float3(1, 2, -2));
	float3 bitangent = cross(normal, tangent);
	float3x3 tbn = float3x3(tangent, bitangent, normal);

	float occlusion = 0.0;
	for(int i = 0; i < SAMPLE_COUNT; i++)
	{
		float3 samplePos = mul(Samples[i], tbn);
		samplePos = samplePos * radius + pos;
		float4 offset = float4(samplePos, 1.0);
		offset = mul(offset, Projection);
		offset.xy /= offset.w;
		offset.xy = offset.xy * 0.5 + 0.5;

		float sampleDepth = tex2D(depthMap, offset.xy).r;

		float rangeCheck = abs(pos.z - sampleDepth) < radius ? 1.0 : 0.0;
		occlusion += (sampleDepth <= samplePos.z ? 1.0 : 0.0) * rangeCheck;
	}

	occlusion = 1.0 - (occlusion / SAMPLE-COUNT);

	return float4(occlusion, occlusion, occlusion, 1);
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

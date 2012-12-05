#include <HelperFunctions.fxh>

float4x4 View;
float4x4 Projection;

texture depthMap;
texture normalMap;
texture noiseMap;

#define SAMPLE_COUNT 16
float3 Samples[SAMPLE_COUNT];
float radius = 2;

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

sampler NoiseSampler = sampler_state
{
    Texture = (noiseMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 PixelShaderFunction(PostProcessWithViewRayVSOutput input) : COLOR0
{
    float depth = tex2D(DepthSampler, input.TexCoord).r;
	float3 normal = 2 * tex2D(NormalSampler, input.TexCoord).xyz - 1;
	//float3 pos = WorldPositionFromDepth(depth, input.TexCoord);
	float3 origin = depth * input.ViewRay;
	//float3 pos = ViewPositionFromDepth(depth, input.TexCoord);

	//return float4(-pos.z/farPlane, -pos.z/farPlane, -pos.z/farPlane, 1);

	float3 rvec = 2 * tex2D(NoiseSampler, input.TexCoord * 30).xyz - 1;
	//float3 rvec = normalize(float3(0.3, 0.3, 0));
	float3 tangent = normalize(rvec-normal * dot(rvec, normal));
	float3 bitangent = cross(normal, tangent);
	//float3x3 tbn = compute_tangent_frame(normal, pos, input.TexCoord);
	float3x3 tbn = float3x3(tangent, bitangent, normal);
	//return float4(mul(float3(0,0,-1), tbn), 1);

	float occlusion = 0.0;
	for(int i = 0; i < SAMPLE_COUNT; i++)
	{
		float3 sample = mul(Samples[i], tbn) * radius;
		sample = sample * radius + origin;
		float4 offset = float4(sample, 1.0);
		offset = mul(offset, Projection);
	 	offset.xy /= offset.w;
		offset.xy = offset.xy * 0.5 + 0.5;

		float sampleDepth = tex2D(DepthSampler, offset.xy).r;

		float rangeCheck = abs(origin.z - sampleDepth) < radius ? 1.0 : 0.0;
		occlusion += (sampleDepth <= sample.z ? 1.0 : 0.0);
		//occlusion *= rangeCheck;
	}

	//occlusion *= dot(normal, normal);

	/*for(int i = 0; i < SAMPLE_COUNT; i++)
	{
		float3 samplePos = mul(Samples[i], tbn);
		samplePos = samplePos * radius + pos;
		float4 offset = float4(samplePos, 1.0);
		offset = mul(offset, Projection);
		offset.xy /= offset.w;
		offset.xy = offset.xy * 0.5 + 0.5;

		float sampleDepth = tex2D(DepthSampler, offset.xy).r;

		occlusion += -1 * sign(pos.z - sampleDepth);

		float rangeCheck = abs(pos.z - sampleDepth) < radius ? 1.0 : 0.0;
		//occlusion += (sampleDepth <= samplePos.z ? 1.0 : 0.0) * rangeCheck;

		occlusion += sign(sampleDepth - pos.z);
	}*/

	occlusion = 1.0 - (occlusion / SAMPLE_COUNT);

	return float4(occlusion, occlusion, occlusion, 1);
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 PostProcessWithViewRayVS();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

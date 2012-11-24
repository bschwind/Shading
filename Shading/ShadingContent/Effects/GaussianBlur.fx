#include <HelperFunctions.fxh>

texture tex;

sampler TextureSampler = sampler_state
{
    Texture = (tex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

#define SAMPLE_COUNT 10

float dx, dy;
float SampleWeights[SAMPLE_COUNT];

float4 HorizontalBlur(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
	c += tex2D(TextureSampler, texCoord) * SampleWeights[0];

	for(int i = 1; i < SAMPLE_COUNT; i++)
	{
		//We take steps of 1.5 away from the middle pixel in order to take advantage
		//of the hardware's bilinear filtering
		c += tex2D(TextureSampler, texCoord + float2(dx * ((i-1)*2 + 1.5), 0)) * SampleWeights[i];
	}

	for(int i = 1; i < SAMPLE_COUNT; i++)
	{
		c += tex2D(TextureSampler, texCoord + float2(-dx * ((i-1)*2 + 1.5), 0)) * SampleWeights[i];
	}
    
    return c;
}

float4 VerticalBlur(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
	c += tex2D(TextureSampler, texCoord) * SampleWeights[0];

	for(int i = 1; i < SAMPLE_COUNT; i++)
	{
		c += tex2D(TextureSampler, texCoord + float2(0, dy * ((i-1)*2 + 1.5))) * SampleWeights[i];
	}

	for(int i = 1; i < SAMPLE_COUNT; i++)
	{
		c += tex2D(TextureSampler, texCoord + float2(0, -dy * ((i-1)*2 + 1.5))) * SampleWeights[i];
	}
    
    return c;
}

technique HorizontalGaussianBlur
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 HorizontalBlur();
    }
}

technique VerticalGaussianBlur
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 VerticalBlur();
    }
}
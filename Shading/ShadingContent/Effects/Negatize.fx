#include <HelperFunctions.fxh>

texture tex;

sampler TextureSampler = sampler_state
{
    Texture = (tex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 Negatize(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
	c = tex2D(TextureSampler, texCoord);
	float a = c.a;
	
	c = 1-c;
	c.a = a;
	return c;
}

technique Negatize
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 Negatize();
    }
}
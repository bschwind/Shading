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
	return tex2D(TextureSampler, texCoord);
}

technique Negatize
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 Negatize();
    }
}
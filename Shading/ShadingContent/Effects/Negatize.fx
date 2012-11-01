sampler TextureSampler : register(s0);

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
        PixelShader = compile ps_2_0 Negatize();
    }
}
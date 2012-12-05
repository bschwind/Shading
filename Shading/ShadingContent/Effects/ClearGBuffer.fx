#include <HelperFunctions.fxh>

float farplane;

struct PixelShaderOutput
{
	float4 Color    : COLOR0;
	float4 Normal   : COLOR1;
	float4 Depth    : COLOR2;
};

PixelShaderOutput PixelShaderFunction()
{
	PixelShaderOutput output;

	output.Color = float4(0, 0, 0, 0);
    output.Normal = float4(0.5, 0.5, 0.5, 0);
	output.Depth = 1;

	return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

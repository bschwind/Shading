#include <HelperFunctions.fxh>

float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal   : NORMAL0;
};

struct VertexShaderOutput
{
    float4 PositionOut  : POSITION0;
	float3 Normal       : NORMAL0;
	float3 ViewSpacePos : TEXCOORD0;
};

struct PixelShaderOutput
{
	float4 Color    : COLOR0;
	float4 Normal   : COLOR1;
	float4 Depth    : COLOR2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.PositionOut = mul(viewPosition, Projection);
	output.ViewSpacePos = viewPosition;
	output.Normal = mul(mul(input.Normal, (float3x3)World),(float3x3)View);
    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;

	output.Color = float4(0.5, 0.5, 0.5, 1);
    output.Normal = float4((input.Normal.rgb + 1) * 0.5, 0);
	output.Depth = float4(-input.ViewSpacePos.z / farPlane, 1, 1, 1);

	return output;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

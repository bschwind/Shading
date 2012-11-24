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
    float4 PositionOut : POSITION0;
	float3 Normal      : NORMAL0;
	float Depth       : TEXCOORD0;
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
	output.Normal = mul(input.Normal, World);
	output.Depth = (-viewPosition.z - nearPlane) / (farPlane - nearPlane);
    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;

	output.Color = float4(0.5, 0.5, 0.5, 1);
    output.Normal = float4(input.Normal.rgb, 1);
	output.Depth = input.Depth;

	float3 pos = PositionFromDepth(output.Depth, float2(0, 0));

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

float2 halfPixel;
float nearPlane = 0.001;
float farPlane = 10;
float3 frustumCorners[8];

struct PostProcessVSInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PostProcessVSOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

PostProcessVSOutput PostProcessVS(PostProcessVSInput input)
{
    PostProcessVSOutput output;
    output.Position = float4(input.Position,1);
	output.Position.xy -= float2(halfPixel.x, -halfPixel.y);
    output.TexCoord = input.TexCoord;
    return output;
}

float3 PositionFromDepth(float depth, float2 screenCoord)
{
	return lerp(
    lerp(
        lerp(frustumCorners[0], frustumCorners[1], screenCoord.x),
        lerp(frustumCorners[3], frustumCorners[2], screenCoord.x),
        screenCoord.y),
    lerp(
        lerp(frustumCorners[4], frustumCorners[5], screenCoord.x),
        lerp(frustumCorners[7], frustumCorners[6], screenCoord.x),
        screenCoord.y),
    depth);
}
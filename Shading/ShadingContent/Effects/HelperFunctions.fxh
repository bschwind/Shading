float2 halfPixel;
float nearPlane = 0.001;
float farPlane = 10;
float3 frustumCorners[4];
float4x4 invProjection;

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

struct PostProcessWithViewRayVSOutput
{
    float4 Position     : POSITION0;
    float2 TexCoord     : TEXCOORD0;
	float3 ViewRay      : TEXCOORD1;
};

PostProcessVSOutput PostProcessVS(PostProcessVSInput input)
{
    PostProcessVSOutput output;
    output.Position = float4(input.Position.xy, 0, 1);
	output.Position.xy -= float2(halfPixel.x, -halfPixel.y);
    output.TexCoord = input.TexCoord;
    return output;
}

PostProcessWithViewRayVSOutput PostProcessWithViewRayVS(PostProcessVSInput input)
{
    PostProcessWithViewRayVSOutput output;
    output.Position = float4(input.Position.xy, 0, 1);
	output.Position.xy -= float2(halfPixel.x, -halfPixel.y);
    output.TexCoord = input.TexCoord;
	output.ViewRay = frustumCorners[input.Position.z];
    return output;
}

float3 WorldPositionFromDepth(float depth, float2 screenCoord)
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

float3 ViewPositionFromDepth(float Depth, float2 vTexCoord)
{
        // Get the depth value for this pixel
        float z = Depth; 
        // Get x/w and y/w from the viewport position
        float x = vTexCoord.x * 2 - 1;
        float y = (1 - vTexCoord.y) * 2 - 1;
        float4 vProjectedPos = float4(x, y, z, 1.0f);
        // Transform by the inverse projection matrix
        float4 vPositionVS = mul(vProjectedPos, invProjection );
        // Divide by w to get the view-space position
        return vPositionVS.xyz / vPositionVS.w;  
}

float3x3 compute_tangent_frame(float3 N, float3 P, float2 UV)
{
	float3 dp1 = ddx(P);
	float3 dp2 = ddy(P);
	float2 duv1 = ddx(UV);
	float2 duv2 = ddy(UV);

	float3x3 M = float3x3(dp1, dp2, cross(dp1, dp2));
	float2x3 inverseM = float2x3( cross( M[1], M[2] ), cross( M[2], M[0] ) );
	float3 T = mul(float2(duv1.x, duv2.x), inverseM);
	float3 B = mul(float2(duv1.y, duv2.y), inverseM);

	return float3x3(normalize(T), normalize(B), N);
}
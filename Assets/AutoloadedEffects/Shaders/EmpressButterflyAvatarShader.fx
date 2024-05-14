sampler baseTexture : register(s0);

float globalTime;
float horizontalScale;
float gradientCount;
float blurOffset;
float blurWeights[7];
float4 gradient[20];
float2 center;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    // Apply nonlinear horizontal scaling.
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos / pos.w;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 CalculateBlurColor(float2 coords)
{
    float4 baseColor = 0;
    for (int i = -3; i < 4; i++)
    {
        for (int j = -3; j < 4; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            float2 blurCoords = coords + float2(i, j) * blurOffset;
            baseColor += tex2D(baseTexture, blurCoords) * weight;
        }
    }
    return baseColor;
}

float4 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float scale = horizontalScale - distance(coords.x, 0.5) * lerp(0.5, 1.5, coords.y);
    coords.x = (coords.x - 0.5) / scale + 0.5;
    if (horizontalScale != 1)
        coords.y -= scale * 0.05;
    
    float4 baseColor = CalculateBlurColor(coords);
    
    float greyscale = dot(baseColor.rgb, float3(0.3, 0.6, 0.1));
    float evaluation = sin(globalTime * -3 + greyscale * 9) * 0.5 + 0.499;
    return PaletteLerp(evaluation) * baseColor.a * input.Color;
}
technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
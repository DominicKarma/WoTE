sampler baseTexture : register(s0);

float globalTime;
float gradientCount;
float blurOffset;
float blurWeights[7];
float4 gradient[20];

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

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = CalculateBlurColor(coords);
    
    float greyscale = dot(baseColor.rgb, float3(0.3, 0.6, 0.1));
    float evaluation = sin(globalTime * -3 + greyscale * 9) * 0.5 + 0.499;
    return PaletteLerp(evaluation) * baseColor.a * sampleColor;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
sampler baseTexture : register(s0);
sampler gradientTexture : register(s1);

float gradientCount;
float timeOffset;
float4 gradient[20];

float4 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(baseTexture, coords);
    float2 gradientCoords = baseColor.xy + float2(timeOffset, 0);
    float loopValue = frac(gradientCoords.x);
    gradientCoords.x = gradientCoords.x < 0 ? -loopValue : loopValue;
    
    return PaletteLerp(gradientCoords.x) * sampleColor * baseColor.a;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
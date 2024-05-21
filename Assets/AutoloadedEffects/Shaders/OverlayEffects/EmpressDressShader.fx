sampler baseTexture : register(s0);

float globalTime;
float gradientCount;
float2 baseTextureSize;
float4 gradient[20];

float4 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelationFactor = 2 / baseTextureSize;
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    float4 baseColor = tex2D(baseTexture, coords);
    float hueInterpolant = frac(pow(baseColor.r, 0.75) - globalTime * 0.7 + distance(coords, 0.5) * 2);
    float outlineInterpolant = smoothstep(0.2, 0.4, baseColor.r);
    return PaletteLerp(hueInterpolant) * baseColor.a * float4(outlineInterpolant, outlineInterpolant, outlineInterpolant, 1) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
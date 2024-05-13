sampler baseTexture : register(s0);

float globalTime;
float gradientCount;
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
    
    float greyscale = dot(baseColor.rgb, float3(0.3, 0.6, 0.1));
    float e = ddx(greyscale) + ddy(greyscale) * 2;
    float evaluation = (sin(globalTime * -3 - greyscale * 3) * 0.5 + 0.5);
    return PaletteLerp(evaluation) * baseColor.a * sampleColor;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
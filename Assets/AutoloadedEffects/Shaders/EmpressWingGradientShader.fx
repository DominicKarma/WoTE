sampler baseTexture : register(s0);
sampler gradientTexture : register(s1);

float globalTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(baseTexture, coords);
    float2 gradientCoords = baseColor.xy + float2(globalTime, 0);    
    float loopValue = frac(gradientCoords.x);
    gradientCoords.x = gradientCoords.x < 0 ? -loopValue : loopValue;
    
    return tex2D(gradientTexture, gradientCoords) * sampleColor * baseColor.a;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
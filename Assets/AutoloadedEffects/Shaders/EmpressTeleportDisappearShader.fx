sampler baseTexture : register(s0);
sampler erasureNoise : register(s1);

float cutoffY;
float globalTime;
bool invertDisappearanceDirection;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(baseTexture, coords);
    float erasureNoiseOffset = tex2D(erasureNoise, coords * 0.85 + float2(0, globalTime * 0.75)) * 0.2;
    float erasureCutoff = smoothstep(cutoffY, cutoffY + 0.03, coords.y + erasureNoiseOffset);
    
    // Invert the disappearance direction if necessary.
    // This is simply an optimization for doing a 'erasureCutoff = 1 - erasureCutoff' calculation if invertDisappearanceDirection is true.
    erasureCutoff = abs(invertDisappearanceDirection - erasureCutoff);
    
    return baseColor * erasureCutoff;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
sampler baseTexture : register(s0);
sampler erasureNoise : register(s1);

bool invertDisappearanceDirection;
float cutoffY;
float globalTime;
float blurOffset;
float blurWeights[5];

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = 0;
    for (int i = -2; i < 3; i++)
    {
        for (int j = -2; j < 3; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            baseColor += tex2D(baseTexture, coords + float2(i, j) * blurOffset) * weight;
        }
    }
    baseColor *= sampleColor;

    float erasureNoiseOffset = tex2D(erasureNoise, coords * 0.85 + float2(0, globalTime * 0.75)) * 0.2;
    float erasureCutoff = smoothstep(cutoffY, cutoffY + 0.06, coords.y + erasureNoiseOffset);
    
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
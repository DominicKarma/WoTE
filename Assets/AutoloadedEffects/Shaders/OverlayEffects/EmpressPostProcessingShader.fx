sampler baseTexture : register(s0);
sampler erasureNoise : register(s1);

bool invertDisappearanceDirection;
float cutoffY;
float globalTime;
float directionBlurInterpolant;
float defocusBlurOffset;
float defocusBlurWeights[5];
float2 directionalBlurOffset;
float directionalBlurWeights[18];

float4 CalculateBlurColor(float2 coords)
{
    float4 defocusBlurColor = 0;
    for (int i = -2; i < 3; i++)
    {
        for (int j = -2; j < 3; j++)
        {
            float weight = defocusBlurWeights[abs(i) + abs(j)];
            defocusBlurColor += tex2D(baseTexture, coords + float2(i, j) * defocusBlurOffset) * weight;
        }
    }
    
    float4 directionalBlurColor = 0;
    for (int j = 0; j < 18; j++)
    {
        float blurWeight = directionalBlurWeights[j] * 0.5;
        directionalBlurColor += tex2D(baseTexture, coords - j * directionalBlurOffset) * blurWeight;
        directionalBlurColor += tex2D(baseTexture, coords + j * directionalBlurOffset) * blurWeight;
    }
    
    return lerp(defocusBlurColor, directionalBlurColor, directionBlurInterpolant);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float erasureNoiseOffset = tex2D(erasureNoise, coords * 0.85 + float2(0, globalTime * 0.75)) * 0.2;
    float erasureCutoff = smoothstep(cutoffY, cutoffY + 0.06, coords.y + erasureNoiseOffset);
    
    // Invert the disappearance direction if necessary.
    // This is simply an optimization for doing a 'erasureCutoff = 1 - erasureCutoff' calculation if invertDisappearanceDirection is true.
    erasureCutoff = abs(invertDisappearanceDirection - erasureCutoff);
    
    return CalculateBlurColor(coords) * sampleColor * erasureCutoff;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
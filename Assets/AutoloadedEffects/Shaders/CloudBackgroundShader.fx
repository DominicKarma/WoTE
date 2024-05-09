sampler baseTexture : register(s0);
sampler uvAlteringTexture : register(s1);

float globalTime;
float2 baseTextureSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelationFactor = 1.1 / baseTextureSize;
    coords = round(coords / pixelationFactor) * pixelationFactor;
    
    float rollOffset = tex2D(uvAlteringTexture, coords + float2(0.031, 0.02) * globalTime) * 0.05;
    float2 coordsOffset = float2(globalTime * 0.009, 0) + rollOffset;
    float4 baseColor = tex2D(baseTexture, coords * float2(2.4, 1) + coordsOffset) * sampleColor;
    
    float distanceFromMoon = distance((coords - 0.5) * float2(1, 0.4) + 0.5, float2(0.5, 0.45));
    float opacity = pow(coords.y, 1.6) * smoothstep(1, 0.7, coords.y + baseColor.b * 0.4) * smoothstep(0.07, 0.17, distanceFromMoon);
    
    return baseColor * opacity + float4(0.4, 0.5, 0.6, 0) * opacity;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
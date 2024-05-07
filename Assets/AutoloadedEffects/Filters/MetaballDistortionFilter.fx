sampler screenTexture : register(s0);
sampler heatMetaballsTexture : register(s2);

float globalTime;
float opacity;
float2 screenZoom;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 headDistortionData = tex2D(heatMetaballsTexture, (coords - 0.5) / screenZoom + 0.5);
    float heatDistortionAngle = headDistortionData.g * 6.283;
    float2 heatDistortionDirection = float2(cos(heatDistortionAngle), sin(heatDistortionAngle));
    
    return tex2D(screenTexture, coords + heatDistortionAngle * headDistortionData.a * 0.0051);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

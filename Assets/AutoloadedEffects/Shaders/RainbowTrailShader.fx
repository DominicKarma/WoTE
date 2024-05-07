sampler rainbowTexture : register(s1);
sampler noiseTexture : register(s2);

float localTime;
float hueOffset;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (input.TextureCoordinates.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    // Calculate the base glow. This will dictate the colors of the rainbow's tip.
    float tipNoise = tex2D(noiseTexture, coords + float2(localTime * 1.5, 0)) - 0.5;
    float glow = smoothstep(0.3, 0.175, coords.x + QuadraticBump(coords.y) * 0.1 + tipNoise * 0.2);
    
    // Apply inner glow, adding the bright trail behind the tip.
    // This is influenced by noise and becomes weaker near the end of the rainbow.
    float horizontalEdgeDistance = distance(coords.y, 0.5);    
    float innerGlowInterpolant = saturate(tex2D(noiseTexture, coords + float2(localTime * 2.3, 0.48)));
    float innerGlow = lerp(0.12, 0.9, innerGlowInterpolant) * (1 - coords.x);    
    glow += saturate(pow(innerGlow / (horizontalEdgeDistance - tipNoise * 0.3), 3.5)) * (1 - coords.x) * 2;
    
    // Calculate a hue and opacity value, combining everything together.
    float hue = tex2D(noiseTexture, coords * 0.6 + float2(localTime * 2.5, 0)) * 0.2 + glow * 0.2 + coords.x * 0.08 + hueOffset;
    float opacity = smoothstep(0.5, 0.4, horizontalEdgeDistance);
    return tex2D(rainbowTexture, float2(hue, 0)) * glow * opacity + pow(glow, 4) * opacity * 0.4;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

sampler noiseTexture : register(s1);
sampler streakTexture : register(s2);

float fireColorInterpolant;
float globalTime;
float brightness;
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

float4 CalculatePetalColor(float2 coords, float4 baseColor)
{
    float horizontalEdgeDistance = distance(coords.y, 0.5);
    float edgeFade = smoothstep(0.5, 0, horizontalEdgeDistance);
    float glow = saturate(edgeFade / horizontalEdgeDistance * 0.5);
    return float4(baseColor.rgb, 0) * glow + (1 - coords.x) * edgeFade * 1.3;
}

float4 CalculateFireColor(float2 coords)
{
    float fireNoise = tex2D(noiseTexture, coords * 2 + float2(globalTime * -3.95, 0));
    float horizontalEdgeDistance = distance(coords.y, 0.5);
    float edgeFade = smoothstep(0.5, 0.2, horizontalEdgeDistance);
    
    // Calculate the glow intensity.
    // This depends on the noise value above, and is capped to allow for colors that aren't just pure white in the center.
    // It also depends on the length along the petal. Colors that are further out become more translucent.
    float glow = clamp(edgeFade / horizontalEdgeDistance * fireNoise * 0.4, 0, 5) * (1 - coords.x);
    
    float4 fireColor = lerp(float4(0.4, 1.05, 1.95, 1), float4(0.94, 0.85, 0.45, 1), fireNoise) * glow;
    
    // Make the fire draw additively in the center, to ensure that layering problems do not appear where they intersect on the Empress.
    fireColor.a *= smoothstep(0.2, 0.4, coords.x);
    
    // Apply darkening effects to the fire, for contrast purposes.
    fireColor -= tex2D(streakTexture, coords + float2(globalTime * -4.15, 0)) * fireColor.a / glow * 1.15;
    
    return fireColor * brightness * 2;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (input.TextureCoordinates.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    return lerp(CalculatePetalColor(coords, input.Color), CalculateFireColor(coords), fireColorInterpolant) * input.Color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

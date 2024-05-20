sampler noiseTexture : register(s1);

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
    
    float colorNoiseUVOffset = tex2D(noiseTexture, coords * float2(6, 1.1) + float2(localTime * 1.3, 0.15));
    float colorNoise = tex2D(noiseTexture, coords * float2(5, 1) + float2(localTime * 3.1, 0) + colorNoiseUVOffset * 0.15) - colorNoiseUVOffset * 0.9;
    
    float glow = pow((1 - coords.x) * 0.3 / distance(coords.y, 0.5), 3);
    float4 trailInterpolationColor = lerp(float4(0.35, 0.29, 0.74, 1), input.Color, smoothstep(0.65, 0.05, coords.x));
    float4 trailColor = lerp(trailInterpolationColor, 1, colorNoise);
    
    return trailColor * glow;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

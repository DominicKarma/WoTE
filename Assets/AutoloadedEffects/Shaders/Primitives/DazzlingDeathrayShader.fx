sampler streakNoiseTexture : register(s1);

float localTime;
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
    
    float horizontalCenterDistance = distance(coords.y, 0.5);
    
    float pulse = sin(localTime * 54) * 0.5 + 0.5;
    float scaleFactor = lerp(1, 2, pulse);
    
    float glow = saturate(pow(QuadraticBump(coords.y), 2)) + smoothstep(0.5, 0, horizontalCenterDistance * scaleFactor) / horizontalCenterDistance * 0.2;
    float4 baseColor = float4(input.Color.rgb, 1) * glow;
    float4 color = baseColor;
    color.a *= smoothstep(0.03, 0.4, coords.x);
    
    return saturate(color) * smoothstep(0.98, 0.93, coords.x) * input.Color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

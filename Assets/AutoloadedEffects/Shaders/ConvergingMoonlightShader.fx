sampler streakTexture : register(s1);

float globalTime;
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
    
    float offset = 0.025;
    float2 baseCoords = coords * 0.5 + float2(globalTime * 0.4, 0);
    float4 left = tex2D(streakTexture, baseCoords + float2(-offset, 0));
    float4 right = tex2D(streakTexture, baseCoords + float2(offset, 0));
    float4 top = tex2D(streakTexture, baseCoords + float2(0, -offset));
    float4 bottom = tex2D(streakTexture, baseCoords + float2(0, offset));
    float4 center = tex2D(streakTexture, baseCoords);
    
    float streak = pow((left + right + top + bottom + center) * 0.2, 2) / tex2D(streakTexture, baseCoords * 0.6);
    
    return input.Color * saturate(streak) * pow(QuadraticBump(coords.y), 2);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

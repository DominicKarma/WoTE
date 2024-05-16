sampler baseTexture : register(s1);

float globalTime;
float blurOffset;
float blurWeights[11];
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

float4 CalculateBlurColor(float2 coords)
{
    float4 baseColor = 0;
    for (int i = -5; i < 6; i++)
    {
        for (int j = -5; j < 6; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            float2 blurCoords = coords + float2(i, j) * blurOffset;
            baseColor += tex2D(baseTexture, blurCoords) * weight;
        }
    }
    return baseColor;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (input.TextureCoordinates.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    return CalculateBlurColor(coords) * input.Color;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
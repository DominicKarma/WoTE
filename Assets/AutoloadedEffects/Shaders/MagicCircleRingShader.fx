sampler ringTexture : register(s1);

float globalTime;
float spinScrollOffset;
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
    output.TextureCoordinates = input.TextureCoordinates;
    output.Position = pos;
    output.Color = input.Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Calculate the base coordinates.
    float glow = 1 - (smoothstep(0, 0.15, input.TextureCoordinates.y) * smoothstep(1, 0.85, input.TextureCoordinates.y));
    glow *= pow(sin(input.TextureCoordinates.y * 3.141), 0.3) * 2;
    
    return saturate(input.Color) * (tex2D(ringTexture, input.TextureCoordinates.xy * float2(-1, 1) + float2(-spinScrollOffset, 0)) + glow * 2);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

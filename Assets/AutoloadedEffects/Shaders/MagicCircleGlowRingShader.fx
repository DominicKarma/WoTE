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
    float baseNoise = tex2D(ringTexture, input.TextureCoordinates.xy * float2(1.2, 0) + float2(globalTime * 0.15, 0.5));
    float glow = 0.2 + pow(baseNoise, 5) * 1.1;
    float angleCosine = input.TextureCoordinates.z;
    float verticalGlow = smoothstep(1, 0.9, input.TextureCoordinates.y) * input.TextureCoordinates.y;
    return saturate(abs(angleCosine) * glow) * input.Color * verticalGlow;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

sampler noiseTextureA : register(s1);
sampler noiseTextureB : register(s2);
sampler gradientMapTextureA : register(s3);
sampler gradientMapTextureB : register(s4);

float globalTime;
float localTime;
float opacity;
float horizontalStack;
float swirlDirection;
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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Calculate the base coordinates.
    float x = frac(input.TextureCoordinates.x + (input.TextureCoordinates.y * horizontalStack + localTime * 2) * swirlDirection);    
    float horizontalAngle = acos(saturate(x) * 2 - 1);
    float2 coords = float2(horizontalAngle / 3.141, input.TextureCoordinates.y);
    
    // Calculate vertical fade-out values.
    float noise = saturate(tex2D(noiseTextureA, coords * 0.9 + float2(0, globalTime * 0.6)) + tex2D(noiseTextureB, coords * 1.1 + float2(0, globalTime * 0.75)) * 0.3);
    float verticalEdgeFadeBias = smoothstep(0.3, 0, coords.y) * 0.8 + smoothstep(0.87, 1, coords.y + noise * 0.5) * 1.4;
    float verticalEdgeFadeOut = smoothstep(noise * 0.18, 0.22, noise * 0.7 - verticalEdgeFadeBias);
    
    // Use noise to calculate the color of the tornado, sampling from two gradient map textures.
    float4 gradient = tex2D(gradientMapTextureA, noise * 2.2) * 0.5 + tex2D(gradientMapTextureB, noise * 2 + localTime * 0.1 + 0.45) * 0.85;
    float4 color = saturate(gradient) * verticalEdgeFadeOut * input.Color * 1.13;
    
    // Make colors fade at the edges of the tornado.
    // This uses a bit of noise when discerning the edge, to make the overall shape a bit more natural.
    float horizontalEdgeCosine = input.TextureCoordinates.z;
    float edgeFade = 1 - smoothstep(0.92, 1, abs(input.TextureCoordinates.z) + noise * 0.15);
    
    return color * edgeFade * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

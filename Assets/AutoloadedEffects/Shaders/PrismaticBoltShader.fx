sampler streakCullNoiseTexture : register(s1);
sampler trailTexture : register(s2);
sampler colorGradientTexture : register(s3);

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
    
    // Calculate the brightness of the trail texture value at the given pixel.
    float streakCullNoise = tex2D(streakCullNoiseTexture, coords + float2(localTime * -0.54, 0));
    float opacity = smoothstep(0.7, 0.5, coords.x + lerp(-0.1, 0.6, streakCullNoise));
    float trailTextureBrightness = tex2D(trailTexture, coords + float2(localTime * -1.4, 0));
    
    // Calculate the glow of the pixel, based on how far it is from the horizontal edge.
    float horizontalEdgeDistance = distance(coords.y, 0.5);
    float glow = smoothstep(0.5, 0.25, horizontalEdgeDistance + coords.x) * 0.3 / horizontalEdgeDistance;
    
    // Calculate colors.
    float trailHue = coords.x * 1.3 - localTime * 0.9;
    float4 trailColor = tex2D(colorGradientTexture, float2(trailHue, 0)) * opacity * trailTextureBrightness * lerp(1, 2, coords.x);
    float4 rainbowColor = tex2D(colorGradientTexture, coords + float2(trailTextureBrightness * 0.5 + coords.x - localTime, 0)) * glow;
    
    // Combine colors.
    return lerp(rainbowColor, trailColor, smoothstep(0, 0.5, coords.x));
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

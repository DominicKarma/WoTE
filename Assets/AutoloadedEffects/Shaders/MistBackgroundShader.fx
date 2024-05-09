sampler baseTexture : register(s0);
sampler uvAlteringTexture : register(s1);
sampler rainbowMapTexture : register(s2);

float globalTime;
float dewAppearanceCutoffThreshold;
float2 worldOffset;
float2 baseTextureSize;

float hash12(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 30000);
}

float CalculateDewBrightness(float2 coords)
{
    float brightness = hash12(coords);
    if (brightness >= dewAppearanceCutoffThreshold)
        brightness = pow((brightness - dewAppearanceCutoffThreshold) / (1 - dewAppearanceCutoffThreshold), 26);
    else
        brightness = 0;
    
    return brightness;
}

float4 CalculateMistColor(float2 coords, float4 sampleColor)
{
    // Pixelate coordinates.
    float2 baseCoords = coords;
    float2 standardPixelationFactor = 1.1 / baseTextureSize;
    coords = round(coords / standardPixelationFactor) * standardPixelationFactor;
    
    // Calculate an offset value that's applied to the base texture's UVs, to make it look like the mist is rolling over time.
    float rollOffset = tex2D(uvAlteringTexture, coords + float2(0.041, 0.03) * globalTime) * 0.05;
    
    // Determine the coordinate offset for the given pixel, taking into account the rolling offset an time, to make it look like the results are drifting.
    float2 coordsOffset = float2(globalTime * 0.019, 0) + rollOffset;
    
    // Sample the mist texture, making it weaker near the top, so that there's a smooth transition from sky to ground mist.
    float mistOpacity = smoothstep(0.8, 1.2, coords.y);
    return tex2D(baseTexture, coords * float2(1.5, 1) + coordsOffset) * sampleColor * mistOpacity;
}

float4 CalculateDewColor(float2 coords)
{
    // Pixelate dew coordinates.
    float2 dewCoords = coords * float2(baseTextureSize.x / baseTextureSize.y, 1);
    float2 dewPixelationFactor = float2(3, 1.5) / baseTextureSize;
    dewCoords = round(dewCoords / dewPixelationFactor) * dewPixelationFactor;
    
    // Swap between two dew brightnesses depending on the camera's position and time, making it look like the dew reflects differently over time.
    float dewSwapInterpolant = sin(globalTime * 1.3 + worldOffset.x * 50) * 0.5 + 0.5;
    float dewBrightness = lerp(CalculateDewBrightness(dewCoords), CalculateDewBrightness(dewCoords * 0.5 + 0.5), dewSwapInterpolant);
    
    // Calculate the color of the dew. This uses a premade texture, but is interpolated towards pure white for a bit of a nice tint effect.
    float4 dewHue = lerp(tex2D(rainbowMapTexture, coords * 5), 1, 0.4) * 2;
    
    // Combine the brightness and hue.
    return dewHue * dewBrightness;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords += worldOffset;
    
    float4 mistColor = CalculateMistColor(coords, sampleColor);
    float4 dewColor = CalculateDewColor(coords) * pow(mistColor.a / sampleColor.a, 2);
    return mistColor + dewColor;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
sampler gradientTexture : register(s0);
sampler erasureNoise : register(s1);

float globalTime;
float pulsationIntensity;
bool invertDisappearanceDirection;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the distance to the center of the ring. This is magnified a bit for intensity purposes in later equations.
    float2 coordsNormalizedToCenter = coords * 2 - 1;
    float distanceFromCenterSqr = distance(coords, 0.5) * 2;

    // Calculate coordinates relative to the sphere.
    // This pinch factor effectively ensures that the UVs are relative to a circle, rather than a rectangle.
    // This helps SIGNIFICANTLY for making the texturing look realistic, as it will appear to be traveling on a
    // sphere rather than on a sheet that happens to overlay a circle.
    float spherePinchFactor = (1 - sqrt(abs(1 - distanceFromCenterSqr))) / distanceFromCenterSqr + 0.001;
    
    // Exaggerate the pinch slightly.
    spherePinchFactor = pow(spherePinchFactor, 1.6);
    
    // Calculate the ring position.
    float2 sphereCoords = frac((coords - 0.5) * spherePinchFactor + 0.5 + float2(globalTime * 1.54, 0));
    
    // Calculate the hue interpolant and brightness.
    float pulsationTerm = abs(cos(spherePinchFactor * 15 + globalTime * -3 + sphereCoords.x * 10) * distance(sphereCoords.x, 0.5)) * pulsationIntensity * 0.9;
    float opacity = smoothstep(0.074, 0, distance(sphereCoords.y, 0.5) + smoothstep(0.5, invertDisappearanceDirection ? 0.7 : 0.3, coords.y));
    float edgeCutoff = smoothstep(0.5, 0.48, distance(coords, 0.5));
    float hueInterpolant = sin(tex2D(erasureNoise, sphereCoords) * 3.141 + globalTime * 4) * 0.5 + 0.5;
    float4 baseColor = tex2D(gradientTexture, float2(hueInterpolant, 0.5));
    
    return sampleColor * (baseColor * opacity + smoothstep(0.6, 1, opacity * baseColor.a) * 0.75) * edgeCutoff;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
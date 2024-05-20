sampler uvOffsetNoiseTexture : register(s0);

float globalTime;
float explosionDistance;
float shockwaveOpacityFactor;
float2 screenSize;
float2 projectilePosition;
float3 shockwaveColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = 0;
    
    float uvOffsetAngle = tex2D(uvOffsetNoiseTexture, coords + globalTime * 0.25) * 8;
    float2 uvOffset = float2(cos(uvOffsetAngle), sin(uvOffsetAngle)) * 0.007;
    
    // Calculate the distance of the UV coordinate from the explosion center.
    // This has a small offset based on noise so that the shape of the overall explosion isn't a perfect circle.
    float offsetFromProjectile = length((coords + uvOffset) * screenSize - projectilePosition);
    
    // Calculate how close the distance is to the explosion line.
    float signedDistanceFromExplosion = (offsetFromProjectile - explosionDistance) / screenSize.x;
    float distanceFromExplosion = abs(signedDistanceFromExplosion);
    
    // Make the shockwave's intensity dissipate at the outer edges.
    distanceFromExplosion += smoothstep(0.01, 0.09, signedDistanceFromExplosion);
    
    // Make colors very bright near the explosion line.
    color += float4(shockwaveColor, 1) * shockwaveOpacityFactor / distanceFromExplosion * 0.041;
    
    return color * sampleColor;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
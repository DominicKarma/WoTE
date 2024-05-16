sampler baseTexture : register(s0);

float polygonSides;
float appearanceInterpolant;
float globalTime;
float offsetAngle;
float sectionStartOffsetAngle;
float scale;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    float polarCoefficient = cos(3.141 / polygonSides) / cos(2 / polygonSides * asin(cos(polygonSides / 2 * (angle + offsetAngle))));
    float2 polygonEdge = float2(cos(angle), sin(angle)) * polarCoefficient;
    float2 normalizedPolygonEdge = polygonEdge * 0.49 + 0.5;
    
    float normalizedAngle = frac((angle + sectionStartOffsetAngle + 3.141) / 6.283);
    float sectionInterpolant = frac(normalizedAngle * polygonSides);
    float opacity = smoothstep(sectionInterpolant * 0.99, sectionInterpolant, appearanceInterpolant);
    
    float bloomGlowInterpolant = smoothstep(0.019, 0, distance(coords, normalizedPolygonEdge)) * 0.3;
    float innerGlowInterpolant = smoothstep(0.005, 0, distance(coords, normalizedPolygonEdge));
    float glowInterpolant = bloomGlowInterpolant + innerGlowInterpolant;
    
    return (glowInterpolant / scale) * sampleColor * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
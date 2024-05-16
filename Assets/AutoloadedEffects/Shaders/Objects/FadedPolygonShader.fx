sampler baseTexture : register(s0);

float polygonSides;
float appearanceInterpolant;
float globalTime;
float offsetAngle;
float sectionStartOffsetAngle;
float scale;

float2 CalculateNormalizedPolygonEdge(float angle)
{
    // Calculate the position of the edge of the polygon.
    // This follows for polar coordinates, hence the <cos(theta), sin(theta)> base vector.
    // The coefficient in this context is used to convert the angularly defined circle into an inscribed polygon.
    float polarCoefficient = cos(3.141 / polygonSides) / cos(2 / polygonSides * asin(cos(polygonSides * (angle + offsetAngle) * 0.5)));
    float2 polygonEdge = float2(cos(angle), sin(angle)) * polarCoefficient;
    
    // Alter the range of the resulting outputs from the range of -1 to -1 to the ranage of 0 to 1.
    return polygonEdge * 0.5 + 0.5;
}

bool AngleHasBeenTracedOut(float angle)
{
    // Subdivide the angle evenly, such that a given "section" fits into one of the lines that compose the resulting shape.
    // Each "section" along the polygon will trace out at an equivalent pace, in accordance with the central appearanceInterpolant variable.
    float normalizedAngle = frac((angle + sectionStartOffsetAngle + 3.141) / 6.283);
    float sectionInterpolant = frac(normalizedAngle * polygonSides);
    return appearanceInterpolant >= sectionInterpolant;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    float2 polygonEdge = CalculateNormalizedPolygonEdge(angle);
    
    // Calculate glow variables based on the pixel's proximity to the pixel's edge.
    // The closer a pixel is to the edge, the brighter it is, generally.
    // More distant (but still close) pixels gain a little bit of a glow, but not as much. This provides a basic bloom effect.
    float bloomGlowInterpolant = smoothstep(0.019, 0, distance(coords, polygonEdge)) * 0.3;
    float innerGlowInterpolant = smoothstep(0.005, 0, distance(coords, polygonEdge));
    float glowInterpolant = bloomGlowInterpolant + innerGlowInterpolant;
    
    return AngleHasBeenTracedOut(angle) * glowInterpolant / scale * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
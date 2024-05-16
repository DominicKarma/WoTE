sampler baseTexture : register(s0);

float polygonSides;
float appearanceInterpolant;
float globalTime;
float offsetAngle;
float scale;

float2 StarPolarEquation(float pointCount, float angle)
{
    float spacedAngle = angle;
    
    // There should be a star point that looks directly upward. However, that isn't the case for odd star counts with the equation below.
    // To address this, a -90 degree rotation is performed.
    if (pointCount % 2 != 0)
        spacedAngle -= 1.5707;

    // Refer to desmos to view the resulting shape this creates. It's basically a black box of trig otherwise.
    float sqrt3 = 1.732051;
    float numerator = cos(3.141 * (pointCount + 1) / pointCount);
    float starAdjustedAngle = asin(cos(pointCount * spacedAngle)) * 2;
    float denominator = cos((starAdjustedAngle + 1.5707 * pointCount) / (pointCount * 2));
    float2 result = float2(cos(angle), sin(angle)) * numerator / denominator / sqrt3;
    return result;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    float polarCoefficient = cos(3.141 / polygonSides) / cos(2 / polygonSides * asin(cos(polygonSides / 2 * (angle + offsetAngle))));
    float2 polygonEdge = float2(cos(angle), sin(angle)) * polarCoefficient;
    float2 normalizedPolygonEdge = polygonEdge * 0.49 + 0.5;
    
    float sectionOffsetAngle = distance(polygonSides % 2, 1) <= 0.01 ? 0 : offsetAngle;
    float normalizedAngle = frac((angle + sectionOffsetAngle + 3.141) / 6.283);
    float sectionInterpolant = frac(normalizedAngle * polygonSides);
    float opacity = smoothstep(sectionInterpolant * 0.75, sectionInterpolant, appearanceInterpolant);
    return (distance(coords, normalizedPolygonEdge) <= 0.0025 / scale) * sampleColor * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
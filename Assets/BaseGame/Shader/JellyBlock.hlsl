#ifndef JELLY_BLOCK_INCLUDED
#define JELLY_BLOCK_INCLUDED

// Cartoon jelly / candy look for a rounded cube (match-3 style).
// Full opaque shading — no SDF cutouts, so mesh seams stay closed.

void JellyBlock_float(
    float3 PositionOS,
    float3 NormalWS,
    float3 ViewDirWS,
    float4 BaseColor,
    float4 DarkColor,
    float HighlightIntensity,
    float Glossiness,
    float RimStrength,
    float HeightScale,
    out float3 Color,
    out float Alpha)
{
    float3 N = normalize(NormalWS);
    float3 V = normalize(ViewDirWS);
    float ndotv = saturate(dot(N, V));

    // --- Vertical candy gradient (darker bottom, brighter top) ---
    float height = saturate(PositionOS.y / max(HeightScale, 1e-4) * 0.5 + 0.5);
    float3 col = lerp(DarkColor.rgb, BaseColor.rgb, height * 0.5 + 0.5);

    // Soft center lift (juicy body)
    float centerLift = pow(ndotv, 1.4);
    col = lerp(col * 0.92, col * 1.06, centerLift);

    // Soft outer bevel — darker near silhouette
    float rimDark = pow(1.0 - ndotv, 2.0);
    col = lerp(col, DarkColor.rgb * 0.75, rimDark * RimStrength);

    // Lighter top rim (match reference glow on upper edge)
    float topRim = saturate(N.y * 0.8 + 0.2) * pow(1.0 - ndotv, 2.5);
    col += BaseColor.rgb * topRim * 0.35;

    // --- Cartoon glossy blob (upper-left), like the reference highlight ---
    // Build a view-aligned normal so the highlight sticks to the facing surface.
    float3 up = float3(0.0, 1.0, 0.0);
    float3 right = normalize(cross(up, V));
    up = normalize(cross(V, right));
    float2 nFace = float2(dot(N, right), dot(N, up));

    float2 hlCenter = float2(-0.28, 0.42);
    float2 hlSize = float2(0.32, 0.48);
    float2 hlDelta = (nFace - hlCenter) / hlSize;
    float blob = saturate(1.0 - length(hlDelta));
    blob = pow(blob, lerp(1.2, 2.4, saturate(Glossiness)));

    // Secondary soft sheen under the main glint
    float2 sheenCenter = float2(-0.12, 0.25);
    float2 sheenDelta = (nFace - sheenCenter) / float2(0.55, 0.7);
    float sheen = saturate(1.0 - length(sheenDelta));
    sheen = pow(sheen, 2.5) * 0.35;

    float3 highlightCol = float3(1.0, 0.97, 0.97);
    col = lerp(col, highlightCol, blob * saturate(HighlightIntensity));
    col += highlightCol * sheen * HighlightIntensity * 0.45;

    // Tiny bright core in the glint
    float core = pow(blob, 4.0) * HighlightIntensity * 0.55;
    col += highlightCol * core;

    Color = saturate(col);
    Alpha = 1.0;
}

#endif

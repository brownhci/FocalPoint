#ifndef LIGHTWEIGHT_SIMPLE_LIT_META_PASS_INCLUDED
#define LIGHTWEIGHT_SIMPLE_LIT_META_PASS_INCLUDED

#include "MetaInput.hlsl"

Varyings LightweightVertexMeta(Attributes input)
{
    Varyings output;

    output.positionCS = MetaVertexPosition(input.positionOS, input.uvLM, input.uvDLM, unity_LightmapST);
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);



    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.normalWS   = TransformObjectToWorldNormal(input.normalOS);


    ADVANCED_DISSOLVE_INIT_DATA(TransformWorldToHClip(output.positionWS), input.uv.xy, input.uvLM.xy, output)

    #if defined(_DISSOLVEMASK_XYZ_AXIS) || defined(_DISSOLVEMASK_PLANE) || defined(_DISSOLVEMASK_SPHERE) || defined(_DISSOLVEMASK_BOX) || defined(_DISSOLVEMASK_CYLINDER) || defined(_DISSOLVEMASK_CONE)
        output.positionWS += _Dissolve_ObjectWorldPos;
    #endif

    return output;
}

half4 LightweightFragmentMetaSimple(Varyings input) : SV_Target
{

float4 dissolaveAlpha = AdvancedDissolveGetAlpha(input.uv.xy, input.positionWS.xyz, input.normalWS, input.dissolveUV);

float3 dissolveAlbedo = 0;
float3 dissolveEmission = 0;
float dissolveBlend = DoDissolveAlbedoEmission(dissolaveAlpha, dissolveAlbedo, dissolveEmission, input.uv.xy, 1);


    float2 uv = input.uv;
    MetaInput metaInput;
    metaInput.Albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb;
    metaInput.SpecularColor = SampleSpecularSmoothness(uv, 1.0h, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap)).xyz;
    metaInput.Emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));


    metaInput.Albedo = lerp(metaInput.Albedo, dissolveAlbedo, dissolveBlend);
	metaInput.Emission = lerp(metaInput.Emission, dissolveEmission, dissolveBlend);
    

    return MetaFragment(metaInput);
}

#endif

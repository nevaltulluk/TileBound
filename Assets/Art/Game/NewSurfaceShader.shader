Shader "Custom/Water_WavesAndFoam_BIRP"
{
    Properties
    {
        _BaseColor ("Base Color (RGBA = color+alpha)", Color) = (0.10, 0.55, 0.75, 0.65)

        // Vertex waves (actual mesh wobble)
        _WaveHeight1  ("Wave Height 1 (m)", Float) = 0.08
        _WaveFreq1    ("Wave Freq 1 (1/m)", Float) = 1.2
        _WaveSpeed1   ("Wave Speed 1", Float) = 1.0
        _WaveHeight2  ("Wave Height 2 (m)", Float) = 0.05
        _WaveFreq2    ("Wave Freq 2 (1/m)", Float) = 1.9
        _WaveSpeed2   ("Wave Speed 2", Float) = 1.6
        _Chop         ("Horizontal Chop (push UVs)", Float) = 0.02

        // Optional color wobble (tiny shimmer)
        _ColorWobble  ("Color Wobble Amount", Float) = 0.06

        // Foam
        _FoamWidth    ("Foam Width (eye m)", Float) = 0.08
        _FoamIntensity("Foam Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 250
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Unlit alpha:fade vertex:vert
        #pragma target 3.0

        #include "UnityCG.cginc"

        fixed4 _BaseColor;

        float _WaveHeight1, _WaveFreq1, _WaveSpeed1;
        float _WaveHeight2, _WaveFreq2, _WaveSpeed2;
        float _Chop, _ColorWobble;

        float _FoamWidth, _FoamIntensity;

        sampler2D _CameraDepthTexture;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;   // for depth sampling
            float3 worldPos;
        };

        // Unlit lighting function
        inline half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(s.Albedo, s.Alpha);
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // Object to world for direction; we assume water is roughly XZ plane with Y up
            float3 pos = v.vertex.xyz;
            float t = _Time.y;

            // Two Gerstner-ish simple sines combined (lightweight)
            float w1 = sin(pos.x * _WaveFreq1 + t * _WaveSpeed1) * _WaveHeight1;
            float w2 = cos(pos.z * _WaveFreq2 + t * _WaveSpeed2) * _WaveHeight2;

            // Vertical displacement (visible wobble)
            pos.y += (w1 + w2);

            // Tiny horizontal chop so it’s not purely vertical
            pos.x += _Chop * sin(pos.z * (_WaveFreq1*0.7) + t * (_WaveSpeed1*1.3));
            pos.z += _Chop * cos(pos.x * (_WaveFreq2*0.8) + t * (_WaveSpeed2*1.1));

            v.vertex.xyz = pos;

            // Needed for depth sampling
            o.screenPos = UnityObjectToClipPos(v.vertex);
            o.worldPos  = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1)).xyz;
            o.uv_MainTex = v.texcoord.xy;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float4 col = _BaseColor;

            // Subtle color shimmer so it feels alive even without textures
            float t = _Time.y;
            float shimmer = (sin((IN.worldPos.x + IN.worldPos.z)*0.4 + t*2.5) * 0.5 + 0.5) * _ColorWobble;
            col.rgb = saturate(col.rgb + shimmer);

            // -------- Depth-fade foam (camera-relative, axis-independent) --------
            // Sample scene depth at this pixel
            float rawSceneDepth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));

            // Convert both to linear eye space
            float sceneEye   = LinearEyeDepth(rawSceneDepth);
            float surfaceNDC = IN.screenPos.z / IN.screenPos.w;
            float surfaceEye = LinearEyeDepth(surfaceNDC);

            // Difference (other geometry in front of water => small positive)
            float diff = (sceneEye - surfaceEye);

            // Map difference to 0..1 across _FoamWidth meters in eye space
            float foam = saturate(1.0 - saturate(diff / max(_FoamWidth, 1e-5)));
            foam = smoothstep(0.0, 1.0, foam) * _FoamIntensity;

            // Mix to white at contact
            float3 rgb = lerp(col.rgb, 1.0.xxx, foam);

            o.Albedo = rgb;
            o.Emission = 0;
            o.Alpha = col.a;
        }
        ENDCG
    }
    FallBack Off
}

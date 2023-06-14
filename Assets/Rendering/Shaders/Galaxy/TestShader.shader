Shader "TestShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        // Use this to control the speed of the animation.
        _Speed ("Hue Rotation Speed", float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        // Expose the speed parameter to use in our code.
        half _Speed;

        // Colour conversion functions.
        float3 hsv_to_rgb(float h, float s, float v) {
            h = 6.0f * frac(h);

            float3 hue = saturate(float3(
                abs(h-3.0f) - 1.0f,
                2.0f - abs(h - 2.0f),
                2.0f - abs(h - 4.0f)
            ));

            return ((hue - 1.0f) * s + 1.0f) * v;
        }

        // These functions are from https://chilliant.com/rgb2hsv.html
        const float EPSILON = 1e-10;
 
        float3 rgb_to_hcv(in float3 rgb) {
            // Based on work by Sam Hocevar and Emil Persson
            float4 p = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0/3.0) : float4(rgb.gb, 0.0, -1.0/3.0);
            float4 q = (rgb.r < p.x) ? float4(p.xyw, rgb.r) : float4(rgb.r, p.yzx);
            float c = q.x - min(q.w, q.y);
            float h = abs((q.w - q.y) / (6 * c + EPSILON) + q.z);
            return float3(h, c, q.x);
        }

        float3 rgb_to_hsv(float3 rgb) {
            float3 hcv = rgb_to_hcv(rgb);
            float s = hcv.y / (hcv.z + EPSILON);
            return float3(hcv.x, s, hcv.z);
        }

        // Original surface function, with modifications...
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Read the colour from the texture, and apply material tint.
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            // Convert colour to HSV colour space.
            float3 hsv = rgb_to_hsv(c.rgb);      
           
            // Shift the hue proportional to time, and convert back.
            o.Albedo = hsv_to_rgb(hsv.x + _Time.x * _Speed, hsv.y, hsv.z);

            // The rest we leave as-is in the default shader.
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
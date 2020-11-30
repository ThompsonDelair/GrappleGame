
// Example provided by MinionsArt
Shader "Toon/2ToneGradientMask" {
    Properties {
        [Header(Basic Colors)]
        _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Mask("ColorMask (Red = Prim Green = Sec Blue = Ter)", 2D) = "white" {} // mask texture
        _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
        
        // Primary
        [Header(Primary Gradient)]
        _PrimaryTexture ("Primary Texture", 2D) = "white" {}
        _ColorPrim1("Primary Color Top", Color) = (0.5,0.5,0.5,1) // primary color, replaces the red masked area
        _ColorPrim2("Primary Color Bottom", Color) = (0.5,0.5,0.5,1) // primary color, replaces the red masked area
        [Toggle] _Emis1("Primary: Emissive?", Float) = 0 // sets the color as emissive if toggled
        _Value1("Primary: Gradient Placing", Range(-1,2)) = 0.5 // blend value with the original texture

        // Secondary
        [Header(Secondary Gradient)]
        _SecondaryTexture ("Secondary Texture", 2D) = "white" {}
        _ColorSec1("Secondary Color Top", Color) = (0.5,0.5,0.5,1) // secondary color, replaces green masked area
        _ColorSec2("Secondary Color  Bottom", Color) = (0.5,0.5,0.5,1) // secondary color, replaces green masked area
        [Toggle] _Emis2("Secondary: Emissive?", Float) = 0// sets the color as emissive if toggled
        _Value2("Secondary: Gradient Placing", Range(-1,2)) = 0.5// blend value with the original texture
        
        // Tertiary
        [Header(Tertiary Gradient)]
        _TertiaryTexture ("Tertiary Texture", 2D) = "white" {}
        _ColorTert1("Tertiary Color Top", Color) = (0.5,0.5,0.5,1)// tertiary color, replaces blue masked area
        _ColorTert2("Tertiary Color Bottom", Color) = (0.5,0.5,0.5,1)// tertiary color, replaces blue masked area
        [Toggle] _Emis3("Tertiary: Emissive?", Float) = 0// sets the color as emissive if toggled
        _Value3("Tertiary: Gradient Placing", Range(-1,2)) = 0.5// blend value with the original texture
       

        // Specular Parameters
        [Header(Specular Fields)]
        _Brightness("Outline Brightness", Range(1,3)) = 2//
        _OutlineZ("Outline Z", Range(0.01,0.25)) = 0.2//
        _Outline ("Outline width", Range (.002, 0.03)) = .005
    }
 
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
 
        CGPROGRAM
        #pragma surface surf ToonRamp
        
        sampler2D _Ramp;
        
        // custom lighting function that uses a texture ramp based
        // on angle between light direction and normal
        #pragma lighting ToonRamp exclude_path:prepass
        inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
            #ifndef USING_DIRECTIONAL_LIGHT
            lightDir = normalize(lightDir);
            #endif
        
            half d = dot (s.Normal, lightDir) * 0.5 + 0.5;
            half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
        
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
            c.a = 0;
            return c;
        }
        
        
        sampler2D _MainTex, _PrimaryTexture, _SecondaryTexture, _TertiaryTexture;
        sampler2D _Mask; // mask texture
        float4 _Color, _ColorPrim1, _ColorPrim2, _ColorSec1, _ColorSec2, _ColorTert1, _ColorTert2;// custom colors
        float _Emis1, _Emis2, _Emis3; // emission toggles
        float _Value1, _Value2, _Value3;// original texture blend values
        
        struct Input {
            float2 uv_MainTex : TEXCOORD0;
        };
        
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            half4 m = tex2D(_Mask, IN.uv_MainTex); // mask based on the uvs
            
            
            // Primary Gradient
            float3 PrimaryColor = tex2D(_PrimaryTexture, IN.uv_MainTex) * lerp(_ColorPrim2, _ColorPrim1, saturate(IN.uv_MainTex.y)) * m.r;

            // Secondary Gradient
            float3 SecondaryColor =  tex2D(_SecondaryTexture, IN.uv_MainTex) * lerp(_ColorSec2, _ColorSec1, saturate(IN.uv_MainTex.y)) * m.g;

            // Tertiary Gradient
            float3 TertiaryColor = tex2D(_TertiaryTexture, IN.uv_MainTex) * lerp(_ColorTert2, _ColorTert1, saturate(IN.uv_MainTex.y )) * m.b;
        
            float3 NonMasked = c.rgb * (1 - m.r - m.g - m.b); // the part of the model thats not affected by the colour customisation
        
            o.Albedo = NonMasked  + PrimaryColor + SecondaryColor + TertiaryColor; // all parts added together form the new look for the model
            o.Emission = PrimaryColor * _Emis1 + SecondaryColor * _Emis2 + TertiaryColor * _Emis3; // emissive only shows up when the toggles for their colours are toggled
            o.Alpha = c.a;
        }
        ENDCG
 
    }
 
    Fallback "Diffuse"
}

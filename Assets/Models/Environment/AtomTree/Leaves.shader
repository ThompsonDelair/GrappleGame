Shader "Custom/BlackTransparentChannel" {

     Properties {
         // Main properties
        _Color ("Color", Color) = (1,1,1,1)
        _TransparentColor ("Transparent Color", Color) = (1,1,1,1)
        _Threshold ("Threshhold", Float) = 0.1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        
        // Vertex Displacement
        _Amplitude ("Wave Size", Range(0, 1)) = 0.4
        _Frequency ("Wave Frequency", Range(1, 8)) = 2
        _AnimationSpeed ("Animation Speed", Range(0, 5)) = 1

        // UV Scrolling
        _ScrollXSpeed ("X Scroll Speed", Range(-5, 5)) = 0
        _ScrollYSpeed ("Y Scroll Speed", Range(-5, 5)) = 0
     }

     SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        
        CGPROGRAM
            #include "unityCG.cginc"
            // #pragma surface surf 

            // Base texture and color
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TransparentColor;
            half _Threshold;

            // Vertex Displacement Parameters
            float _Amplitude;
            float _Frequency;
            float _AnimationSpeed;

            // UV scrolling parameters
            fixed _ScrollXSpeed;
            fixed _ScrollYSpeed;

            struct Input {
                float2 uv_MainTex;
            };

            void vert (inout appdata_full data) {
                // The sin function requires something to increase if we want it to change. In this case, we use _Time.y
                // vetexYPos += sin(vertexXPos * _Frequency + (_Time.y * _AnimationSpeed)) * _Amplitude;
                float4 modifiedPos = data.vertex;
                modifiedPos.y += sin( (data.vertex.x * _Frequency) + (_Time * _AnimationSpeed) ) * _Amplitude;
                data.vertex = modifiedPos;
            }
    
            void surf (Input IN, inout SurfaceOutputStandard o) {

                // Calculate UV Scrolling effect
                fixed varX = _ScrollXSpeed * _Time;
                fixed varY = _ScrollYSpeed * _Time;
                fixed2 scrolled_UV = IN.uv_MainTex + fixed2(varX, varY);

                // Read color from the texture
                half4 tex = tex2D (_MainTex, scrolled_UV);
                
                // Output colour will be the texture color * the vertex colour
                half4 output_col = tex * _Color;
                
                //calculate the difference between the texture color and the transparent color
                //note: we use 'dot' instead of length(transparent_diff) as its faster, and
                //although it'll really give the length squared, its good enough for our purposes!
                half3 transparent_diff = output_col.xyz - _TransparentColor.xyz;
                half transparent_diff_squared = dot(transparent_diff,transparent_diff);
                
                //if colour is too close to the transparent one, discard it.
                //note: you could do cleverer things like fade out the alpha
                if(transparent_diff_squared < _Threshold)
                    discard;

                //output albedo and alpha just like a normal shader
                o.Emission = output_col.rgb;
                o.Alpha = output_col.a;
            }

            #pragma surface surf Standard fullforwardshadows vertex:vert Lambert alpha

        ENDCG
     } 
     FallBack "Diffuse"
 }

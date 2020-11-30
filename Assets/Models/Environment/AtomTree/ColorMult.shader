// 1. Shader is a keyword, saved in a String location.
Shader "Unlit/FirstUnlit" {

    // This is where properties THAT ARE EXPOSED are declared
    Properties {
        // _Color must be the same name of the declared fixed4 in CGPROGRAM
        _Color("Color", Color) = (0, 0, 0, 1)
        _MainTex("Main Texture", 2D) = "white" {}
    }

    // 2. Shaders can have a number of Subshaders.
    Subshader {

        // 3. Shaders are rendered in one or more Passes.
        Pass {
            Tags {
                "RenderType" = "Opaque"
                "Queue" = "Geometry"
            }

            // 4. This tells Unity where our HLSL starts and ends.
            CGPROGRAM
                #include "unityCG.cginc"

                // Lets set some properties. These are not exposed
                fixed4 _Color;
                sampler2D _MainTex;

                // 5. Store 3d model data into a custom struct called Appdata.
                struct appdata {
                    float4 vertex : POSITION; // POSITION is a pre-defined member in CGPROGRAM ???
                    float2 uv : TEXCOORD0;
                };

                // 6. The v2f structure must store the data being outputted from vertex shader to the fragment shader.
                struct v2f {
                    float4 position : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                // 7. The Vertex shader takes in Appdata and returns a v2f structure.
                v2f vert(appdata v) {
                    v2f o; // Create a new instance of v2f

                    o.position = UnityObjectToClipPos(v.vertex); // Set it's position and return.
                    o.uv = v.uv;
                    return o;
                }

                // 8. The Fragment shader takes in the v2f struct created by Vertex shader.
                fixed4 frag(v2f i) : SV_TARGET {
                    return tex2D(_MainTex, i.uv) * _Color;
                }

                #pragma vertex vert
                #pragma fragment frag

            ENDCG
        }

    }
}

Shader "Unlit/SimpleShader" {

    Properties {
        
        _Tint ( "Surface Color", Color ) = (1, 1, 1, 1)
        _HighTone ( "High Tone", Color) = (1, 1, 1, 1)
        _MidTone ( "Mid Tone", Color) = (1, 1, 1, 1)
        _LowTone ( "Low Tone", Color) = (1, 1, 1, 1)

        _MainTex ("Base (RGB)", 2D) = "white" {}
        _DiffFalloff ("Diffuse Falloff", Float) = 0.5

        _GradientTop ( "Gradient Top", Range(0.45, 1)) = 0.9
        _GradientBot ( "Gradient Contrast", Range(0.0, 0.35)) = 0.1

        _DiffPostSteps ( "Diffuse Post Steps" , Range(1,50) ) = 3
        _SpecPostSteps ( "Specular Post Steps" , Range(1,50) ) = 3
        _Gloss ( "Surface Diffuse", Range(3,30) ) = 1

        // _MainTex ("Texture", 2D) = "white" {}
    }

    // The Subshader contains the Vertex Shader and the Fragment Shader
    //      a) Vert Shader : A foreach loop that operates on all vertices of a mesh in clip space, collecting data.
    //      b ) Frag Shader : A foreach loop that operates on all fragments that overlap with a mesh in clip space, using the vertex shader's output as inputs.
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            // From here on, we're writing CG code. CG code is the standard library for Shaders in Unity.
            CGPROGRAM

            // #pragma is a pre-compiler step to define the name of the vertex and the fragment shader.
            //      in this case, for instance, we're telling the compiler our vertex shader will be called vert.
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            // Of course, unity specific functions are included using this given namespace.
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            // === appdata - Mesh data: vertex positions, vertex normals, UV's tangents, vertex colors. === 
            //      We use this struct to define what data we actually want from the mesh for use in Vert Shader.
            //      Each Vertex of the mesh will then have this data available to it.
            struct VertexInputs {
                float4 vertex : POSITION;
                // float4 colors : COLOR;
                float3 normal : NORMAL;
                // float4 tangent : TANGENT;

                // UV's are accessed through indices denoting their channel.
                //      UV Channels are used to define 2d coords on a 3d mesh. (Think UV unwrapping in Blender)
                //      It's good to store them in float2's, as they are x-y coords.
                float2 uv0 : TEXCOORD0;
            };

            // === v2f (vertex to fragment) - The output of the Vertex Shader, which then goes into the Fragment Shader === 
            //      Each field in the v2f is an INTERPOLATOR, whic passes the data through the frag shader.
            struct VertexOutput {
                float4 clipSpacePos : SV_POSITION; // This is the clipped position in the shader. Safer to call this vertex.

                // In the v2f struct, fields using TEXCOORD are Interpolators.
                //       They use the same terms as UV's in the Input Struct, but are NOT UV'S
                float2 uv0 : TEXCOORD0;
                float3 normal : TEXCOORD1; // We'll need to normalize this so it's length 1 once we're in the Frag shader
                float3 worldPos : TEXCOORD2;

                LIGHTING_COORDS(3, 4)
            };

            // // === Variables tied to the properties we've defined. ===
            // sampler2D _MainTex;
            // float4 _MainTex_ST;

            sampler2D _MainTex;
            float4 _Tint, _HighTone, _MidTone, _LowTone;
            float _Gloss, _DiffFalloff, _GradientTop , _GradientBot, _MiddlePoint ;
            int _DiffPostSteps, _SpecPostSteps;

            // This is the Vertex Shader.
            VertexOutput vert (VertexInputs v) {
                // 1. Create new vertex output struct
                VertexOutput o;

                // 2. Pass through the data from our inputs into our outputs.
                o.uv0 = v.uv0;
                o.normal = v.normal;
                o.clipSpacePos = UnityObjectToClipPos( v.vertex ); // With this function, Unity handles transformation from the local -> clip space
                o.worldPos = mul (unity_ObjectToWorld, v.vertex); // Transforms the vertex from local space to world space
                
                // 3. Send this vertex with it's data into the Frag Shader
                return o;
            }

            // Example Lerp function :  value = lerp (a, b, t)
            // Interpolates between value a & b with a rate of t
            float3 MyLerp( float3 a, float3 b, float t ) {

                return ( t * b ) + ( (1 - t) * a );

            }

            // Example inverse Lerp : t = invLerp (a, b, value)
            // Gives the percentage between a & b where v lies.
            float InvLerp( float3 a, float3 b, float value ) {
                return (value - a) / (b - a);
            }

            // Change a Gradient into a number of steps
            float4 Posterize(float steps, float value) {
                return floor( value * steps) / steps;
            }
            
            // === The Frag Shader's output can have a few precisions of Vectors that may be used: === 
            //      fixed : -1 -> 1
            //      half : -32000, -> 32000
            //      float : -lots -> lots (most accurate for desktop)

            // This is the Fragment Shader
            float4 frag (VertexOutput o) : SV_Target {
                
                // 1. Create new data using the Vertex output.
                float2 uv = o.uv0;
                float3 normal = normalize(o.normal); // We gots to interpolate this one by normalizing it

                float t = uv.y;

                t = Posterize(_DiffPostSteps, t);

                // return float4(blend, 1);

                // Lighting
                float3 lightDir =  _WorldSpaceLightPos0.xyz; 
                float3 lightColor = _LightColor0.rgb; 
                float3 gradientColor = lerp(_LowTone , _MidTone, InvLerp(_GradientBot, _GradientTop, saturate(o.uv0.y)));
                gradientColor += lerp(_MidTone , _HighTone, InvLerp(_GradientBot, _GradientTop, saturate(o.uv0.y)));
                // return float4 (gradientColor, 0);

                // Direct Diffuse Lighting
                float lightFalloff = max (0.2, lerp( 0.8, dot(lightDir, normal), _DiffFalloff) ); // Calculate how much of the object will be in light from a source
                lightFalloff = Posterize( _DiffPostSteps, lightFalloff); // Break up the falloff into several hard cut-offs
                float3 directDiffuseLight = lightColor * lightFalloff; // Apply color to the object based on the light.

                // Ambient Light
                float3 ambientLight = float3(0.5, 0.5, 0.5);

                // Direct Specular Light - camera vector bounces off normal vector, comparing against light direction vector.
                float3 cameraPos = _WorldSpaceCameraPos.xyz;
                float3 fragToCam = cameraPos - o.worldPos;
                float3 viewDir = normalize( fragToCam );

                float3 viewReflect = reflect(-viewDir, normal); // Bouncing the view direction off the normal of the surface mesh
                float3 rimReflect = -reflect(-viewDir, normal);

                float specularFalloff = max (0.2, dot ( viewReflect, lightDir )); 
                float rimFalloff = max (  0, dot ( rimReflect, lightDir ));

                specularFalloff = Posterize(_SpecPostSteps, specularFalloff);
                specularFalloff = pow (specularFalloff, _Gloss); // This is a tunable way to alter the amount of gloss on an object.
                
                float3 directSpecular = specularFalloff * lightColor;

                float3 rimSpecular = rimFalloff * lightColor;
                rimSpecular = Posterize(_SpecPostSteps, rimSpecular);
                rimSpecular = pow (rimSpecular, _Gloss);

                // Composite Light
                float3 surfaceColor = _Tint.rgb * tex2D(_MainTex, o.uv0) * gradientColor;
                float3 diffuseLight = ambientLight + directDiffuseLight;
                float3 finalSurfaceColor = diffuseLight * surfaceColor + directSpecular;

                // 2. Draw something at the position of the Fragment.
                return float4(finalSurfaceColor , 0);

            }
            ENDCG
        }
    }

    // === RANDOM TERM DICTIONARY ===
    // Diffuse - Smooth objects, where the direction you're viewing the object does not effect the lighting.
    // Specular - Reflections off the object, which are view-direction dependant 

    // === HANDY TIPS ===
    //      Dot Product
    //          Given two vectors, Dot product is a way to determine how similar they are.
    //          This is commonly used in Lighting, as it's a way to easily quantify how lit an area will be
    //          based on the dot product of the surface normal vector and the incoming light direction vector.

    //      World Space Camera Viewing Direction.
    //          1. Include the world-space as an interpolator in v2f                : float3 worldPos : TEXCOORD2;
    //          2. Get the world-space transform of the vector in vert shader       : o.worldPos = mul (unity_ObjectToWorld, v.vertex);
    //          3. Get the vector between the current Fragment & the Camera         : float3 fragToCam = cameraPos - o.worldPos;
    //          4. We can optionally normalize this, if depth doesn't matter        : float3 viewDir = normalize( fragToCam );

    //      Phong Specular Highlights. (Using World-Space Camera View Direction)
    //          1. Get the position of the camera                                   : float3 cameraPos = _WorldSpaceCameraPos.xyz;
    //          2. Get the vector of the currently rendered fragment to the camera  : float3 fragToCam = normalize (cameraPos - o.worldPos);
    //          3. Calculate the Reflection Vector of Camera -> Normal              : float3 viewReflect = reflect(-viewDir, o.normal);
    //          4. Compare it's similarity to the light direction to find falloff   : float specularFalloff = max (0.2, dot ( viewReflect, lightDir )); 
}   

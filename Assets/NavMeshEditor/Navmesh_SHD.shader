// Shader created with Shader Forge v1.22 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.22;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:6,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:False,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:3138,x:33209,y:32672,varname:node_3138,prsc:2|emission-1580-OUT,alpha-7056-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:31960,y:32526,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_TexCoord,id:8594,x:32061,y:32982,varname:node_8594,prsc:2,uv:0;n:type:ShaderForge.SFN_Step,id:1998,x:32248,y:32907,varname:node_1998,prsc:2|A-5041-OUT,B-8594-V;n:type:ShaderForge.SFN_Slider,id:5041,x:31904,y:32856,ptovrint:False,ptlb:OutlineThickness,ptin:_OutlineThickness,varname:node_5041,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7777778,max:1;n:type:ShaderForge.SFN_Lerp,id:5073,x:32473,y:32663,varname:node_5073,prsc:2|A-7241-RGB,B-1193-OUT,T-1998-OUT;n:type:ShaderForge.SFN_Vector3,id:2383,x:32114,y:32239,varname:node_2383,prsc:2,v1:0,v2:1,v3:0;n:type:ShaderForge.SFN_Dot,id:8629,x:32317,y:32268,varname:node_8629,prsc:2,dt:1|A-2383-OUT,B-2017-OUT;n:type:ShaderForge.SFN_NormalVector,id:2017,x:32145,y:32343,prsc:2,pt:False;n:type:ShaderForge.SFN_Slider,id:1159,x:31957,y:32110,ptovrint:False,ptlb:SlopeShading,ptin:_SlopeShading,varname:node_1159,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:10;n:type:ShaderForge.SFN_Slider,id:5446,x:31820,y:32721,ptovrint:False,ptlb:OutlineBrightness,ptin:_OutlineBrightness,varname:node_5446,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Add,id:1193,x:32248,y:32694,varname:node_1193,prsc:2|A-7241-RGB,B-5446-OUT;n:type:ShaderForge.SFN_Multiply,id:1580,x:32916,y:32472,varname:node_1580,prsc:2|A-5095-OUT,B-5073-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:5360,x:32525,y:32268,varname:node_5360,prsc:2|IN-8629-OUT,IMIN-5656-OUT,IMAX-8043-OUT,OMIN-9736-OUT,OMAX-8043-OUT;n:type:ShaderForge.SFN_Vector1,id:5656,x:32303,y:32437,varname:node_5656,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:8043,x:32315,y:32524,varname:node_8043,prsc:2,v1:1;n:type:ShaderForge.SFN_Negate,id:9736,x:32305,y:32108,varname:node_9736,prsc:2|IN-1159-OUT;n:type:ShaderForge.SFN_Clamp01,id:5095,x:32711,y:32279,varname:node_5095,prsc:2|IN-5360-OUT;n:type:ShaderForge.SFN_Slider,id:7056,x:31726,y:32969,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_7056,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3498728,max:1;proporder:7241-5041-1159-5446-7056;pass:END;sub:END;*/

Shader "Shader Forge/NavmeshShader" {
    Properties {
        _Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _OutlineThickness ("OutlineThickness", Range(0, 1)) = 0.7777778
        _SlopeShading ("SlopeShading", Range(0, 10)) = 0
        _OutlineBrightness ("OutlineBrightness", Range(0, 1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 0.3498728
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _OutlineThickness;
            uniform float _SlopeShading;
            uniform float _OutlineBrightness;
            uniform float _Opacity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float node_8629 = max(0,dot(float3(0,1,0),i.normalDir));
                float node_5656 = 0.0;
                float node_8043 = 1.0;
                float node_9736 = (-1*_SlopeShading);
                float3 node_5073 = lerp(_Color.rgb,(_Color.rgb+_OutlineBrightness),step(_OutlineThickness,i.uv0.g));
                float3 emissive = (saturate((node_9736 + ( (node_8629 - node_5656) * (node_8043 - node_9736) ) / (node_8043 - node_5656)))*node_5073);
                float3 finalColor = emissive;
                return fixed4(finalColor,_Opacity);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}

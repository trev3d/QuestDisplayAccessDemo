Shader "MediaProjectionDemo/OESTex" {
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue" = "Opaque" }
		LOD 100

		Pass
		{
			GLSLPROGRAM
			#pragma only_renderers gles gles3

			#extension GL_OES_EGL_image_external : require
			#extension GL_OES_EGL_image_external_ess13 : enable

			#include "UnityCG.glslinc"

			uniform samplerExternalOES _MainTex;

			#ifdef VERTEX

			varying vec2 uv;

			void main() {
				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
				uv = gl_MultiTexCoord0.xy;
			}

			#endif

			#ifdef FRAGMENT

			varying vec2 uv;

			void main() {
				gl_FragData[0] = textureExternal(_MainTex, uv);
			}

			#endif 

			ENDGLSL

		}
	}
}
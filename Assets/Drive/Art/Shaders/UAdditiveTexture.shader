// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

Shader "Magic Leap/Unlit/Additive Texture"
{
	Properties {
     _Color ("Main Color", Color) = (1,1,1,1)
	 _MainTex ("Main Texture", 2D) = "Black" {}
	}
  
	 SubShader {
		 Tags { "Queue"="Transparent" "RenderType" = "Transparent" }
		 Blend SrcAlpha One
		 ZWrite Off
		 Lighting Off
		 Cull Off
		 ZTest LEqual

		CGPROGRAM
		#pragma surface surf Additive  halfasview novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap

		half4 LightingAdditive (SurfaceOutput s, half3 lightDir, half3 viewDir)
  	    {
			half3 h = normalize (lightDir + viewDir);

			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		struct Input
		{
			float2 uv_MainTex;
		};
      
		float4 _Color;
		sampler2D _MainTex;

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 maintex = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = _Color * maintex;
		}
		ENDCG
	 }
 }


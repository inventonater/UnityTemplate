// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

Shader "Magic Leap/Unlit/Additive Color"
{
	Properties {
     _Color ("Main Color", Color) = (1,1,1,1)
	}
  
	 SubShader {
		 Tags { "Queue"="Transparent" "RenderType" = "Transparent" }
		 Blend One One
		 ZWrite Off
		 Lighting Off
		 Cull off
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
      
		float3 _Color;

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _Color;  
		}
		ENDCG
	 }
 }


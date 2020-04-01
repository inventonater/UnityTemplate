// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

 Shader "Magic Leap/Particles/Particle Additive"
 {
	Properties
	{
		_Color ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
    }

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType" = "Transparent"}
      
		ZWrite Off
		Lighting Off
		Cull off
		ZTest LEqual
		Blend One One
		CGPROGRAM
		#pragma surface surf Additive  vertex:vert novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap

		half4 LightingAdditive (SurfaceOutput s, half3 lightDir, half3 viewDir, fixed atten)
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
			float4 vertColor;
		};

		sampler2D _MainTex;
		float4 _Color;

		
		  void vert(inout appdata_full v, out Input o)
	  {
	      UNITY_INITIALIZE_OUTPUT(Input,o);
	  	  o.vertColor = v.color;
	  } 


		void surf (Input IN, inout SurfaceOutput o)
		{
			half3 maintex = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = maintex*IN.vertColor;
		}
		ENDCG
	}

    Fallback "Diffuse"
}
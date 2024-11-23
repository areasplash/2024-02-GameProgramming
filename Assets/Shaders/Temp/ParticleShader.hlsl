#ifndef PARTICLE_SHADER_H
#define PARTICLE_SHADER_H

	struct ParticleData {
		float3 position;
		float3 scale;

		float2 tiling;
		float2 offset;
		float3 color;
		float  emission;
		float  alpha;
	};

	StructuredBuffer<ParticleData> _ParticleData;

	CBUFFER_START(UnityPerObject)
		float2 _Tiling   = {1, 1};
		float2 _Offset   = {0, 0};
		float3 _Color    = {0, 0, 0};
		float  _Emission = 1;
		float  _Alpha    = 1;
	CBUFFER_END
	
	void Setup() {
		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			SetUnityMatrices(unity_InstanceID, unity_ObjectToWorld, unity_WorldToObject);
		#endif
	}

	void Passthrough_float(
		in  float3 In,
		out float3 Out,
		out float2 Out_Tiling,
		out float2 Out_Offset,
		out float3 Out_Color,
		out float  Out_Emission,
		out float  Out_Alpha) {
		
		Out = In;
		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			CreatureData data = _CreatureData[unity_InstanceID];
			Out_Tiling   = data.tiling;
			Out_Offset   = data.offset;
			Out_Color    = data.color;
			Out_Emission = data.emission;
			Out_Alpha    = data.alpha;
		#else
			Out_Tiling   = float2(1, 1);
			Out_Offset   = float2(0, 0);
			Out_Color    = float3(1, 1, 1);
			Out_Emission = float (0);
			Out_Alpha    = float (1);
		#endif
	}

#endif
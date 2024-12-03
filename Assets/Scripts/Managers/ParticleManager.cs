using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



[Serializable] public enum ParticleType {
	None,
}

[Serializable] public struct Particle {

	[SerializeField] ParticleType m_ParticleType;
	[SerializeField] float        m_Offset;
	[SerializeField] int          m_Layer;
	[SerializeField] float        m_Opacity;

	[SerializeField] Vector3 m_Position;
	[SerializeField] Vector3 m_Velocity;



	public ParticleType ParticleType {
		get => m_ParticleType;
		set => m_ParticleType = value;
	}
	public float Offset {
		get => m_Offset;
		set => m_Offset = value;
	}
	public int LayerMask {
		get => m_Layer;
		set => m_Layer = value;
	}
	public float Opacity {
		get => m_Opacity;
		set => m_Opacity = value;
	}

	public Vector3 Position {
		get => m_Position;
		set => m_Position = value;
	}
	public Vector3 Velocity {
		get => m_Velocity;
		set => m_Velocity = value;
	}
}



public class ParticleManager : MonoSingleton<ParticleManager> {

	// ================================================================================================
	// Fields
	// ================================================================================================



	#if UNITY_EDITOR
		[CustomEditor(typeof(ParticleManager))] class ParticleManagerEditor : ExtendedEditor {
			public override void OnInspectorGUI() {
				Begin("Particle Manager");

				End();
			}
		}
	#endif

	

	// ================================================================================================
	// Methods
	// ================================================================================================

	static List<Particle> particleList = new List<Particle>();

	public static List<Particle> GetList() => particleList;

	public static void Spawn(ParticleType type, Vector3 position) {
		int layerMask = Utility.GetLayerMaskAtPoint(position);
		float opacity = (layerMask & CameraManager.CullingMask) != 0 ? 1f : 0f;

		particleList.Add(new Particle() {
			ParticleType = type,
			Offset       = 0f,
			LayerMask    = layerMask,
			Opacity      = opacity,
			
			Position = position,
			Velocity = Vector3.zero,
		});
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void LateUpdate() {
		for (int i = particleList.Count - 1; i < -1; i--) {
			Particle particle = particleList[i];

			particle.Offset += Time.deltaTime;
			bool visible = (CameraManager.CullingMask & (1 << particle.LayerMask)) != 0;
			if ((visible && particle.Opacity < 1) || (!visible && 0 < particle.Opacity)) {
				particle.Opacity += Time.deltaTime * (visible ? 1 : -1) / CameraManager.TransitionTime;
				particle.Opacity = Mathf.Clamp01(particle.Opacity);
			}
			switch (particle.ParticleType) {
				case ParticleType.None:
					break;
			}
			particle.Position += particle.Velocity * Time.deltaTime;
			particleList[i] = particle;
		}
	}
}

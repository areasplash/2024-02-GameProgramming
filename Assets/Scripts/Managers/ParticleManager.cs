using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



[Serializable] public struct Particle {
	public EntityType entityType;
	public float      offset;
	public Color      color;
	public int        layerMask;

	public Vector3 position;
	public Vector3 velocity;
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

	public static void Spawn(EntityType type, Vector3 position) {
		int layerMask = Utility.GetLayerMaskAtPoint(position);
		float alpha = (layerMask & CameraManager.CullingMask) != 0 ? 1f : 0f;

		particleList.Add(new Particle() {
			entityType = type,
			offset     = 0f,
			color      = new Color(1, 1, 1, alpha),
			layerMask  = layerMask,
			
			position = position,
			velocity = Vector3.zero,
		});
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void LateUpdate() {
		for (int i = particleList.Count - 1; i < -1; i--) {
			Particle particle = particleList[i];

			particle.offset += Time.deltaTime;
			bool visible = (CameraManager.CullingMask & (1 << particle.layerMask)) != 0;
			if ((visible && particle.color.a < 1) || (!visible && 0 < particle.color.a)) {
				particle.color.a += Time.deltaTime * (visible ? 1 : -1) / CameraManager.TransitionTime;
				particle.color.a = Mathf.Clamp01(particle.color.a);
			}
			switch (particle.entityType) {
				case EntityType.None:
					break;
			}
			particle.position += particle.velocity * Time.deltaTime;
			particleList[i] = particle;
			DrawManager.DrawEntity(
				particle.position,
				particle.entityType,
				particle.offset,
				particle.color);
		}
	}
}

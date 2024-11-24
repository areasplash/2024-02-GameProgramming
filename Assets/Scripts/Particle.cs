using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



[Serializable] public enum ParticleType {
	None,
}



// ====================================================================================================
// Particle Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(Particle)), CanEditMultipleObjects]
	public class ParticleEditor : ExtendedEditor {

		Particle I => target as Particle;

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Particle Properties");

			LabelField("Particle", EditorStyles.boldLabel);
			I.ParticleType = EnumField ("Particle Type", I.ParticleType);
			I.Offset	   = FloatField("Offset",        I.Offset);
			Space();

			LabelField("Rigidbody", EditorStyles.boldLabel);
			I.Velocity = Vector3Field("Velocity", I.Velocity);
			Space();
			
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Particle
// ====================================================================================================

public class Particle : MonoBehaviour {

	// Constants

	const string PrefabPath = "Prefabs/Particle";



	// Fields

	[SerializeField] ParticleType  m_ParticleType  = ParticleType.None;
	[SerializeField] float         m_Offset        = 0;

	[SerializeField] Vector3 m_Velocity = Vector3.zero;



	// Properties

	public ParticleType ParticleType {
		get => m_ParticleType;
		set {
			m_ParticleType = value;
			#if UNITY_EDITOR
				bool pooled = value == ParticleType.None;
				gameObject.name = pooled? "Particle" : value.ToString();
			#endif
			Initialize();
		}
	}

	public float Offset {
		get => m_Offset;
		set => m_Offset = value;
	}



	public Vector3 Velocity {
		get => m_Velocity;
		set => m_Velocity = value;
	}



	// Methods

	static List<Particle> particleList = new List<Particle>();
	static List<Particle> particlePool = new List<Particle>();
	static Particle particlePrefab;
	static Particle particle;

	public static List<Particle> GetList() => particleList;

	public static Particle Spawn(ParticleType type, Vector3 position) {
		if (particlePool.Count == 0) {
			if (!particlePrefab) particlePrefab = Resources.Load<Particle>(PrefabPath);
			particle = Instantiate(particlePrefab);
		}
		else {
			int i = particlePool.Count - 1;
			particle = particlePool[i];
			particle.gameObject.SetActive(true);
			particlePool.RemoveAt(i);
		}
		particle.transform.position = position;
		particle.transform.rotation = Quaternion.identity;
		particle.ParticleType = type;
		return particle;
	}

	void OnSpawn() {
		particleList.Add(this);
	}

	public static void Despawn(Particle particle) {
		if (!particle) return;
		particle.gameObject.SetActive(false);
	}

	void OnDespawn() {
		ParticleType = ParticleType.None;
		Offset       = 0;
		Velocity     = Vector3.zero;

		particleList.Remove(this);
		particlePool.Add   (this);
	}

	void OnDestroy() {
		particleList.Remove(this);
		particlePool.Add   (this);
	}



	public void GetData(int[] data) {
		if (data == null) data = new int[7];
		int i = 0;
		data[i++] = Utility.ToInt(transform.position.x);
		data[i++] = Utility.ToInt(transform.position.y);
		data[i++] = Utility.ToInt(transform.position.z);
		data[i++] = Utility.ToInt(transform.rotation);

		data[i++] = Utility.ToInt(ParticleType);
		data[i++] = Utility.ToInt(Offset);
		data[i++] = Utility.ToInt(Velocity);
	}

	public void SetData(int[] data) {
		if (data == null) return;
		int i = 0;
		Vector3 position;
		position.x         = Utility.ToFloat(data[i++]);
		position.y         = Utility.ToFloat(data[i++]);
		position.z         = Utility.ToFloat(data[i++]);
		transform.position = position;
		transform.rotation = Utility.ToQuaternion(data[i++]);

		ParticleType       = Utility.ToEnum<ParticleType>(data[i++]);
		Offset             = Utility.ToFloat(data[i++]);
		Velocity           = Utility.ToVector3(data[i++]);
	}

	public static void GetData(List<int[]> data) {
		for (int i = 0; i < particleList.Count; i++) {
			if (data.Count - 1 < i) data.Add(null);
			particleList[i].GetData(data[i]);
		}
	}

	public static void SetData(List<int[]> data) {
		for (int i = 0; i < data.Count; i++) {
			if (particleList.Count - 1 < i) Spawn(ParticleType.None, Vector3.zero);
			particleList[i].SetData(data[i]);
		}
		for (int i = particleList.Count - 1; data.Count <= i; i--) Despawn(particleList[i]);
	}



	// Physics

	public float TransitionOpacity { get; set; }

	int layerMask = 0;

	void BeginTrigger() {
		layerMask = Utility.GetLayerMaskAtPoint(transform.position, transform);
		if (layerMask == 0) layerMask |= CameraManager.ExteriorLayer;
		TransitionOpacity = ((CameraManager.CullingMask | layerMask) != 0)? 1 : 0;
	}

	void FixedUpdate() {
		bool visible = (CameraManager.CullingMask & layerMask) != 0;
		if ((visible && TransitionOpacity < 1) || (!visible && 0 < TransitionOpacity)) {
			TransitionOpacity += (visible? 1 : -1) * Time.deltaTime / CameraManager.TransitionTime;
			TransitionOpacity = Mathf.Clamp01(TransitionOpacity);
		}

		if (Velocity != Vector3.zero) {
			transform.position += Velocity * Time.deltaTime;
		}
	}



	// Lifecycle

	Action OnUpdate;

	bool link = false;

	void OnEnable() {
		link = true;
		BeginTrigger();
		OnSpawn();
	}

	void Update() {
		if (link) {
			link = false;
			LinkAction();
		}
		Offset += Time.deltaTime;
		OnUpdate?.Invoke();
	}

	void OnDisable() {
		OnDespawn();
	}



	// Particle

	void Initialize() {
		switch (ParticleType) {
			case ParticleType.None:
				break;
		}
	}

	void LinkAction() {
		switch (ParticleType) {
			case ParticleType.None:
				break;
		}
	}
}

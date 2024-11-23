using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



[Serializable] public enum StructureType {
	None,
	Table,
	Chair,
}



// ====================================================================================================
// Structure Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(Structure)), CanEditMultipleObjects]
	public class StructureEditor : Editor {

		Structure I => target as Structure;

		T EnumField<T>(string label, T value) where T : Enum => (T)EnumPopup(label, value);

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Structure Properties");
			Space();
			LabelField("Structure", EditorStyles.boldLabel);
			I.StructureType = EnumField("Structure Type", I.StructureType);
			Space();
			LabelField("Rigidbody", EditorStyles.boldLabel);
			I.Velocity       = Vector3Field("Velocity",        I.Velocity);
			I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
			I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
			I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Structure
// ====================================================================================================

public class Structure : MonoBehaviour {

	// Constants

	const string PrefabPath = "Prefabs/Structure";



	// Fields

	[SerializeField] StructureType m_StructureType  = StructureType.None;

	[SerializeField] Vector3       m_Velocity       = Vector3.zero;
	[SerializeField] Vector3       m_ForcedVelocity = Vector3.zero;
	[SerializeField] Vector3       m_GroundVelocity = Vector3.zero;
	[SerializeField] Vector3       m_GravitVelocity = Vector3.zero;



	// Properties

	public StructureType StructureType {
		get => m_StructureType;
		set {
			m_StructureType = value;
			#if UNITY_EDITOR
				bool pooled = value == StructureType.None;
				gameObject.name = pooled? "Pooled Structure" : value.ToString();
			#endif
			Initialize();
		}
	}

	public Vector3 Velocity {
		get => m_Velocity;
		set => m_Velocity = value;
	}

	public Vector3 ForcedVelocity {
		get => m_ForcedVelocity;
		set => m_ForcedVelocity = value;
	}

	public Vector3 GroundVelocity {
		get => m_GroundVelocity;
		set => m_GroundVelocity = value;
	}

	public Vector3 GravitVelocity {
		get => m_GravitVelocity;
		set => m_GravitVelocity = value;
	}



	// Methods

	static List<Structure> structureList = new List<Structure>();
	static List<Structure> structurePool = new List<Structure>();
	static Structure structurePrefab;
	static Structure structure;

	public static List<Structure> GetList() => structureList;

	public static Structure Spawn(StructureType type, Vector3 position) {
		if (structurePool.Count == 0) {
			if (!structurePrefab) structurePrefab = Resources.Load<Structure>(PrefabPath);
			structure = Instantiate(structurePrefab);
		}
		else {
			int i = structurePool.Count - 1;
			structure = structurePool[i];
			structure.gameObject.SetActive(true);
			structurePool.RemoveAt(i);
		}
		structureList.Add(structure);
		structure.transform.position = position;
		structure.transform.rotation = Quaternion.identity;
		structure.StructureType = type;
		return structure;
	}

	void OnSpawn() {
		structureList.Add(this);
	}

	public static void Despawn(Structure structure) {
		if (!structure) return;
		structure.gameObject.SetActive(false);
	}

	void OnDespawn() {
		StructureType = StructureType.None;
		Velocity      = Vector3.zero;

		structureList.Remove(this);
		structurePool.Add   (this);
	}

	void OnDestroy() {
		structureList.Remove(this);
		structurePool.Remove(this);
	}



	public void GetData(int[] data) {
		if (data == null) data = new int[9];
		int i = 0;
		data[i++] = Utility.ToInt(transform.position.x);
		data[i++] = Utility.ToInt(transform.position.y);
		data[i++] = Utility.ToInt(transform.position.z);
		data[i++] = Utility.ToInt(transform.rotation);

		data[i++] = Utility.ToInt(StructureType);
		data[i++] = Utility.ToInt(Velocity);
		data[i++] = Utility.ToInt(ForcedVelocity);
		data[i++] = Utility.ToInt(GroundVelocity);
		data[i++] = Utility.ToInt(GravitVelocity);
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

		StructureType      = Utility.ToEnum<StructureType>(data[i++]);
		Velocity           = Utility.ToVector3(data[i++]);
		ForcedVelocity     = Utility.ToVector3(data[i++]);
		GroundVelocity     = Utility.ToVector3(data[i++]);
		GravitVelocity     = Utility.ToVector3(data[i++]);
	}

	public static void GetData(List<int[]> data) {
		for (int i = 0; i < structureList.Count; i++) {
			if (data.Count - 1 < i) data.Add(null);
			structureList[i].GetData(data[i]);
		}
	}

	public static void SetData(List<int[]> data) {
		for (int i = 0; i < data.Count; i++) {
			if (structureList.Count - 1 < i) Spawn(StructureType.None, Vector3.zero);
			structureList[i].SetData(data[i]);
		}
		for (int i = structureList.Count - 1; data.Count <= i; i--) Despawn(structureList[i]);
	}



	// Physics

	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;
	bool           isGrounded      = false;

	Rigidbody rb;

	void Start() => TryGetComponent(out rb);

	void BeginTrigger() {
		gameObject.layer = Utility.GetLayerAtPoint(transform.position, transform);

		grounds.Clear();
		groundChanged = true;
	}

	void OnTriggerEnter(Collider collider) {
		if (!collider.isTrigger) {
			grounds.Add(collider);
			groundChanged = true;
		}
	}

	void OnTriggerExit(Collider collider) {
		if (!collider.isTrigger) {
			grounds.Remove(collider);
			groundChanged = true;
		}
	}

	void EndTrigger() {
		if (groundChanged) {
			groundChanged = false;
			if (0 < grounds.Count) {
				int i = grounds.Count - 1;
				grounds[i].TryGetComponent(out groundRigidbody);
				groundRotation  = grounds[i].transform.localRotation;
				isGrounded      = true;
			}
			else {
				groundRigidbody = null;
				groundRotation  = Quaternion.identity;
				isGrounded      = false;
			}
		}
	}

	void FixedUpdate() {
		EndTrigger();

		if (groundRigidbody) GroundVelocity = groundRigidbody.linearVelocity;
		if (!isGrounded) GravitVelocity += Physics.gravity * Time.deltaTime;

		Vector3 linearVelocity = Vector3.zero;
		linearVelocity += groundRotation * Velocity;
		linearVelocity += ForcedVelocity + GroundVelocity + GravitVelocity;
		rb.linearVelocity = linearVelocity;
		if (Velocity != Vector3.zero) rb.rotation = Quaternion.LookRotation(Velocity);

		if (ForcedVelocity != Vector3.zero) {
			ForcedVelocity *= 0.9f;
			if (ForcedVelocity.sqrMagnitude < 0.01f) ForcedVelocity = Vector3.zero;
		}
		if (GroundVelocity != Vector3.zero) {
			GroundVelocity *= 0.9f;
			if (GroundVelocity.sqrMagnitude < 0.01f) GroundVelocity = Vector3.zero;
		}
		if (GravitVelocity != Vector3.zero) {
			if (isGrounded) GravitVelocity = Vector3.zero;
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
		OnUpdate?.Invoke();
	}

	void OnDisable() {
		OnDespawn();
	}



	// Structure

	void Initialize() {
		switch (StructureType) {
			case StructureType.None:
				break;
		}
	}

	void LinkAction() {
		switch (StructureType) {
			case StructureType.None:
				OnUpdate = () => {};
				break;
		}
	}
}

using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



[Serializable] public enum StructureType {
	None,
	Table,
	Chair,
	Chest,
}



public class Structure : MonoBehaviour {

	const string PrefabPath = "Prefabs/Structure";
	const string MeshPath   = "Prefabs/StructureMeshes/";



	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] StructureType m_StructureType = StructureType.None;
	[SerializeField] AttributeType m_AttributeType = 0;
	[SerializeField] GameObject    m_Mesh;

	[SerializeField] Vector3 m_Velocity       = Vector3.zero;
	[SerializeField] Vector3 m_ForcedVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GroundVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GravitVelocity = Vector3.zero;



	public StructureType StructureType {
		get => m_StructureType;
		set {
			if (m_StructureType == value) return;
			m_StructureType = value;
			#if UNITY_EDITOR
				bool pooled = value == StructureType.None;
				gameObject.name = pooled? "Structure" : value.ToString();
			#endif
			Initialize();
		}
	}

	public GameObject Mesh {
		get => m_Mesh;
		set => m_Mesh = value;
	}

	Rigidbody body;
	Rigidbody Body {
		get {
			if (!body) TryGetComponent(out body);
			return body;
		}
	}

	public AttributeType AttributeType {
		get => m_AttributeType;
		set {
			m_AttributeType = value;
			if ((value & AttributeType.Pinned) != 0) {
				Velocity       = Vector3.zero;
				ForcedVelocity = Vector3.zero;
				GroundVelocity = Vector3.zero;
				GravitVelocity = Vector3.zero;
				Body.       velocity = Vector3.zero;
				Body.angularVelocity = Vector3.zero;
				Body.mass = float.MaxValue;
			}
			else {
				Body.mass = 1;
			}
			if ((value & AttributeType.Floating) != 0) {
				GravitVelocity = Vector3.zero;
			}
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



	#if UNITY_EDITOR
	[CustomEditor(typeof(Structure))] class StructureEditor : ExtendedEditor {
			Structure I => target as Structure;
			public override void OnInspectorGUI() {
				Begin("Structure");

				LabelField("Structure", EditorStyles.boldLabel);
				I.StructureType = EnumField  ("Structure Type", I.StructureType);
				I.AttributeType = FlagField  ("Attribute Type", I.AttributeType);
				I.Mesh          = ObjectField("Mesh",           I.Mesh);
				Space();

				LabelField("Rigidbody", EditorStyles.boldLabel);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();
				
				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

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
		StructureType  = StructureType.None;
		AttributeType  = 0;
		
		Velocity       = Vector3.zero;
		ForcedVelocity = Vector3.zero;
		GroundVelocity = Vector3.zero;
		GravitVelocity = Vector3.zero;

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
		data[i++] = Utility.ToInt(AttributeType);

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
		AttributeType      = Utility.ToEnum<AttributeType>(data[i++]);

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



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	int            layer           = 0;

	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;
	bool           isGrounded      = false;

	void BeginTrigger() {
		layer = Utility.GetLayerAtPoint(transform.position, transform);

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
				Utility.TryGetComponentInParent(grounds[i].transform, out groundRigidbody);
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

		if ((AttributeType & AttributeType.Pinned) != 0) return;
		if (groundRigidbody) GroundVelocity = groundRigidbody.velocity;
		if (!isGrounded && (AttributeType & AttributeType.Floating) == 0) {
			GravitVelocity += Physics.gravity * Time.deltaTime;
		}

		Vector3 linearVelocity = Vector3.zero;
		linearVelocity += groundRotation * Velocity;
		linearVelocity += ForcedVelocity + GroundVelocity + GravitVelocity;
		Body.velocity = linearVelocity;

		if (ForcedVelocity != Vector3.zero) {
			ForcedVelocity *= !isGrounded ? 0.97f : 0.91f;
			if (ForcedVelocity.sqrMagnitude < 0.01f) ForcedVelocity = Vector3.zero;
		}
		if (GroundVelocity != Vector3.zero) {
			GroundVelocity *= !isGrounded ? 0.97f : 0.91f;
			if (GroundVelocity.sqrMagnitude < 0.01f) GroundVelocity = Vector3.zero;
		}
		if (GravitVelocity != Vector3.zero) {
			if (isGrounded) GravitVelocity = Vector3.zero;
		}
	}



	public bool IsInteractable() => (AttributeType & (AttributeType)(-1 & ~0xFFFF)) != 0;

	public AttributeType GetInteractableType() {
		if (IsInteractable()) for (int i = 16; i < 32; i++) {
			if ((AttributeType & (AttributeType)(1 << i)) != 0) return (AttributeType)(1 << i);
		}
		return 0;
	}

	public void Interact(Creature creature) {
		if (IsInteractable()) OnInteract?.Invoke(creature);
	}



	Action<Creature> OnInteract;
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
		Destroy(Mesh);
		OnDespawn();
	}



	// ------------------------------------------------------------------------------------------------
	// Structure
	// ------------------------------------------------------------------------------------------------

	static Dictionary<StructureType, GameObject> mesh = new Dictionary<StructureType, GameObject>();

	void SetLayer(GameObject gameObject, int layer) {
		gameObject.layer = layer;
		for (int i = 0; i < gameObject.transform.childCount; i++) {
			SetLayer(gameObject.transform.GetChild(i).gameObject, layer);
		}
	}

	void Initialize() {
		if (!mesh.ContainsKey(StructureType)) mesh.Add(StructureType, null);

		if (!Mesh || !mesh[StructureType] || !Mesh.name.Equals(mesh[StructureType].name)) {
			if (Mesh) {
				if (Application.isPlaying) Destroy(Mesh);
				else DestroyImmediate(Mesh);
			}
			int layer = Utility.GetLayerAtPoint(transform.position, transform);
			if (mesh[StructureType] ??= Resources.Load<GameObject>(MeshPath + StructureType.ToString())) {
				Mesh = Instantiate(mesh[StructureType], transform);
				Mesh.transform.name = mesh[StructureType].name;
				Mesh.transform.SetParent(transform);
				Mesh.transform.localPosition = Vector3.zero;
				Mesh.transform.localRotation = Quaternion.identity;
				SetLayer(Mesh, layer);
			}
		}

		switch (StructureType) {
			case StructureType.None:
				break;
			case StructureType.Table:
				AttributeType = AttributeType.Pinned;
				break;
			case StructureType.Chair:
				AttributeType = AttributeType.Pinned | AttributeType.Interact;
				break;
			case StructureType.Chest:
				AttributeType = AttributeType.Pinned;
				break;
		}
	}

	void LinkAction() {
		switch (StructureType) {
			case StructureType.None:
				OnInteract = null;
				OnUpdate   = null;
				break;
		}
	}
}

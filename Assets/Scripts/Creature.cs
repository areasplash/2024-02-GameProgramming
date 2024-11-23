using UnityEngine;
using UnityEngine.AI;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



[Serializable] public enum AnimationType {
	Idle,
	Move,
	Serve,
	Seat,
	Eat,
	Drink,
}

[Serializable] public enum CreatureType {
	None,
	Player,
	Client,
}





// ====================================================================================================
// Creature Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(Creature)), CanEditMultipleObjects]
	public class CreatureEditor : Editor {

		Creature I => target as Creature;

		T EnumField<T>(string label, T value) where T : Enum => (T)EnumPopup(label, value);

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Creature Properties");
			Space();
			LabelField("Creature", EditorStyles.boldLabel);
			I.CreatureType  = EnumField ("Creature Type",  I.CreatureType);
			I.AnimationType = EnumField ("Animation Type", I.AnimationType);
			I.Offset        = FloatField("Offset",         I.Offset);
			I.HitboxType    = EnumField ("Hitbox Type",    I.HitboxType);
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
// Creature
// ====================================================================================================

public class Creature : MonoBehaviour {

	// Constants

	const string PrefabPath = "Prefabs/Creature";



	// Fields

	[SerializeField] CreatureType  m_CreatureType  = CreatureType.None;
	[SerializeField] AnimationType m_AnimationType = AnimationType.Idle;
	[SerializeField] float         m_Offset        = 0;
	[SerializeField] HitboxType    m_HitboxType    = HitboxType.Humanoid;

	[SerializeField] Vector3 m_Velocity       = Vector3.zero;
	[SerializeField] Vector3 m_ForcedVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GroundVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GravitVelocity = Vector3.zero;



	// Properties

	public CreatureType CreatureType {
		get => m_CreatureType;
		set {
			m_CreatureType = value;
			#if UNITY_EDITOR
				bool pooled = value == CreatureType.None;
				gameObject.name = pooled? "Pooled Creature" : value.ToString();
			#endif
			Initialize();
		}
	}

	public AnimationType AnimationType {
		get => m_AnimationType;
		set => m_AnimationType = value;
	}

	public float Offset {
		get => m_Offset;
		set => m_Offset = value;
	}

	public HitboxType HitboxType {
		get => m_HitboxType;
		set {
			m_HitboxType = value;
			HitboxData data = NavMeshManager.GetHitboxData(value);
			if (TryGetComponent(out CapsuleCollider capsule)) {
				capsule.radius = data.radius;
				capsule.height = data.height;
			}
			if (TryGetComponent(out SphereCollider sphere)) {
				sphere.center = new Vector3(0, -(data.height / 2 - data.radius) - 0.08f, 0);
				sphere.radius = data.radius - 0.04f;
			}
			if (TryGetComponent(out NavMeshAgent agent)) {
				agent.agentTypeID = data.agentTypeID;
				agent.radius = data.radius;
				agent.height = data.height;
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



	// Methods

	static List<Creature> creatureList = new List<Creature>();
	static List<Creature> creaturePool = new List<Creature>();
	static Creature creaturePrefab;
	static Creature creature;

	public static List<Creature> GetList() => creatureList;

	public static Creature Spawn(CreatureType type, Vector3 position) {
		if (creaturePool.Count == 0) {
			if (!creaturePrefab) creaturePrefab = Resources.Load<Creature>(PrefabPath);
			creature = Instantiate(creaturePrefab);
		}
		else {
			int i = creaturePool.Count - 1;
			creature = creaturePool[i];
			creature.gameObject.SetActive(true);
			creaturePool.RemoveAt(i);
		}
		creature.transform.position = position;
		creature.transform.rotation = Quaternion.identity;
		creature.CreatureType = type;
		return creature;
	}

	void OnSpawn() {
		creatureList.Add(this);
	}

	public static void Despawn(Creature creature) {
		if (!creature) return;
		creature.gameObject.SetActive(false);
	}

	void OnDespawn() {
		CreatureType   = CreatureType.None;
		AnimationType  = AnimationType.Idle;
		Offset         = 0;
		HitboxType     = HitboxType.Humanoid;

		Velocity       = Vector3.zero;
		ForcedVelocity = Vector3.zero;
		GroundVelocity = Vector3.zero;
		GravitVelocity = Vector3.zero;

		creatureList.Remove(this);
		creaturePool.Add   (this);
	}

	void OnDestroy() {
		creatureList.Remove(this);
		creaturePool.Remove(this);
	}



	void GetData(int[] data) {
		if (data == null) data = new int[12];
		int i = 0;
		data[i++] = Utility.ToInt(transform.position.x);
		data[i++] = Utility.ToInt(transform.position.y);
		data[i++] = Utility.ToInt(transform.position.z);
		data[i++] = Utility.ToInt(transform.rotation);

		data[i++] = Utility.ToInt(CreatureType);
		data[i++] = Utility.ToInt(AnimationType);
		data[i++] = Utility.ToInt(Offset);
		data[i++] = Utility.ToInt(HitboxType);

		data[i++] = Utility.ToInt(Velocity);
		data[i++] = Utility.ToInt(ForcedVelocity);
		data[i++] = Utility.ToInt(GroundVelocity);
		data[i++] = Utility.ToInt(GravitVelocity);
	}

	void SetData(int[] data) {
		if (data == null) return;
		int i = 0;
		Vector3 position;
		position.x         = Utility.ToFloat(data[i++]);
		position.y         = Utility.ToFloat(data[i++]);
		position.z         = Utility.ToFloat(data[i++]);
		transform.position = position;
		transform.rotation = Utility.ToQuaternion(data[i++]);

		CreatureType       = Utility.ToEnum<CreatureType>(data[i++]);
		AnimationType      = Utility.ToEnum<AnimationType>(data[i++]);
		Offset             = Utility.ToFloat(data[i++]);
		HitboxType         = Utility.ToEnum<HitboxType>(data[i++]);

		Velocity           = Utility.ToVector3(data[i++]);
		ForcedVelocity     = Utility.ToVector3(data[i++]);
		GroundVelocity     = Utility.ToVector3(data[i++]);
		GravitVelocity     = Utility.ToVector3(data[i++]);
	}

	public static void GetData(List<int[]> data) {
		for (int i = 0; i < creatureList.Count; i++) {
			if (data.Count - 1 < i) data.Add(null);
			creatureList[i].GetData(data[i]);
		}
	}

	public static void SetData(List<int[]> data) {
		for (int i = 0; i < data.Count; i++) {
			if (creatureList.Count - 1 < i) Spawn(CreatureType.None, Vector3.zero);
			creatureList[i].SetData(data[i]);
		}
		for (int i = creatureList.Count - 1; data.Count <= i; i--) Despawn(creatureList[i]);
	}



	// Physics

	public float TransitionOpacity { get; set; }

	List<Collider> layers          = new List<Collider>();
	bool           layerChanged    = false;
	int            layerMask       = 0;

	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;
	bool           isGrounded      = false;

	Rigidbody rb;

	void Start() => TryGetComponent(out rb);

	void BeginTrigger() {
		layers.Clear();
		layerChanged = true;
		layerMask = Utility.GetLayerMaskAtPoint(transform.position, transform);
		if (layerMask == 0) layerMask |= CameraManager.ExteriorLayer;
		TransitionOpacity = ((CameraManager.CullingMask | layerMask) != 0)? 1 : 0;

		grounds.Clear();
		groundChanged = true;
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.isTrigger) {
			layers.Add(collider);
			layerChanged = true;
		}
		else {
			grounds.Add(collider);
			groundChanged = true;
		}
	}

	void OnTriggerExit(Collider collider) {
		if (collider.isTrigger) {
			layers.Remove(collider);
			layerChanged = true;
		}
		else {
			grounds.Remove(collider);
			groundChanged = true;
		}
	}

	void EndTrigger() {
		if (layerChanged) {
			layerChanged = false;
			layerMask = 0;
			for(int i = 0; i < layers.Count; i++) layerMask |= 1 << layers[i].gameObject.layer;
			if (layerMask == 0) layerMask |= CameraManager.ExteriorLayer;
		}
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

		bool visible = (CameraManager.CullingMask & layerMask) != 0;
		if ((visible && TransitionOpacity < 1) || (!visible && 0 < TransitionOpacity)) {
			TransitionOpacity += (visible? 1 : -1) * Time.deltaTime / CameraManager.TransitionTime;
			TransitionOpacity = Mathf.Clamp01(TransitionOpacity);
		}

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
		Offset += Time.deltaTime;
		OnUpdate?.Invoke();
	}

	void OnDisable() {
		OnDespawn();
	}



	// Creature

	void Initialize() {
		switch (CreatureType) {
			case CreatureType.None:
				break;
			case CreatureType.Player:
				InitializeAsPlayer();
				break;
			case CreatureType.Client:
				break;
		}
	}

	void LinkAction() {
		switch (CreatureType) {
			case CreatureType.None:
				OnUpdate = () => {};
				break;
			case CreatureType.Player:
				OnUpdate = () => UpdatePlayer();
				break;
			case CreatureType.Client:
				OnUpdate = () => UpdateClient();
				break;
		}
	}



	// Player

	public Vector3        input = Vector3.zero;
	public Queue<Vector3> queue = new Queue<Vector3>();

	void InitializeAsPlayer() {
		HitboxType = HitboxType.Humanoid;
	}

	void UpdatePlayer() {
		if (input == Vector3.zero && queue.Count == 0) {
			AnimationType = AnimationType.Idle;
			Velocity = Vector3.zero;
		}
		else {
			AnimationType = AnimationType.Move;
			if (input != Vector3.zero) {
				Velocity = input * 5;
				if (0 < queue.Count) queue.Clear();
			}
			else {
				Vector3 delta = queue.Peek() - transform.position;
				Velocity = new Vector3(delta.x, 0, delta.z).normalized * 5;
				if (new Vector3(delta.x, 0, delta.z).magnitude < 0.1f) queue.Dequeue();
			}
		}
	}



	// Client

	public void UpdateClient() {

	}
}

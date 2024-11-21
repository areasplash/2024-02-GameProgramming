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

		SerializedProperty m_CreatureType;
		SerializedProperty m_AnimationType;
		SerializedProperty m_Offset;
		SerializedProperty m_HitboxType;

		SerializedProperty m_Velocity;
		SerializedProperty m_ForcedVelocity;
		SerializedProperty m_GroundVelocity;
		SerializedProperty m_GravitVelocity;

		Creature I => target as Creature;

		void OnEnable() {
			m_CreatureType  = serializedObject.FindProperty("m_CreatureType");
			m_AnimationType = serializedObject.FindProperty("m_AnimationType");
			m_Offset        = serializedObject.FindProperty("m_Offset");
			m_HitboxType    = serializedObject.FindProperty("m_HitboxType");

			m_Velocity       = serializedObject.FindProperty("m_Velocity");
			m_ForcedVelocity = serializedObject.FindProperty("m_ForcedVelocity");
			m_GroundVelocity = serializedObject.FindProperty("m_GroundVelocity");
			m_GravitVelocity = serializedObject.FindProperty("m_GravitVelocity");
		}

		T EnumField<T>(string label, T value) where T : Enum => (T)EnumPopup(label, value);

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Creature Properties");
			Space();
			LabelField("Core", EditorStyles.boldLabel);
			I.CreatureType  = EnumField ("Creature Type",  I.CreatureType);
			I.AnimationType = EnumField ("Animation Type", I.AnimationType);
			I.Offset        = FloatField("Offset",         I.Offset);
			I.HitboxType    = EnumField ("Hitbox Type",    I.HitboxType);
			Space();
			LabelField("Rigidbody", EditorStyles.boldLabel);
			PropertyField(m_Velocity);
			PropertyField(m_ForcedVelocity);
			PropertyField(m_GroundVelocity);
			PropertyField(m_GravitVelocity);
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
		set => m_CreatureType = value;
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
			HitboxData data = NavMeshManager.I.GetHitboxData(value);
			if (TryGetComponent(out CapsuleCollider capsule)) {
				capsule.radius = data.radius;
				capsule.height = data.height;
			}
			if (TryGetComponent(out SphereCollider sphere)) {
				sphere.center = new Vector3(0, -(data.height / 2 - data.radius) - 0.08f, 0);
				sphere.radius = data.radius - 0.06f;
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



	public Action OnSpawn   { get; set; }
	public Action OnDespawn { get; set; }
	public Action OnUpdate  { get; set; }



	// Methods

	public void GetData(ref int[] data) {
		int i = 0;
		data[i++] = Utility.ToInt(transform.position.x);
		data[i++] = Utility.ToInt(transform.position.y);
		data[i++] = Utility.ToInt(transform.position.z);
		data[i++] = Utility.ToInt(transform.rotation);

		data[i++] = Utility.ToInt(CreatureType);
		data[i++] = Utility.ToInt(AnimationType);
		data[i++] = Utility.ToInt(Offset);
		data[i++] = Utility.ToInt(HitboxType);

		data[i++] = Utility.ToInt(Velocity, true);
		data[i++] = Utility.ToInt(ForcedVelocity, true);
		data[i++] = Utility.ToInt(GroundVelocity, true);
		data[i++] = Utility.ToInt(GravitVelocity, true);
	}

	public void SetData(int[] data) {
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

		Velocity           = Utility.ToVector3(data[i++], true);
		ForcedVelocity     = Utility.ToVector3(data[i++], true);
		GroundVelocity     = Utility.ToVector3(data[i++], true);
		GravitVelocity     = Utility.ToVector3(data[i++], true);
	}



	// Lifecycle

	bool creatureSpawned;

	

	void OnEnable() => creatureSpawned = true;

	void Update() {
		if (creatureSpawned) {
			creatureSpawned = false;
			InitAction();
			InitTrigger();
			OnSpawn?.Invoke();
		}
		Offset += Time.deltaTime;
		OnUpdate?.Invoke();
	}

	void OnDisable() {
		OnDespawn?.Invoke();
	}



	// Physics

	Rigidbody rb;

	List<Collider> layers          = new List<Collider>();
	bool           layerChanged    = false;
	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;



	public int   LayerMask    { get; private set; }
	public float LayerOpacity { get; set; }
	public bool  IsGrounded   { get; private set; }



	void Start() => TryGetComponent(out rb);

	void InitTrigger() {
		layers.Clear();
		LayerMask    = Utility.GetLayerMaskAtPoint(transform.position, gameObject);
		LayerOpacity = ((CameraManager.I.LayerMask | LayerMask) != 0)? 1 : 0;

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
			LayerMask = 0;
			for(int i = 0; i < layers.Count; i++) LayerMask |= 1 << layers[i].gameObject.layer;
			layerChanged = false;
		}
		if (groundChanged) {
			if (0 < grounds.Count) {
				int i = grounds.Count - 1;
				grounds[i].TryGetComponent(out groundRigidbody);
				groundRotation  = grounds[i].transform.localRotation;
				IsGrounded      = true;
			}
			else {
				groundRigidbody = null;
				groundRotation  = Quaternion.identity;
				IsGrounded      = false;
			}
			groundChanged = false;
		}
	}

	void FixedUpdate() {
		EndTrigger();
		if (groundRigidbody) GroundVelocity = groundRigidbody.linearVelocity;
		if (!IsGrounded) GravitVelocity += Physics.gravity * Time.deltaTime;
		
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
			if (IsGrounded) GravitVelocity = Vector3.zero;
		}
	}



	// Creature Pool

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
		creatureList.Add(creature);
		creature.transform.position = position;
		return creature;
	}

	public static void Despawn(Creature creature) {
		if (!creature) return;
		creature.gameObject.SetActive(false);
		creatureList.Remove(creature);
		creaturePool.Add   (creature);
	}

	void OnDestory() {
		creatureList.Remove(this);
		creaturePool.Add   (this);
	}



	// Creature Action

	void InitAction() {
		switch (CreatureType) {
			case CreatureType.None:
			case CreatureType.Player:
				OnSpawn   = () => {};
				OnDespawn = () => {};
				OnUpdate  = () => UpdatePlayer();
				break;
			case CreatureType.Client:
				OnSpawn   = () => {};
				OnDespawn = () => {};
				OnUpdate  = () => UpdateClient();
				break;
		}
	}



	// Action : Player

	public Vector3        input = Vector3.zero;
	public Queue<Vector3> queue = new Queue<Vector3>();



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



	// Action : Client

	public void UpdateClient() {

	}
}

using UnityEngine;

using System;
using System.Collections.Generic;
using UnityEngine.AI;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



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

		SerializedProperty m_Speed;
		SerializedProperty m_Force;

		Creature I => target as Creature;

		void OnEnable() {
			m_CreatureType  = serializedObject.FindProperty("m_CreatureType");
			m_AnimationType = serializedObject.FindProperty("m_AnimationType");
			m_Offset        = serializedObject.FindProperty("m_Offset");
			m_HitboxType    = serializedObject.FindProperty("m_HitboxType");

			m_Speed = serializedObject.FindProperty("m_Speed");
			m_Force = serializedObject.FindProperty("m_Force");
		}

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
			LabelField("Movement", EditorStyles.boldLabel);
			PropertyField(m_Speed);
			PropertyField(m_Force);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Creature
// ====================================================================================================

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
}



public class Creature : MonoBehaviour {

	// Constants

	const string PrefabPath = "Prefabs/Creature";



	// Fields

	[SerializeField] CreatureType  m_CreatureType  = CreatureType.None;
	[SerializeField] AnimationType m_AnimationType = AnimationType.Idle;
	[SerializeField] float         m_Offset        = 0;
	[SerializeField] HitboxType    m_HitboxType    = HitboxType.Humanoid;

	[SerializeField] float   m_Speed = 0;
	[SerializeField] Vector3 m_Force = Vector3.zero;



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
				sphere.center = new Vector3(0, -(data.height / 2 - data.radius) - 0.10f, 0);
				sphere.radius = data.radius - 0.05f;
			}
			if (TryGetComponent(out NavMeshAgent agent)) {
				agent.agentTypeID = data.agentTypeID;
				agent.radius = data.radius;
				agent.height = data.height;
			}
		}
	}



	public float Speed {
		get => m_Speed;
		set => m_Speed = value;
	}

	public Vector3 Force {
		get => m_Force;
		set => m_Force = value;
	}



	public int   LayerMask    { get; private set; }
	public float LayerOpacity { get; set; }



	// Methods

	public void GetData(ref int[] data) {
		int i = 0;
		data[i++] = Utility.ToInt(transform.position);
		data[i++] = Utility.ToInt(transform.rotation);

		data[i++] = Utility.ToInt(CreatureType);
		data[i++] = Utility.ToInt(AnimationType);
		data[i++] = Utility.ToInt(Offset);
		data[i++] = Utility.ToInt(HitboxType);

		data[i++] = Utility.ToInt(Speed);
		data[i++] = Utility.ToInt(Force, true);
	}

	public void SetData(int[] data) {
		int i = 0;
		transform.position = Utility.ToVector3(data[i++]);
		transform.rotation = Utility.ToQuaternion(data[i++]);

		CreatureType       = Utility.ToEnum<CreatureType>(data[i++]);
		AnimationType      = Utility.ToEnum<AnimationType>(data[i++]);
		Offset             = Utility.ToFloat(data[i++]);
		HitboxType         = Utility.ToEnum<HitboxType>(data[i++]);

		Speed              = Utility.ToFloat(data[i++]);
		Force              = Utility.ToVector3(data[i++], true);
	}

	

	static List<Creature> creatureList = new List<Creature>();
	static List<Creature> creaturePool = new List<Creature>();

	static Creature creature;
	static Creature creaturePrefab;

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
		creatureList.Remove(creature);
		creaturePool.Add   (creature);
		creature.gameObject.SetActive(false);
	}

	void OnRemove() {
		creatureList.Remove(this);
		creaturePool.Add   (this);
		gameObject.SetActive(false);
	}



	// Lifecycle

	List<Collider> layers       = new List<Collider>();
	bool           layerChanged = false;

	void BeginDetectLayer() {
		layerChanged = false;
	}

	void OnDetectLayerEnter(Collider collider) {
		if (collider.isTrigger) {
			layers.Add(collider);
			layerChanged = true;
		}
	}

	void OnDetectLayerExit(Collider collider) {
		if (collider.isTrigger) {
			layers.Remove(collider);
			layerChanged = true;
		}
	}

	void EndDetectLayer() {
		if (layerChanged) {
			LayerMask = 0;
			for(int i = 0; i < layers.Count; i++) LayerMask |= 1 << layers[i].gameObject.layer;
		}
	}



	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;
	bool           isGrounded      = false;

	void BeginDetectGround() {
		groundChanged = false;
	}

	void OnDetectGroundEnter(Collider collider) {
		if (!collider.isTrigger) {
			grounds.Add(collider);
			groundChanged = true;
		}
	}

	void OnDetectGroundExit(Collider collider) {
		if (!collider.isTrigger) {
			grounds.Remove(collider);
			groundChanged = true;
		}
	}

	void EndDetectGround() {
		if (groundChanged) {
			if (0 < grounds.Count) {
				int i = grounds.Count - 1;
				grounds[i].TryGetComponent(out groundRigidbody);
				groundRotation  = grounds[i].transform.rotation;
				isGrounded      = true;
			}
			else {
				groundRigidbody = null;
				groundRotation  = Quaternion.identity;
				isGrounded      = false;
			}
		}
	}



	Rigidbody rb;

	void StartPhysics() => TryGetComponent(out rb);

	public Vector3        input = Vector3.zero;
	public Queue<Vector3> queue = new Queue<Vector3>();

	void UpdatePhysics() {
		Vector3 velocity = Vector3.zero;
		if (input == Vector3.zero && queue.Count == 0) {
			AnimationType = AnimationType.Idle;
		}
		else {
			AnimationType = AnimationType.Move;
			if (input != Vector3.zero) {
				velocity = input * Speed;
				if (0 < queue.Count) queue.Clear();
			}
			else {
				Vector3 delta = queue.Peek() - transform.position;
				velocity = new Vector3(delta.x, 0, delta.z).normalized * Speed;
				if (new Vector3(delta.x, 0, delta.z).magnitude < 0.1f) queue.Dequeue();
			}
		}
		Force = isGrounded ? Vector3.zero : Force + Physics.gravity * Time.deltaTime;
		rb.linearVelocity = groundRotation * velocity + Force;
		if (velocity != Vector3.zero) rb.rotation = Quaternion.LookRotation(velocity);
	}



	void Start() => StartPhysics();

	void OnEnable() {
		layerChanged  = true;
		groundChanged = true;
	}

	void Update() => Offset += Time.deltaTime;

	void FixedUpdate() {
		EndDetectLayer   ();
		BeginDetectLayer ();
		EndDetectGround  ();
		BeginDetectGround();

		UpdatePhysics();
	}

	void OnTriggerEnter(Collider collider) {
		OnDetectLayerEnter (collider);
		OnDetectGroundEnter(collider);
	}

	void OnTriggerExit(Collider collider) {
		OnDetectLayerExit (collider);
		OnDetectGroundExit(collider);
	}

	void OnDestory() => OnRemove();
}

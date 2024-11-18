using UnityEngine;

using System;
using System.Collections.Generic;
using UnityEngine.AI;
using NUnit.Framework;




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



	public int   CurrentMask  { get; private set; }
	public float LayerOpacity { get; set; }

	public bool  IsFalling    { get; private set; }



	// Methods





	// Lifecycle

	int currentMask;

	void BeginDetectLayer() {
		currentMask = 0;
	}

	void DetectLayer(Collider collider) {
		if (collider.isTrigger) currentMask |= 1 << collider.gameObject.layer;
	}

	void EndDetectLayer() {
		CurrentMask = currentMask;
	}



	bool isFalling;

	void BeginDetectFalling() {
		isFalling = true;
	}

	void DetectFalling(Collider collider) {
		if (!collider.isTrigger) isFalling = false;
	}

	void EndDetectFalling() {
		IsFalling = isFalling;
	}



	Rigidbody rb;

	void StartPhysics() => TryGetComponent(out rb);

	public Queue<Vector3> queue = new Queue<Vector3>();

	void UpdatePhysics() {
		Vector3 velocity = Vector3.zero;
		if (queue.Count == 0) {
			AnimationType = AnimationType.Idle;
		}
		else {
			AnimationType = AnimationType.Move;
			Vector3 delta = queue.Peek() - transform.position;
			velocity = new Vector3(delta.x, 0, delta.z).normalized * Speed;
			if (velocity != Vector3.zero) rb.rotation = Quaternion.LookRotation(velocity);
			if (new Vector3(delta.x, 0, delta.z).magnitude < 0.1f) queue.Dequeue();
		}
		Force = IsFalling? Force + Physics.gravity * Time.fixedDeltaTime : Vector3.zero;
		rb.linearVelocity = velocity + Force;
	}



	void Start() => StartPhysics();

	void Update() => Offset += Time.deltaTime;

	void FixedUpdate() {
		EndDetectLayer();
		BeginDetectLayer();

		EndDetectFalling();
		UpdatePhysics();
		BeginDetectFalling();
	}

	void OnTriggerStay(Collider collider) {
		DetectLayer  (collider);
		DetectFalling(collider);
	}
}

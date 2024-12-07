using UnityEngine;

using System;
using System.Collections.Generic;



[Serializable] public enum EntityType {
	None,
	Player,
	Staff,
	
	Client,
		Adventurer,
		Knight,
		Necromancer,
		Priest,
		Wizard,

	Money,
		Money1,
		Money2,
		Money3,
		Money4,
		Money5,

	Item,
		ItemPlatter,
		ItemFlour,
		ItemButter,
		ItemCheese,
		ItemBlueberry,
		ItemTomato,
		ItemPotato,
		ItemCabbage,
		ItemMeat,

	Food,
		FoodPancake,
		FoodCheeseCake,
		FoodSpaghetti,
		FoodSoup,
		FoodSandwich,
		FoodSalad,
		FoodSteak,
		FoodWine,
		FoodBeer,

	Particle,
	
	Structure,
		Table,
		Chair,
		Chest,
		Pot,
		Trashcan,
	
	UI,
		UIBubble,
		UILoading,
		UIEating,
		UIBarBorder,
		UIBarFill,
}

[Serializable] public enum MotionType {
	None,
	Idle,
	Move,
}

[Serializable, Flags] public enum AttributeType {
	Pinned   = 1 <<  0,
	Floating = 1 <<  1,
	Piercing = 1 <<  2,
}

[Serializable] public enum ImmunityType {
	None,
	Low,
	Partial,
	Full,
}

[Serializable] public struct Effect {
	float        m_strength;
	float        m_duration;
	ImmunityType m_immunity;

	public float        strength { get => m_strength; set => m_strength = Mathf.Max(0, value); }
	public float        duration { get => m_duration; set => m_duration = Mathf.Max(0, value); }
	public ImmunityType immunity { get => m_immunity; set => m_immunity = value; }

	public Effect(float strength = 0, float duration = 0, ImmunityType immunity = ImmunityType.None) {
		m_strength = Mathf.Max(0, strength);
		m_duration = Mathf.Max(0, duration);
		m_immunity = immunity;
	}
}

[Serializable] public struct Status {
	float m_limit;
	float m_value;

	public float limit { get => m_limit; set => m_limit = Mathf.Max(    0, value); }
	public float value { get => m_value; set => m_value = Mathf.Min(value, limit); }

	public Status(float limit = 0, float value = 0) {
		m_limit = Mathf.Max(    0,   limit);
		m_value = Mathf.Min(value, m_limit);
	}
}

// Only used in UI
[Serializable] public enum InteractionType {
	None,
	Interact,
	TakeOut,
	PutIn,
	Add,
	Cook,
	Cancel,
	Drop,
	Serve,
	Collect,
}



[RequireComponent(typeof(Rigidbody))] public abstract class Entity : MonoBehaviour {

	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] EntityType m_EntityType = EntityType.None;
	[SerializeField] MotionType m_MotionType = MotionType.None;
	[SerializeField] float      m_Offset     = 0f;
	[SerializeField] Color      m_Color      = Color.white;
	[SerializeField] float      m_Intensity  = 0f;

	[SerializeField] HitboxType      m_HitboxType    = HitboxType.Humanoid;
	[SerializeField] AttributeType   m_AttributeType = 0;
	[SerializeField] float           m_SenseRange    = 0f;

	[SerializeField] float   m_Speed          = 0f;
	[SerializeField] Vector3 m_Velocity       = Vector3.zero;
	[SerializeField] Vector3 m_ForcedVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GroundVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GravitVelocity = Vector3.zero;



	public EntityType EntityType {
		get => m_EntityType;
		protected set {
			if (m_EntityType == value) return;
			m_EntityType = value;
			#if UNITY_EDITOR
				name = value.ToString();
			#endif
		}
	}
	public MotionType MotionType {
		get           => m_MotionType;
		protected set => m_MotionType = value;
	}
	public float Offset {
		get           => m_Offset;
		protected set => m_Offset = value;
	}
	public Color Color {
		get           => m_Color;
		protected set => m_Color = value;
	}
	public float Intensity {
		get           => m_Intensity;
		protected set => m_Intensity = value;
	}
	


	CapsuleCollider hitbox;
	SphereCollider  ground;
	protected CapsuleCollider Hitbox => hitbox? hitbox : TryGetComponent(out hitbox)? hitbox : null;
	protected SphereCollider  Ground => ground? ground : TryGetComponent(out ground)? ground : null;

	public HitboxType HitboxType {
		get => m_HitboxType;
		protected set {
			if (m_HitboxType == value) return;
			m_HitboxType = value;
			HitboxData data = NavMeshManager.GetHitboxData(value);
			if (Hitbox) {
				hitbox.radius = data.radius;
				hitbox.height = data.height;
			}
			if (Ground) {
				ground.center = new Vector3(0, -(data.height / 2 - data.radius) - 0.08f, 0);
				ground.radius = data.radius - 0.04f;
			}
		}
	}

	Rigidbody body;
	protected Rigidbody Body => body? body : TryGetComponent(out body)? body : null;

	public AttributeType AttributeType {
		get => m_AttributeType;
		protected set {
			if (m_AttributeType == value) return;
			m_AttributeType = value;
			if ((value & AttributeType.Pinned) != 0) {
				Velocity 	   = Vector3.zero;
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
			if ((value & AttributeType.Piercing) != 0) {
				//if (hitbox || TryGetComponent(out hitbox)) hitbox.excludeLayers |=  EntityMask;
				//if (ground || TryGetComponent(out ground)) ground.excludeLayers |=  EntityMask;
			}
			else {
				//if (hitbox || TryGetComponent(out hitbox)) hitbox.excludeLayers &= ~EntityMask;
				//if (ground || TryGetComponent(out ground)) ground.excludeLayers &= ~EntityMask;
			}
		}
	}

	public float SenseRange {
		get           => m_SenseRange;
		protected set => m_SenseRange = value;
	}



	public float Speed {
		get           => m_Speed;
		protected set => m_Speed = value;
	}
	public Vector3 Velocity {
		get           => m_Velocity;
		protected set => m_Velocity = value;
	}
	public Vector3 ForcedVelocity {
		get           => m_ForcedVelocity;
		protected set => m_ForcedVelocity = value;
	}
	public Vector3 GroundVelocity {
		get           => m_GroundVelocity;
		protected set => m_GroundVelocity = value;
	}
	public Vector3 GravitVelocity {
		get           => m_GravitVelocity;
		protected set => m_GravitVelocity = value;
	}



	public float Opacity    { get; protected set; } = 1f;
	public bool  IsGrounded { get; protected set; } = false;



    // ================================================================================================
	// Methods
	// ================================================================================================

	protected void FindPath(Vector3 target, ref Queue<Vector3> queue) {
		float offset = NavMeshManager.GetHitboxData(HitboxType).height * 0.5f;
		NavMeshManager.FindPath(transform.position, target, ref queue, offset);
	}


	public virtual InteractionType Interactable(Entity entity) => InteractionType.None;

	public virtual void Interact(Entity entity) {}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	List<Collider> layers          = new List<Collider>();
	bool           layerChanged    = false;
	int            layerMask       = 0;

	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;

	void BeginTrigger() {
		layers.Clear();
		layerChanged = true;
		layerMask = Utility.GetLayerMaskAtPoint(transform.position, transform);
		if (layerMask == 0) layerMask |= CameraManager.ExteriorLayer;
		Opacity = ((CameraManager.CullingMask | layerMask) != 0) ? 1f : 0f;

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
			for(int i = layers.Count - 1; -1 < i; i--) {
				if (layers[i] == null) layers.RemoveAt(i);
				else layerMask |= 1 << layers[i].gameObject.layer;
			}
			if (layerMask == 0) layerMask |= CameraManager.ExteriorLayer;
		}
		if (groundChanged) {
			groundChanged = false;
			for (int i = grounds.Count - 1; -1 < i; i--) {
				if (grounds[i] == null) grounds.RemoveAt(i);
			}
			if (0 < grounds.Count) {
				int j = grounds.Count - 1;
				Utility.TryGetComponentInParent(grounds[j].transform, out groundRigidbody);
				groundRotation  = grounds[j].transform.localRotation;
				IsGrounded      = true;
			}
			else {
				groundRigidbody = null;
				groundRotation  = Quaternion.identity;
				IsGrounded      = false;
			}
		}
	}

	void FixedUpdate() {
		EndTrigger();
		bool visible = (CameraManager.CullingMask & layerMask) != 0;
		if ((visible && Opacity < 1) || (!visible && 0 < Opacity)) {
			Opacity += (visible? 1 : -1) * Time.deltaTime / CameraManager.TransitionTime;
			Opacity = Mathf.Clamp01(Opacity);
		}

		if ((AttributeType & AttributeType.Pinned) != 0) {
			Body.velocity = Vector3.zero;
		}
		else {
			if (groundRigidbody) GroundVelocity = groundRigidbody.velocity;
			if (!IsGrounded && (AttributeType & AttributeType.Floating) == 0) {
				GravitVelocity += Physics.gravity * Time.deltaTime;
			}

			Vector3 linearVelocity = Vector3.zero;
			linearVelocity += groundRotation * Velocity;
			linearVelocity += ForcedVelocity + GroundVelocity + GravitVelocity;
			Body.velocity = linearVelocity;
			
			if (ForcedVelocity != Vector3.zero) {
				ForcedVelocity *= !IsGrounded ? 0.97f : 0.91f;
				if (ForcedVelocity.sqrMagnitude < 0.01f) ForcedVelocity = Vector3.zero;
			}
			if (GroundVelocity != Vector3.zero) {
				GroundVelocity *= !IsGrounded ? 0.97f : 0.91f;
				if (GroundVelocity.sqrMagnitude < 0.01f) GroundVelocity = Vector3.zero;
			}
			if (GravitVelocity != Vector3.zero) {
				if (IsGrounded) GravitVelocity = Vector3.zero;
			}
		}
		if (Body.position.y < -32) Destroy(gameObject);
	}



	protected virtual void Awake() => BeginTrigger();

	protected virtual void LateUpdate() {
		DrawManager.DrawEntity(
			transform.position,
			transform.rotation,
			EntityType,
			MotionType,
			Offset,
			new Color(Color.r, Color.g, Color.b, Color.a * Opacity),
			Intensity);
		DrawManager.DrawShadow(
			transform.position,
			transform.rotation,
			new Vector3(Hitbox.radius * 2f, Hitbox.height * 0.5f, Hitbox.radius * 2f));
		Offset += Time.deltaTime;
	}
}

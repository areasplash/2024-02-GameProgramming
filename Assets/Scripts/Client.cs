using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Client : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField, Range(0f, 1f)] float m_UniqueProb = 0.2f;



	float UniqueProb {
		get => m_UniqueProb;
		set => m_UniqueProb = value;
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(Client))] class CreatureEditor : ExtendedEditor {
			Client I => target as Client;
			public override void OnInspectorGUI() {
				Begin("Client");

				LabelField("Entity", EditorStyles.boldLabel);
				I.EntityType = EnumField ("Entity Type", I.EntityType);
				I.MotionType = EnumField ("Motion Type", I.MotionType);
				I.Offset     = FloatField("Offset",      I.Offset);
				I.Color      = ColorField("Color",       I.Color);
				I.Intensity  = FloatField("Intensity",   I.Intensity);
				Space();
				I.HitboxType    = EnumField("Hitbox Type",    I.HitboxType);
				I.AttributeType = FlagField("Attribute Type", I.AttributeType);
				I.SenseRange    = Slider   ("Sense Range",    I.SenseRange, 0, 32);
				Space();

				LabelField("Rigidbody", EditorStyles.boldLabel);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();

				LabelField("Client", EditorStyles.boldLabel);
				I.UniqueProb = Slider("Unique Probability", I.UniqueProb, 0, 1);

				End();
			}
		}
#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		// if waiting for food
		if ((entity is Player || entity is Staff) && true) return InteractionType.Serve;
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		// if waiting for food
		if ((entity is Player || entity is Staff) && true) {

		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	static Client adventurer;
	static Client knight;
	static Client necromancer;
	static Client priest;
	static Client wizard;	

	void Start() {
		if (UniqueProb < Random.value) switch ((int)(Random.value * 5)) {
			case 0:
				if (adventurer == null) {
					adventurer = this;
					EntityType = EntityType.Adventurer;
				}
			break;
			case 1:
				if (knight == null) {
					knight = this;
					EntityType = EntityType.Knight;
				}
			break;
			case 2:
				if (necromancer == null) {
					necromancer = this;
					EntityType = EntityType.Necromancer;
				}
			break;
			case 3:
				if (priest == null) {
					priest = this;
					EntityType = EntityType.Priest;
				}
			break;
			case 4:
				if (wizard == null) {
					wizard = this;
					EntityType = EntityType.Wizard;
				}
			break;
		}
	}



	enum State {
		
	}
	State state;

	Queue<Vector3> queue = new Queue<Vector3>();

	void Update() {
		switch (state) {

		}
	}
}

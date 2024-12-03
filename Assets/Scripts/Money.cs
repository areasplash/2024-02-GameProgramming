using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Money : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] int m_Value;



	public int Value {
		get => m_Value;
		set {
			m_Value = value;
			EntityType = value switch {
				1 => EntityType.Money1,
				2 => EntityType.Money2,
				3 => EntityType.Money3,
				4 => EntityType.Money4,
				_ => EntityType.Money5,
			};
			Offset = EntityType switch {
				EntityType.Money1 => 0,
				EntityType.Money2 => Random.Range(0, 4),
				EntityType.Money3 => Random.Range(0, 4),
				EntityType.Money4 => Random.Range(0, 4),
				_ => 0,
			};
		}
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(Money))] class CreatureEditor : ExtendedEditor {
			Money I => target as Money;
			public override void OnInspectorGUI() {
				Begin("Money");

				LabelField("Entity", EditorStyles.boldLabel);
				I.EntityType = EnumField ("Entity Type", I.EntityType);
				I.Offset     = FloatField("Offset",      I.Offset);
				Space();
				I.HitboxType    = EnumField("Hitbox Type",    I.HitboxType);
				I.AttributeType = FlagField("Attribute Type", I.AttributeType);
				Space();

				LabelField("Rigidbody", EditorStyles.boldLabel);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();

				LabelField("Money", EditorStyles.boldLabel);
				I.Value = IntSlider("Value", I.Value, 1, 20);

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		return (entity is Player) ? InteractionType.Collect : InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (entity is Player || entity is Staff) {
			// * Particle *
			GameManager.Money += Value;
			Destroy(gameObject);
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	protected override void LateUpdate() {
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
			EntityType != EntityType.Money5 ?
				new Vector3(Hitbox.radius * 2f, Hitbox.height * 0.5f, Hitbox.radius * 2f) :
				new Vector3(Hitbox.radius * 4f, Hitbox.height * 0.5f, Hitbox.radius * 4f));	
	}



	void Update() {
		// * Particle *
	}
}

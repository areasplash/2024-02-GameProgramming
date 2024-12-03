using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Chair : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] Entity m_Occupant;



	public Entity Occupant {
		get         => m_Occupant;
		private set => m_Occupant = value;
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(Chair))] class CreatureEditor : ExtendedEditor {
			Chair I => target as Chair;
			public override void OnInspectorGUI() {
				Begin("Chair");

				LabelField("Entity", EditorStyles.boldLabel);
				I.EntityType = EnumField ("Entity Type", I.EntityType);
				Space();
				I.AttributeType = FlagField("Attribute Type", I.AttributeType);
				Space();

				LabelField("Rigidbody", EditorStyles.boldLabel);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();

				LabelField("Chair", EditorStyles.boldLabel);
				I.Occupant = ObjectField("Occupant", I.Occupant);

				End();
			}
		}
#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		if (entity is Client) {
			if (Occupant == null) return InteractionType.Interact;
			else if (Occupant == entity) return InteractionType.Cancel;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (entity is Client) {
			if (Occupant == null) {
				Occupant = entity;
				entity.transform.position = transform.position + Vector3.up;
			}
			else if (Occupant == entity) {
				Occupant = null;
			}
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	protected override void Awake() {
		int layer = Utility.GetLayerAtPoint(transform.position, transform);
		Stack<GameObject> stack = new Stack<GameObject>();
		stack.Push(gameObject);
		while (0 < stack.Count) {
			GameObject go = stack.Pop();
			go.layer = layer;
			for (int i = 0; i < go.transform.childCount; i++) {
				stack.Push(go.transform.GetChild(i).gameObject);
			}
		}
	}

	protected override void LateUpdate() {}
}

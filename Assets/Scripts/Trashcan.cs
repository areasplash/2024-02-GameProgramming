using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Trashcan : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================

	#if UNITY_EDITOR
		[CustomEditor(typeof(Trashcan))] class CreatureEditor : ExtendedEditor {
			Trashcan I => target as Trashcan;
			public override void OnInspectorGUI() {
				Begin("Trashcan");

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

				LabelField("Trashcan", EditorStyles.boldLabel);

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		if (entity is Player) {
			Player player = entity as Player;
			if (0 < player.Holdings.Count) return InteractionType.Drop;
		}
		if (entity is Staff) {
			Staff staff = entity as Staff;
			if (0 < staff.Holdings.Count) return InteractionType.Drop;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (entity is Player) {
			Player player = entity as Player;
			if (0 < player.Holdings.Count) player.Holdings.RemoveAt(player.Holdings.Count - 1);
		}
		if (entity is Staff) {
			Staff staff = entity as Staff;
			if (0 < staff.Holdings.Count) staff.Holdings.RemoveAt(staff.Holdings.Count - 1);
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void Start() {
		int layer = Utility.GetLayerAtPoint(transform.position, transform);
		Utility.SetLayer(transform, layer);
	}
}

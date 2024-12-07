using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Table : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================
	
	#if UNITY_EDITOR
		[CustomEditor(typeof(Table))] class CreatureEditor : ExtendedEditor {
			Table I => target as Table;
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

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void Start() {
		int layer = Utility.GetLayerAtPoint(transform.position, transform);
		Utility.SetLayer(transform, layer);
	}
}

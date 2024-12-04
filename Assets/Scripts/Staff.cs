using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Staff : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] List<EntityType> m_Holdings = new List<EntityType>();



	public List<EntityType> Holdings {
		get => m_Holdings;
		set => m_Holdings = value;
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(Staff))] class CreatureEditor : ExtendedEditor {
			Staff I => target as Staff;
			public override void OnInspectorGUI() {
				Begin("Staff");

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
				I.Speed          = Slider      ("Speed",           I.Speed, 0, 20);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();

				LabelField("Staff", EditorStyles.boldLabel);
				PropertyField("m_Holdings");
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

	Queue<Vector3> queue = new Queue<Vector3>();
	
}

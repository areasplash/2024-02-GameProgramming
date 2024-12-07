using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Chair : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] float m_TableSearchFreq = 1f;

	[SerializeField] Entity m_Occupant;



	public float TableSearchFreq {
		get => m_TableSearchFreq;
		set => m_TableSearchFreq = value;
	}

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
				I.TableSearchFreq = FloatField("Table Search Frequency", I.TableSearchFreq);
				Space();
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
			if      (table && !Occupant) return InteractionType.Interact;
			else if (Occupant == entity) return InteractionType.Cancel;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (entity is Client) {
			if      (table && !Occupant) Occupant = entity;
			else if (Occupant == entity) Occupant = null;
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void Start() {
		int layer = Utility.GetLayerAtPoint(transform.position, transform);
		Utility.SetLayer(transform, layer);
	}

	public Entity table;

	void Update() {
		if (!UIManager.IsGameRunning) return;
		Offset += Time.deltaTime;
		
		if (!table) {
			bool flag = Utility.Flag(Offset, TableSearchFreq);
			if (flag) Utility.GetMatched(transform.position, 1f, (Entity entity) => {
				return entity is Table;
			}, ref table);
			if (table) {
				Vector3 delta = table.transform.position - transform.position;
				delta = Quaternion.LookRotation(delta).eulerAngles;
				transform.rotation = Quaternion.Euler(0, delta.y, 0);
			}
		}
	}
}

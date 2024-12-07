using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Chest : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] EntityType m_Item;



	public EntityType Item {
		get         => m_Item;
		private set => m_Item = value;
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(Chest))] class CreatureEditor : ExtendedEditor {
			Chest I => target as Chest;
			public override void OnInspectorGUI() {
				Begin("Chest");

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

				LabelField("Chest", EditorStyles.boldLabel);
				I.Item = EnumField("Item Type", I.Item);

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		if (entity is Player) {
			bool exists = (entity as Player).Holdings.Exists(x => x == Item);
			if (exists) return InteractionType.PutIn;
			else        return InteractionType.TakeOut;
		}
		if (entity is Staff) {
			bool exists = (entity as Staff).Holdings.Exists(x => x == Item);
			if (exists) return InteractionType.PutIn;
			else        return InteractionType.TakeOut;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (entity is Player) {
			bool exist = (entity as Player).Holdings.Exists(x => x == Item);
			if (exist) (entity as Player).Holdings.Remove(Item);
			else       (entity as Player).Holdings.Add   (Item);
		}
		if (entity is Staff) {
			bool exist = (entity as Staff).Holdings.Exists(x => x == Item);
			if (exist) (entity as Staff).Holdings.Remove(Item);
			else       (entity as Staff).Holdings.Add   (Item);
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	static List<Chest> list = new List<Chest>();
	public static List<Chest> List => list;

	void OnEnable () => list.Add   (this);
	void OnDisable() => list.Remove(this);

	

	void Start() {
		int layer = Utility.GetLayerAtPoint(transform.position, transform);
		Utility.SetLayer(transform, layer);
	}

	void Update() {
		if (!UIManager.IsGameRunning) return;

		Vector3 position = transform.position + new Vector3(0, 3f, 0);
		DrawManager.DrawEntity(position, EntityType.UIBubble);
		DrawManager.DrawEntity(position, Item);
	}
}

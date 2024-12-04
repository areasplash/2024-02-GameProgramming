using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Chest : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] EntityType m_ItemType;



	public EntityType ItemType {
		get         => m_ItemType;
		private set => m_ItemType = value;
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
				I.ItemType = EnumField("Item Type", I.ItemType);

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		if (entity is Player) {
			bool exist = (entity as Player).Holdings.Exists(x => x == ItemType);
			return exist ? InteractionType.PutIn : InteractionType.TakeOut;
		}
		else if (entity is Staff) {
			bool exist = (entity as Staff).Holdings.Exists(x => x == ItemType);
			return exist ? InteractionType.PutIn : InteractionType.TakeOut;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (entity is Player) {
			bool exist = (entity as Player).Holdings.Exists(x => x == ItemType);
			if (exist) (entity as Player).Holdings.Remove(ItemType);
			else       (entity as Player).Holdings.Add   (ItemType);
		}
		else if (entity is Staff) {
			bool exist = (entity as Staff).Holdings.Exists(x => x == ItemType);
			if (exist) (entity as Staff).Holdings.Remove(ItemType);
			else       (entity as Staff).Holdings.Add   (ItemType);
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

	protected override void LateUpdate() {
		Vector3 position = transform.position + new Vector3(0, 3f, 0);
		DrawManager.DrawEntity(position, EntityType.UIBubble);
		DrawManager.DrawEntity(position, ItemType);
	}
}

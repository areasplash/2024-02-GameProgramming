using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Pot : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] List<EntityType> m_List;



	public List<EntityType> List {
		get         => m_List;
		private set => m_List = value;
	}


	
	#if UNITY_EDITOR
		[CustomEditor(typeof(Pot))] class CreatureEditor : ExtendedEditor {
			Pot I => target as Pot;
			public override void OnInspectorGUI() {
				Begin("Pot");

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

				LabelField("Furnace", EditorStyles.boldLabel);
				PropertyField("m_List");

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		if (entity is Player || entity is Staff) {
			// if cooking
			return InteractionType.Cancel;
			
			// else if Player has a Item
			if ((entity as Player)) return InteractionType.Add;

			// else
			return InteractionType.Cook;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (entity is Player || entity is Staff) {
			// if cooking
			// Cancel
			
			// else if Player has a Item
			// Add Item

			// else
			// Cook Item
			EntityType result = GameManager.GetFoodFromRecipe(List);
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

	

	void Update() {
		
		// draw item list if exist

	}
}

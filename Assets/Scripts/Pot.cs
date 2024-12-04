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
		get => m_List;
		set => m_List = value;
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

		switch (state) {
			case State.Waiting:
				if (entity is Player) {
					bool contains = (entity as Player).Holdings.Contains(EntityType.Food);
					return contains ? InteractionType.Add : InteractionType.Cook;
				}
				else if (entity is Staff) {
					bool contains = (entity as Staff).Holdings.Contains(EntityType.Food);
					return contains ? InteractionType.Add : InteractionType.Cook;
				}
				break;
			case State.Cooking:
				if (entity is Player || entity is Staff) {
					return InteractionType.Cancel;
				}
				break;
			case State.Complete:
				if (entity is Player || entity is Staff) {
					return InteractionType.TakeOut;
				}
				break;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		switch (state) {
			case State.Waiting:
				if (entity is Player) {
					Player player = entity as Player;
					if (player.Holdings.Contains(EntityType.Food)) {
						List.Add(EntityType.Food);
						player.Holdings.Remove(EntityType.Food);
					}
				}
				else if (entity is Staff) {
					Staff staff = entity as Staff;
					if (staff.Holdings.Contains(EntityType.Food)) {
						List.Add(EntityType.Food);
						staff.Holdings.Remove(EntityType.Food);
					}
				}
				break;
			case State.Cooking:
				if (entity is Player || entity is Staff) {
					List.Clear();
					state = State.Waiting;
					Offset = 0f;
				}
				break;
			case State.Complete:
				if (entity is Player) {
					(entity as Player).Holdings.Add(result);
					state = State.Waiting;
				}
				else if (entity is Staff) {
					(entity as Staff).Holdings.Add(result);
					state = State.Waiting;
				}
				break;
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



	const float CookingTime = 10.0f;

	enum State {
		Waiting,
		Cooking,
		Complete,
	}
	State state = State.Waiting;

	EntityType result;

	void Update() {
		switch (state) {
			case State.Waiting:
				// draw exists
				break;
			case State.Cooking:
				Offset += Time.deltaTime / CookingTime;
				Vector3 position = transform.position + new Vector3(0, 3f, 0);
				DrawManager.DrawEntity(position, EntityType.UIBarFill, Mathf.Clamp01(Offset));
				DrawManager.DrawEntity(position, EntityType.UIBarBorder);
				/* Particle */

				if (1.0f <= Offset) {
					result = GameManager.GetFoodFromRecipe(List);
					List.Clear();
					state = State.Complete;
					Offset = 0f;
				}
				break;
			case State.Complete:
				// draw result
				break;
		}
	}
}

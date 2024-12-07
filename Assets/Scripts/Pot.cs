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

	bool IsItem(EntityType type) {
		switch (type) {
			case EntityType.Item:
			case EntityType.ItemPlatter:
			case EntityType.ItemFlour:
			case EntityType.ItemButter:
			case EntityType.ItemCheese:
			case EntityType.ItemBlueberry:
			case EntityType.ItemTomato:
			case EntityType.ItemPotato:
			case EntityType.ItemCabbage:
			case EntityType.ItemMeat:
				return true;
		}
		return false;
	}

	bool IsItemExist(List<EntityType> list) {
		for (int i = 0; i < list.Count; i++) if (IsItem(list[i])) return true;
		return false;
	}



	public override InteractionType Interactable(Entity entity) {
		switch (state) {
			case State.Waiting:
				if (entity is Player) {
					Player player = entity as Player;
					if (IsItemExist(player.Holdings)) {
						bool match = player.Holdings.Exists((EntityType type) => IsItem(type) && !List.Exists((EntityType t) => t == type));
						if (match) return InteractionType.Add;
					}
					else if (0 < List.Count) return InteractionType.Cook;
				}
				else if (entity is Staff) {
					Staff staff = entity as Staff;
					if (IsItemExist(staff.Holdings)) {
						bool match = staff.Holdings.Exists((EntityType type) => IsItem(type) && !List.Exists((EntityType t) => t == type));
						if (match) return InteractionType.Add;
					}
					else if (0 < List.Count) return InteractionType.Cook;
				}
				break;
			case State.Cooking:
				if (entity is Player || entity is Staff) return InteractionType.Cancel;
				break;
			case State.Success:
				if (entity is Player || entity is Staff) return InteractionType.TakeOut;
				break;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		switch (state) {
			case State.Waiting:
				if (entity is Player) {
					Player player = entity as Player;
					if (IsItemExist(player.Holdings)) {
						for (int i = player.Holdings.Count - 1; -1 < i; i--) {
							bool exists = List.Exists((EntityType type) => type == player.Holdings[i]);
							if (IsItem(player.Holdings[i]) && !exists) {
								List.Add(player.Holdings[i]);
								player.Holdings.RemoveAt(i);
								break;
							}
						}
					}
					else {
						state = State.Cooking;
						Offset = 0f;
					}
				}
				else if (entity is Staff) {
					Staff staff = entity as Staff;
					if (IsItemExist(staff.Holdings)) {
						for (int i = staff.Holdings.Count - 1; -1 < i; i--) {
							bool exists = List.Exists((EntityType type) => type == staff.Holdings[i]);
							if (IsItem(staff.Holdings[i]) && !exists) {
								List.Add(staff.Holdings[i]);
								staff.Holdings.RemoveAt(i);
								break;
							}
						}
					}
					else {
						state = State.Cooking;
						Offset = 0f;
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
			case State.Success:
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



	const float CookingTime   = 3.0f;
	const float RestoringTime = 3.0f;

	enum State {
		Waiting,
		Cooking,
		Success,
		Failure,
	}
	State state = State.Waiting;

	EntityType result;

	void Update() {
		switch (state) {
			case State.Waiting:
				for (int i = 0; i < List.Count; i++) {
					DrawManager.DrawEntity(transform.position + new Vector3(0, 3f + i * 0.5f, 0), List[i]);
				}
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
					state = (result != EntityType.None) ? State.Success : State.Failure;
					Offset = 0f;
				}
				break;
			case State.Success:
				DrawManager.DrawEntity(transform.position + new Vector3(0, 3f, 0), result);
				break;
			case State.Failure:
				Offset += Time.deltaTime;
				if (RestoringTime < Offset) {
					state = State.Waiting;
					Offset = 0f;
				}
				break;
		}
	}
}

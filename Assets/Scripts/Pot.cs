using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Pot : Entity {
    
	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] float m_CookingTime = 3.0f;

	[SerializeField] List<EntityType> m_Holdings;



	public float CookingTime {
		get => m_CookingTime;
		set => m_CookingTime = value;
	}

	public List<EntityType> Holdings {
		get => m_Holdings;
		set => m_Holdings = value;
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
				I.CookingTime = FloatField("Cooking Time", I.CookingTime);
				Space();
				PropertyField("m_Holdings");

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	bool IsItem(EntityType type) => type switch {
		EntityType.Item          => true,
		EntityType.ItemPlatter   => true,
		EntityType.ItemFlour     => true,
		EntityType.ItemButter    => true,
		EntityType.ItemCheese    => true,
		EntityType.ItemBlueberry => true,
		EntityType.ItemTomato    => true,
		EntityType.ItemPotato    => true,
		EntityType.ItemCabbage   => true,
		EntityType.ItemMeat      => true,
		_ => false,
	};



	public override InteractionType Interactable(Entity entity) {
		switch (state) {
			case State.Waiting:
				if (entity is Player) {
					Player player = entity as Player;
					int index = player.Holdings.FindIndex((EntityType type)
						=> IsItem(type) && Holdings.IndexOf(type) == -1);
					if (index != -1) return InteractionType.Add;
					else if (0 < Holdings.Count) return InteractionType.Cook;
				}
				if (entity is Staff) {
					Staff staff = entity as Staff;
					int index = staff.Holdings.FindIndex((EntityType type)
						=> IsItem(type) && Holdings.IndexOf(type) == -1);
					if (index != -1) return InteractionType.Add;
					else if (0 < Holdings.Count) return InteractionType.Cook;
				}
				break;
			case State.Cooking:
				if (entity is Player) return InteractionType.Cancel;
				if (entity is Staff ) return InteractionType.Cancel;
				break;
			case State.Success:
				if (entity is Player) return InteractionType.TakeOut;
				if (entity is Staff ) return InteractionType.TakeOut;
				break;
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		switch (state) {
			case State.Waiting:
				if (entity is Player) {
					Player player = entity as Player;
					int index = player.Holdings.FindIndex((EntityType type)
						=> IsItem(type) && Holdings.IndexOf(type) == -1);
					if (index != -1) {
						Holdings.Add(player.Holdings[index]);
						player.Holdings.RemoveAt(index);
					}
					else if (0 < Holdings.Count) {
						state = State.Cooking;
						Offset = 0f;
					}
				}
				else if (entity is Staff) {
					Staff staff = entity as Staff;
					int index = staff.Holdings.FindIndex((EntityType type)
						=> IsItem(type) && Holdings.IndexOf(type) == -1);
					if (index != -1) {
						Holdings.Add(staff.Holdings[index]);
						staff.Holdings.RemoveAt(index);
					}
					else if (0 < Holdings.Count) {
						state = State.Cooking;
						Offset = 0f;
					}
				}
				break;
			case State.Cooking:
				if (entity is Player) {
					Holdings.Clear();
					state = State.Waiting;
					Offset = 0f;
				}
				if (entity is Staff) {
					Holdings.Clear();
					state = State.Waiting;
					Offset = 0f;
				}
				break;
			case State.Success:
				if (entity is Player) {
					(entity as Player).Holdings.Add(Holdings[0]);
					Holdings.Clear();
					state = State.Waiting;
					Offset = 0f;
				}
				if (entity is Staff) {
					(entity as Staff).Holdings.Add(Holdings[0]);
					Holdings.Clear();
					state = State.Waiting;
					Offset = 0f;
				}
				break;
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	static List<Pot> list = new List<Pot>();
	public static List<Pot> List => list;

	void OnEnable () => list.Add   (this);
	void OnDisable() => list.Remove(this);



	void Start() {
		int layer = Utility.GetLayerAtPoint(transform.position, transform);
		Utility.SetLayer(transform, layer);
	}



	enum State {
		Waiting,
		Cooking,
		Success,
		Failure,
	}
	State state = State.Waiting;

	void Update() {
		if (!UIManager.IsGameRunning) return;
		Offset += Time.deltaTime;

		switch (state) {
			case State.Waiting:
				for (int i = 0; i < Holdings.Count; i++) {
					float distance = Holdings.Count / Mathf.Sqrt(Holdings.Count) - 1;
					float angle = 2 * Mathf.PI * i / Holdings.Count;
					float x = Mathf.Cos(angle) * distance;
					float z = Mathf.Sin(angle) * distance;
					DrawManager.DrawEntity(transform.position + new Vector3(x, 3f, z), Holdings[i]);
				}
				break;

			case State.Cooking:
				for (int i = 0; i < Holdings.Count; i++) {
					float distance = Holdings.Count / Mathf.Sqrt(Holdings.Count) - 1;
					float angle = 2 * Mathf.PI * i / Holdings.Count;
					float x = Mathf.Cos(angle) * distance;
					float z = Mathf.Sin(angle) * distance;
					DrawManager.DrawEntity(transform.position + new Vector3(x, 3f, z), Holdings[i]);
				}
				Vector3 position = transform.position + new Vector3(0, 4f, 0);
				DrawManager.DrawEntity(position, EntityType.UIBarFill, Mathf.Clamp01(Offset / CookingTime));
				DrawManager.DrawEntity(position, EntityType.UIBarBorder);
				if (CookingTime < Offset) {
					EntityType result = GameManager.GetFoodFromRecipe(Holdings);
					Holdings.Clear();
					Holdings.Add(result);
					state = (result != EntityType.None) ? State.Success : State.Failure;
					Offset = 0f;
				}
				break;

			case State.Success:
				DrawManager.DrawEntity(transform.position + new Vector3(0, 3f, 0), Holdings[0]);
				break;

			case State.Failure:
				state = State.Waiting;
				Offset = 0f;
				break;
		}
	}
}

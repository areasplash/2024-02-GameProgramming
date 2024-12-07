using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Client : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] float m_UniqueClientProb =  0.2f;

	[SerializeField] float m_ChairSearchFreq  = 10.0f;
	[SerializeField] float m_EntryWaitingTime =  5.0f;
	[SerializeField] float m_MenuChoosingTime =  2.0f;
	[SerializeField] float m_MenuWaitingTime  = 30.0f;
	[SerializeField] float m_EatingTime       = 10.0f;
	[SerializeField] float m_EatAgainProb     =  0.1f;



	float UniqueClientProb {
		get => m_UniqueClientProb;
		set => m_UniqueClientProb = value;
	}

	public float ChairSearchFreq {
		get => m_ChairSearchFreq;
		set => m_ChairSearchFreq = value;
	}
	float EntryWaitingTime {
		get => m_EntryWaitingTime;
		set => m_EntryWaitingTime = value;
	}
	float MenuChoosingTime {
		get => m_MenuChoosingTime;
		set => m_MenuChoosingTime = value;
	}
	float MenuWaitingTime {
		get => m_MenuWaitingTime;
		set => m_MenuWaitingTime = value;
	}
	float EatingTime {
		get => m_EatingTime;
		set => m_EatingTime = value;
	}
	float EatAgainProb {
		get => m_EatAgainProb;
		set => m_EatAgainProb = value;
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(Client))] class CreatureEditor : ExtendedEditor {
			Client I => target as Client;
			public override void OnInspectorGUI() {
				Begin("Client");

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

				LabelField("Client", EditorStyles.boldLabel);
				I.UniqueClientProb = FloatField("Unique Client Probability", I.UniqueClientProb);
				Space();
				I.ChairSearchFreq  = FloatField("Chair Search Frequency",    I.ChairSearchFreq);
				I.EntryWaitingTime = FloatField("Entry Waiting Time",        I.EntryWaitingTime);
				I.MenuChoosingTime = FloatField("Menu Choosing Time",        I.MenuChoosingTime);
				I.MenuWaitingTime  = FloatField("Menu Waiting Time",         I.MenuWaitingTime);
				I.EatingTime       = FloatField("Eating Time",               I.EatingTime);
				I.EatAgainProb     = FloatField("Eat Again Probability",     I.EatAgainProb);
				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public override InteractionType Interactable(Entity entity) {
		if (state == State.MenuWaiting) {
			if (entity is Player) {
				Player player = entity as Player;
				int index = player.Holdings.IndexOf(order);
				if (index != -1) return InteractionType.Serve;
			}
			if (entity is Staff) {
				Staff staff = entity as Staff;
				int index = staff.Holdings.IndexOf(order);
				if (index != -1) return InteractionType.Serve;
			}
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (state == State.MenuWaiting) {
			if (entity is Player) {
				Player player = entity as Player;
				int index = player.Holdings.IndexOf(order);
				if (index != -1) {
					player.Holdings.RemoveAt(index);
					state = State.Eating;
					Offset = 0f;
				}
				pay += GameManager.Price[order];
			}
			if (entity is Staff) {
				Staff staff = entity as Staff;
				int index = staff.Holdings.IndexOf(order);
				if (index != -1) {
					staff.Holdings.RemoveAt(index);
					state = State.Eating;
					Offset = 0f;
				}
				pay += GameManager.Price[order];
			}
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	static List<Client> list = new List<Client>();
	public static List<Client> List => list;

	void OnEnable () => list.Add   (this);
	void OnDisable() => list.Remove(this);



	static Client adventurer;
	static Client knight;
	static Client necromancer;
	static Client priest;
	static Client wizard;

	void Start() {
		if (UniqueClientProb <= Random.value) switch ((int)(Random.value * 5)) {
			case 0: if (!adventurer ) EntityType = EntityType.Adventurer;  break;
			case 1: if (!knight     ) EntityType = EntityType.Knight;      break;
			case 2: if (!necromancer) EntityType = EntityType.Necromancer; break;
			case 3: if (!priest     ) EntityType = EntityType.Priest;      break;
			case 4: if (!wizard     ) EntityType = EntityType.Wizard;      break;
		}
		switch (EntityType) {
			case EntityType.Adventurer:  adventurer  = this; break;
			case EntityType.Knight:      knight      = this; break;
			case EntityType.Necromancer: necromancer = this; break;
			case EntityType.Priest:      priest      = this; break;
			case EntityType.Wizard:      wizard      = this; break;
		}
	}



	public enum State {
		EntryWaiting,
		Seating,
		MenuChoosing,
		MenuWaiting,
		Eating,
		BeginExiting,
		Exiting,
	}
	public State state = State.EntryWaiting;

	Queue<Vector3> queue = new Queue<Vector3>();
	Vector3 positionTemp;

	Entity chair;
	public EntityType order;
	float reputationWeight;
	int pay;

	void Update() {
		if (!UIManager.IsGameRunning) return;
		Offset += Time.deltaTime;

		float y = NavMeshManager.GetHitboxData(HitboxType).height * 0.5f;
		Vector3 bubblePosition = transform.position + new Vector3(0, y + 2f, 0);

		// State

		switch (state) {
			case State.EntryWaiting:
				bool flag = Utility.Flag(Offset, ChairSearchFreq); 
				if (flag) Utility.GetMatched(transform.position, 128f, (Entity entity) => {
					return entity is Chair && entity.Interactable(this) == InteractionType.Interact;
				}, ref chair);
				if (EntryWaitingTime < Offset) {
					state = State.BeginExiting;
					Offset = 0f;
				}
				else if (chair) {
					chair.Interact(this);
					FindPath(chair.transform.position, ref queue);
					state = State.Seating;
					Offset = 0f;
				}
				break;

			case State.Seating:
				if (Vector3.Distance(chair.transform.position, transform.position) < SenseRange) {
					queue.Clear();
					positionTemp = transform.position;
					transform.position = chair.transform.position + new Vector3(0, 0.5f + y, 0);
					transform.rotation = chair.transform.rotation;
					AttributeType |= AttributeType.Pinned;
					state = State.MenuChoosing;
					Offset = 0f;
				}
				break;

			case State.MenuChoosing:
				DrawManager.DrawEntity(bubblePosition, EntityType.UIBubble);
				DrawManager.DrawEntity(bubblePosition, EntityType.UILoading, Offset);
				if (MenuChoosingTime < Offset) {
					order = Random.Range(0, 9) switch {
						0 => EntityType.FoodPancake,
						1 => EntityType.FoodCheeseCake,
						2 => EntityType.FoodSpaghetti,
						3 => EntityType.FoodSoup,
						4 => EntityType.FoodSandwich,
						5 => EntityType.FoodSalad,
						6 => EntityType.FoodSteak,
						7 => EntityType.FoodWine,
						8 => EntityType.FoodBeer,
						_ => EntityType.FoodBeer,
					};
					state = State.MenuWaiting;
					Offset = 0f;
				}
				break;

			case State.MenuWaiting:
				DrawManager.DrawEntity(bubblePosition, EntityType.UIBubble);
				DrawManager.DrawEntity(bubblePosition, order);
				if (MenuWaitingTime < Offset) {
					reputationWeight -= 0.5f;
					state = State.BeginExiting;
					Offset = 0f;
				}
				break;

			case State.Eating:
				bool isDrinking = order == EntityType.FoodWine || order == EntityType.FoodBeer;
				EntityType UI = isDrinking ? EntityType.UIDrinking : EntityType.UIEating;
				DrawManager.DrawEntity(bubblePosition, EntityType.UIBubble);
				DrawManager.DrawEntity(bubblePosition, UI, Offset);
				DrawManager.DrawEntity(transform.position + transform.forward * 0.5f, order);
				if (EatingTime < Offset) {
					if (Random.value < EatAgainProb) {
						state = State.MenuChoosing;
						Offset = 0f;
					}
					else {
						reputationWeight += 0.5f;
						state = State.BeginExiting;
						Offset = 0f;
					}
				}
				break;
			
			case State.BeginExiting:
				Vector3 position = transform.position;
				if (chair) {
					chair.Interact(this);
					transform.position = positionTemp;
					AttributeType &= ~AttributeType.Pinned;
					position = (chair as Chair).table.transform.position + Vector3.up * 1.5f;
					float r = Random.value * 360f * Mathf.Deg2Rad;
					float d = Random.value;
					position += new Vector3(Mathf.Cos(r) * d, 0, Mathf.Sin(r) * d);
				}
				if (0 < pay) GameManager.SpawnMoney(position).Value = pay;
				FindPath(GameManager.ClientSpawnPoint, ref queue);
				state = State.Exiting;
				Offset = 0f;
				break;

			case State.Exiting:
				if (reputationWeight < 0) {
					DrawManager.DrawEntity(bubblePosition, EntityType.UIBubble);
					DrawManager.DrawEntity(bubblePosition, EntityType.UIBad);
				}
				if (queue.Count == 0) {
					GameManager.UpdateReputation(reputationWeight);
					Destroy(gameObject);
				}
				break;
		}

		if (GameManager.CloseHour <= GameManager.Hour) switch (state) {
			case State.EntryWaiting:
			case State.Seating:
			case State.MenuChoosing:
			case State.MenuWaiting:
			case State.Eating:
				state = State.BeginExiting;
				Offset = 0f;
				break;
		}

		// Movement

		if (queue.Count == 0) {
			if (MotionType != MotionType.Idle) {
				MotionType  = MotionType.Idle;
				Offset = 0;
			}
		}
		else {
			if (MotionType != MotionType.Move) {
				MotionType  = MotionType.Move;
				Offset = 0;
			}
			Vector3 delta = queue.Peek() - transform.position;
			Velocity = new Vector3(delta.x, 0, delta.z).normalized * Speed;
			if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.02f) queue.Dequeue();
			if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);

			float radius = NavMeshManager.GetHitboxData(HitboxType).radius;
			if (Vector3.Distance(transform.position, positionTemp) < Speed * 0.5f * Time.deltaTime) {
				Hitbox.radius = Mathf.Max(0.05f, Hitbox.radius - 0.02f * Time.deltaTime);
				Ground.center = new Vector3(0, -(Hitbox.height / 2 - Hitbox.radius) - 0.08f, 0);
				Ground.radius = Hitbox.radius - 0.04f;
			}
			else if (Hitbox.radius < radius) {
				Hitbox.radius = Mathf.Min(radius, Hitbox.radius + 0.02f * Time.deltaTime);
				Ground.center = new Vector3(0, -(Hitbox.height / 2 - Hitbox.radius) - 0.08f, 0);
				Ground.radius = Hitbox.radius - 0.04f;
			}
			positionTemp = transform.position;

			#if UNITY_EDITOR
				Vector3[] array = queue.ToArray();
				for (int i = 0; i < array.Length - 1; i++) {
					Debug.DrawLine(array[i], array[i + 1], Color.red);
				}
			#endif
		}

		// Draw

		DrawManager.DrawEntity(
			transform.position,
			transform.rotation,
			EntityType,
			MotionType,
			Offset,
			new Color(Color.r, Color.g, Color.b, Color.a * Opacity),
			Intensity);
		DrawManager.DrawShadow(
			transform.position,
			transform.rotation,
			new Vector3(Hitbox.radius * 2f, Hitbox.height * 0.5f, Hitbox.radius * 2f));
	}
}

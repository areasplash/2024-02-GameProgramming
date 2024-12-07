using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Client : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField, Range(0f, 1f)] float m_UniqueProb = 0.2f;



	float UniqueProb {
		get => m_UniqueProb;
		set => m_UniqueProb = value;
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
				I.UniqueProb = Slider("Unique Probability", I.UniqueProb, 0, 1);

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
				for (int i = 0; i < (entity as Player).Holdings.Count; i++) {
					if (menu == (entity as Player).Holdings[i]) return InteractionType.Serve;
				}
			}
			else if (entity is Staff) {
				for (int i = 0; i < (entity as Staff).Holdings.Count; i++) {
					if (menu == (entity as Staff).Holdings[i]) return InteractionType.Serve;
				}
			}
		}
		return InteractionType.None;
	}

	public override void Interact(Entity entity) {
		if (state == State.MenuWaiting) {
			if (entity is Player) {
				for (int i = 0; i < (entity as Player).Holdings.Count; i++) {
					if (menu == (entity as Player).Holdings[i]) {
						(entity as Player).Holdings.RemoveAt(i);
						state = State.Eating;
						Offset = 0f;
						break;
					}
				}
			}
			else if (entity is Staff) {
				for (int i = 0; i < (entity as Staff).Holdings.Count; i++) {
					if (menu == (entity as Staff).Holdings[i]) {
						(entity as Staff).Holdings.RemoveAt(i);
						state = State.Eating;
						Offset = 0f;
						break;
					}
				}
			}
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	static Client adventurer;
	static Client knight;
	static Client necromancer;
	static Client priest;
	static Client wizard;	

	void Start() {
		if (UniqueProb < Random.value) switch ((int)(Random.value * 5)) {
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



	const float EntryWaitingTime        =  5.0f;
	const float MinimumMenuChoosingTime =  2.0f;
	const float MenuWaitingTime         = 30.0f;
	const float EatingTime              =  5.0f;
	const float EatAgainProb            =  0.2f;

	enum State {
		EntryWaiting,
		Seating,
		MenuChoosing,
		MenuWaiting,
		Eating,
		Exiting,
	}
	State state = State.EntryWaiting;

	Queue<Vector3> queue = new Queue<Vector3>();

	Entity chair;
	EntityType menu;
	float reputationWeight;
	int moneyToPay;

	void Update() {
		#if UNITY_EDITOR
			Vector3[] array = queue.ToArray();
			for (int i = 0; i < array.Length - 1; i++) {
				Debug.DrawLine(array[i], array[i + 1], Color.red);
			}
		#endif

		switch (state) {
			case State.EntryWaiting:
				MotionType = MotionType.Idle;

				bool flag0 = (int)(Offset * 10) != (int)((Offset - Time.deltaTime) * 10);
				if (flag0) Utility.GetMatched(transform.position, 128f, (Entity entity) => {
					return entity is Chair && entity.Interactable(this) == InteractionType.Interact;
				}, ref chair);

				if (chair) {
					chair.Interact(this);
					FindPath(chair.transform.position, ref queue);
					state = State.Seating;
					Offset = 0f;
				}
				if (EntryWaitingTime < Offset) {
					reputationWeight = -0.1f;
					GameManager.UpdateReputation(reputationWeight);
					Destroy(gameObject);
				}
				break;
			case State.Seating:
				MotionType = MotionType.Move;

				if (SenseRange < Vector3.Distance(chair.transform.position, transform.position)) {
					Vector3 delta = queue.Peek() - transform.position;
					Velocity = new Vector3(delta.x, 0, delta.z).normalized * Speed;
					if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.02f) queue.Dequeue();
					if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);
				}

				else {
					queue.Clear();
					queue.Enqueue(transform.position);
					float y0 = NavMeshManager.GetHitboxData(HitboxType).height * 0.5f + 0.5f;
					transform.position = chair.transform.position + new Vector3(0, y0, 0);
					AttributeType |= AttributeType.Pinned;
					state = State.MenuChoosing;
					Offset = 0f;
				}
				break;
			case State.MenuChoosing:
				MotionType = MotionType.Idle;

				float y1 = NavMeshManager.GetHitboxData(HitboxType).height * 0.5f + 2f;
				Vector3 bubblePosition1 = transform.position + new Vector3(0, y1, 0);
				DrawManager.DrawEntity(bubblePosition1, EntityType.UIBubble);
				DrawManager.DrawEntity(bubblePosition1, EntityType.UILoading, Offset);

				bool flag1 = (int)(Offset * 10) != (int)((Offset - Time.deltaTime) * 10);
				if (MinimumMenuChoosingTime < Offset && flag1 && Random.value < 0.1f) {
					menu = Random.Range(0, 7) switch {
						0 => EntityType.FoodPancake,
						1 => EntityType.FoodCheeseCake,
						2 => EntityType.FoodSpaghetti,
						3 => EntityType.FoodSoup,
						4 => EntityType.FoodSandwich,
						5 => EntityType.FoodSalad,
						6 => EntityType.FoodSteak,
						7 => EntityType.FoodBeer,
						8 => EntityType.FoodWine,
						_ => EntityType.FoodWine,
					};
					state = State.MenuWaiting;
					Offset = 0f;
				}
				break;
			case State.MenuWaiting:
				MotionType = MotionType.Idle;

				float y2 = NavMeshManager.GetHitboxData(HitboxType).height * 0.5f + 2f;
				Vector3 bubblePosition2 = transform.position + new Vector3(0, y2, 0);
				DrawManager.DrawEntity(bubblePosition2, EntityType.UIBubble);
				DrawManager.DrawEntity(bubblePosition2, menu);

				if (MenuWaitingTime < Offset) {
					if (0 < moneyToPay) {
						moneyToPay = Random.Range(1, 6);
						Instantiate(GameManager.PrefabMoney, transform.position, Quaternion.identity).TryGetComponent(out Money money);
						money.Value = moneyToPay;
					}
					chair.Interact(this);
					transform.position = queue.Peek();
					FindPath(GameManager.ClientSpawnPoint, ref queue);
					AttributeType &= ~AttributeType.Pinned;
					state = State.Exiting;
					Offset = 0f;
					reputationWeight -= 0.4f;
				}
				break;
			case State.Eating:
				MotionType = MotionType.Idle;

				float y3 = NavMeshManager.GetHitboxData(HitboxType).height * 0.5f + 2f;
				Vector3 bubblePosition3 = transform.position + new Vector3(0, y3, 0);
				DrawManager.DrawEntity(bubblePosition3, EntityType.UIBubble);
				DrawManager.DrawEntity(bubblePosition3, EntityType.UIEating);
				DrawManager.DrawEntity(transform.position + transform.forward * 0.5f, menu);
				if (EatingTime < Offset) {
					if (Random.value < EatAgainProb) {
						menu = Random.Range(0, 9) switch {
							0 => EntityType.FoodPancake,
							1 => EntityType.FoodCheeseCake,
							2 => EntityType.FoodSpaghetti,
							3 => EntityType.FoodSoup,
							4 => EntityType.FoodSandwich,
							5 => EntityType.FoodSalad,
							6 => EntityType.FoodSteak,
							7 => EntityType.FoodBeer,
							8 => EntityType.FoodWine,
							_ => EntityType.FoodWine,
						};
						state = State.MenuWaiting;
						Offset = 0f;
						reputationWeight += 0.1f;
					}
					else {
						if (0 < moneyToPay) {
							moneyToPay = Random.Range(1, 6);
							Instantiate(GameManager.PrefabMoney, transform.position, Quaternion.identity).TryGetComponent(out Money money);
							money.Value = moneyToPay;
						}
						chair.Interact(this);
						transform.position = queue.Peek();
						FindPath(GameManager.ClientSpawnPoint, ref queue);
						AttributeType &= ~AttributeType.Pinned;
						state = State.Exiting;
						Offset = 0f;
					}
				}
				break;
			case State.Exiting:
				MotionType = MotionType.Move;

				if (0 < queue.Count) {
					Vector3 delta = queue.Peek() - transform.position;
					Velocity = new Vector3(delta.x, 0, delta.z).normalized * Speed;
					if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.02f) queue.Dequeue();
					if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);
				}

				else {
					GameManager.UpdateReputation(reputationWeight);
					Destroy(gameObject);
				}
				break;
		}
	}
}

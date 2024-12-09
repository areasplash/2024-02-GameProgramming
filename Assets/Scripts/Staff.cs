using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Staff : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] float m_CustomerCheckFreq = 10.0f;

	[SerializeField] List<EntityType> m_Holdings = new List<EntityType>();



	public float CustomerCheckFreq {
		get => m_CustomerCheckFreq;
		set => m_CustomerCheckFreq = value;
	}

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
				I.CustomerCheckFreq = FloatField("Customer Check Frequency", I.CustomerCheckFreq);
				Space();
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

	static List<Staff> list = new List<Staff>();
	public static List<Staff> List => list;

	void OnEnable () => list.Add   (this);
	void OnDisable() => list.Remove(this);



	public enum State {
		Waiting,
		ChestChecking,
		ItemTaking,
		Cooking,
		PotWaiting,
		Serving,
	}
	public State state = State.Waiting;

	Queue<Vector3> queue = new Queue<Vector3>();

	Customer customer;
	Pot pot;
	Queue<Chest> chests = new Queue<Chest>();

	void Update() {
		if (!UIManager.IsGameRunning) return;
		Offset += Time.deltaTime;

		// State

		if (state != State.Waiting) {
			if (customer == null) {
				Holdings.Clear();
				customer = null;
				pot = null;
				chests.Clear();
				state = State.Waiting;
				Offset = 0f;
			}
		}

		switch (state) {
			case State.Waiting:
				if (Utility.Flag(Offset, CustomerCheckFreq)) {
					for (int i = 0; i < Customer.List.Count; i++) {
						if (Customer.List[i].state == Customer.State.MenuWaiting) {
							bool pass = false;
							for (int j = 0; j < List.Count; j++) {
								if (List[j].customer == Customer.List[i]) {
									pass = true;
									break;
								}
							}
							if (pass) continue;
							customer = Customer.List[i];
							state = State.ChestChecking;
							Offset = 0f;
							break;
						}
						if (Customer.List[i].state == Customer.State.PayWaiting) {
							bool pass = false;
							for (int j = 0; j < List.Count; j++) {
								if (List[j].customer == Customer.List[i]) {
									pass = true;
									break;
								}
							}
							if (pass) continue;
							customer = Customer.List[i];
							FindPath(customer.transform.position, ref queue);
							state = State.Serving;
							Offset = 0f;
							break;
						}
					}
				}
				break;
			
			case State.ChestChecking:
				if (GameManager.Recipe.TryGetValue(customer.order, out List<EntityType> recipe)) {
					for (int i = 0; i < Chest.List.Count; i++) {
						if (recipe.Contains(Chest.List[i].Item)) chests.Enqueue(Chest.List[i]);
					}
					if (chests.Count == recipe.Count) {
						state = State.ItemTaking;
						Offset = 0f;
						break;
					}
				}
				else {
					for (int i = 0; i < Chest.List.Count; i++) {
						if (Chest.List[i].Item == customer.order) {
							chests.Enqueue(Chest.List[i]);
							break;
						}
					}
					if (0 < chests.Count) {
						state = State.ItemTaking;
						Offset = 0f;
						break;
					}
				}
				chests.Clear();
				state = State.Waiting;
				Offset = 0f;
				break;
			
			case State.ItemTaking:
				if (chests.Count != 0) {
					Chest chest = chests.Peek();
					if (queue.Count == 0) FindPath(chest.transform.position, ref queue);
					if (Vector3.Distance(transform.position, chest.transform.position) < SenseRange) {
						queue.Clear();
						chest.Interact(this);
						chests.Dequeue();
					}
				}
				else {
					if (Holdings.Contains(customer.order)) {
						FindPath(customer.transform.position, ref queue);
						state = State.Serving;
						Offset = 0f;
					}
					else {
						state = State.Cooking;
						Offset = 0f;
					}
				}
				break;

			case State.Cooking:
				if (pot == null) for (int i = 0; i < Pot.List.Count; i++) {
					if (Pot.List[i].Holdings.Count == 0) {
						pot = Pot.List[i];
						FindPath(pot.transform.position, ref queue);
						break;
					}
				}
				else if (0 < Holdings.Count) {
					if (Vector3.Distance(transform.position, pot.transform.position) < SenseRange) {
						if (pot.Holdings.Count != 0) pot = null;
						else {
							queue.Clear();
							int count = GameManager.Recipe[customer.order].Count;
							for (int i = 0; i < count; i++) pot.Interact(this);
							pot.Interact(this);
							state = State.PotWaiting;
							Offset = 0f;
						}
					}
				}
				break;
			
			case State.PotWaiting:
				switch (pot.Interactable(this)) {
					case InteractionType.TakeOut:
						pot.Interact(this);
						pot = null;
						FindPath(customer.transform.position, ref queue);
						state = State.Serving;
						Offset = 0f;
						break;
					case InteractionType.None:
						pot = null;
						state = State.Waiting;
						Offset = 0f;
						break;
				}
				break;
				
			case State.Serving:
				if (Vector3.Distance(transform.position, customer.transform.position) < SenseRange) {
					queue.Clear();
					customer.Interact(this);
					customer = null;
					state = State.Waiting;
					Offset = 0f;
				}
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
			if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.04f) queue.Dequeue();
			if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);

			#if UNITY_EDITOR
				Vector3[] array = queue.ToArray();
				for (int i = 0; i < array.Length - 1; i++) {
					Debug.DrawLine(array[i], array[i + 1], Color.red);
				}
			#endif
		}

		if (0 < Holdings.Count) {
			Vector3 position = transform.position + transform.forward * 0.5f;
			for (int i = 0; i < Holdings.Count; i++) {
				DrawManager.DrawEntity(position + Vector3.up * i * 0.5f, Holdings[i]);
			}
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

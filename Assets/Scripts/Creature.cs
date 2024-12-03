using UnityEngine;
using UnityEngine.AI;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



[Serializable] public enum CreatureType {
	None,
	Player,
	Client,
	Adventurer,
	Knight,
	Necromancer,
	Priest,
	Wizard,

	Money,

	ItemPlatter,
	ItemFlour,
	ItemButter,
	ItemCheese,
	ItemBlueberry,
	ItemTomato,
	ItemPotato,
	ItemCabbage,
	ItemMeat,

	FoodFancake,
	FoodCheeseCake,
	FoodSpaghetti,
	FoodSoup,
	FoodSandwich,
	FoodSalad,
	FoodSteak,
	FoodWine,
	FoodBeer,
}

[Serializable] public enum AnimationType {
	Idle,
	Move,
	Serve,
	Seat,
	Eat,
	Drink,
}

[Serializable, Flags] public enum AttributeType {
	Pinned   = 1 <<  0,
	Floating = 1 <<  1,
	Piercing = 1 <<  2,

	Interact = 1 << 16,
	Open     = 1 << 17,
	Close    = 1 << 18,
	Retrieve = 1 << 19,
	Discard  = 1 << 20,
	Cook     = 1 << 21,
	Serve    = 1 << 22,
}

[Serializable] public enum ImmunityType {
	None,
	Low,
	Partial,
	Full,
}

[Serializable] public struct Effect {
	float        m_strength;
	float        m_duration;
	ImmunityType m_immunity;

	public float        strength { get => m_strength; set => m_strength = Mathf.Max(0, value); }
	public float        duration { get => m_duration; set => m_duration = Mathf.Max(0, value); }
	public ImmunityType immunity { get => m_immunity; set => m_immunity = value; }

	public Effect(float strength = 0, float duration = 0, ImmunityType immunity = ImmunityType.None) {
		m_strength = Mathf.Max(0, strength);
		m_duration = Mathf.Max(0, duration);
		m_immunity = immunity;
	}
}

[Serializable] public struct Status {
	float m_limit;
	float m_value;

	public float limit { get => m_limit; set => m_limit = Mathf.Max(    0, value); }
	public float value { get => m_value; set => m_value = Mathf.Min(value, limit); }

	public Status(float limit = 0, float value = 0) {
		m_limit = Mathf.Max(    0,   limit);
		m_value = Mathf.Min(value, m_limit);
	}
}



public class Creature : MonoBehaviour {

	const string PrefabPath = "Prefabs/Creature";



	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] CreatureType  m_CreatureType  = CreatureType.None;
	[SerializeField] AnimationType m_AnimationType = AnimationType.Idle;
	[SerializeField] float         m_Offset        = 0;
	[SerializeField] AttributeType m_AttributeType = 0;
	[SerializeField] HitboxType    m_HitboxType    = HitboxType.Humanoid;
	[SerializeField] float         m_SenseRange    = 0;

	[SerializeField] Vector3 m_Velocity       = Vector3.zero;
	[SerializeField] Vector3 m_ForcedVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GroundVelocity = Vector3.zero;
	[SerializeField] Vector3 m_GravitVelocity = Vector3.zero;

	[SerializeField] Creature m_HeldCreature  = null; // item이 structure인 경우 고려하지 않음
	[SerializeField] Transform m_HeldPosition = null;
	[SerializeField] CreatureType m_WantedFood = CreatureType.None;



	public CreatureType CreatureType {
		get => m_CreatureType;
		set {
			if (m_CreatureType == value) return;
			m_CreatureType = value;
			#if UNITY_EDITOR
				bool pooled = value == CreatureType.None;
				gameObject.name = pooled? "Creature" : value.ToString();
			#endif
			Initialize();
			LinkAction();
		}
	}

	public AnimationType AnimationType {
		get => m_AnimationType;
		set => m_AnimationType = value;
	}

	public float Offset {
		get => m_Offset;
		set => m_Offset = value;
	}

	static int entityMask = 0;
	static int EntityMask {
		get {
			if (entityMask == 0) entityMask = 1 << LayerMask.NameToLayer("Entity");
			return entityMask;
		}
	}

	Rigidbody body;
	Rigidbody Body {
		get {
			if (!body) TryGetComponent(out body);
			return body;
		}
	}

	CapsuleCollider hitbox;
	SphereCollider  ground;
	NavMeshAgent    agent;

	public AttributeType AttributeType {
		get => m_AttributeType;
		set {
			m_AttributeType = value;
			if ((value & AttributeType.Pinned) != 0) {
				Velocity 	   = Vector3.zero;
				ForcedVelocity = Vector3.zero;
				GroundVelocity = Vector3.zero;
				GravitVelocity = Vector3.zero;
				Body.       velocity = Vector3.zero;
				Body.angularVelocity = Vector3.zero;
				Body.mass = float.MaxValue;
			}
			else {
				Body.mass = 1;
			}
			if ((value & AttributeType.Floating) != 0) {
				GravitVelocity = Vector3.zero;
			}
			if ((value & AttributeType.Piercing) != 0) {
				//if (hitbox || TryGetComponent(out hitbox)) hitbox.excludeLayers |=  EntityMask;
				//if (ground || TryGetComponent(out ground)) ground.excludeLayers |=  EntityMask;
			}
			else {
				//if (hitbox || TryGetComponent(out hitbox)) hitbox.excludeLayers &= ~EntityMask;
				//if (ground || TryGetComponent(out ground)) ground.excludeLayers &= ~EntityMask;
			}
		}
	}

	public HitboxType HitboxType {
		get => m_HitboxType;
		set {
			m_HitboxType = value;
			HitboxData data = NavMeshManager.GetHitboxData(value);
			if (hitbox || TryGetComponent(out hitbox)) {
				hitbox.radius = data.radius;
				hitbox.height = data.height;
			}
			if (ground || TryGetComponent(out ground)) {
				ground.center = new Vector3(0, -(data.height / 2 - data.radius) - 0.08f, 0);
				ground.radius = data.radius - 0.04f;
			}
			if (agent || TryGetComponent(out agent)) {
				agent.agentTypeID = data.agentTypeID;
				agent.radius = data.radius;
				agent.height = data.height;
			}
		}
	}

	public float SenseRange {
		get => m_SenseRange;
		set => m_SenseRange = value;
	}



	public Vector3 Velocity {
		get => m_Velocity;
		set => m_Velocity = value;
	}

	public Vector3 ForcedVelocity {
		get => m_ForcedVelocity;
		set => m_ForcedVelocity = value;
	}

	public Vector3 GroundVelocity {
		get => m_GroundVelocity;
		set => m_GroundVelocity = value;
	}

	public Vector3 GravitVelocity {
		get => m_GravitVelocity;
		set => m_GravitVelocity = value;
	}



	public Creature HeldCreature {
		get => m_HeldCreature;
		set => m_HeldCreature = value;
	}

	public Transform HeldPosition {
		get => m_HeldPosition;
		set => m_HeldPosition = value;
	}

	public CreatureType WantedFood {
		get => m_WantedFood;
		set => m_WantedFood = value;
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(Creature))] class CreatureEditor : ExtendedEditor {
			Creature I => target as Creature;
			public override void OnInspectorGUI() {
				Begin("Creature");

				LabelField("Creature", EditorStyles.boldLabel);
				I.CreatureType  = EnumField ("Creature Type",  I.CreatureType);
				I.AnimationType = EnumField ("Animation Type", I.AnimationType);
				I.Offset        = FloatField("Offset",         I.Offset);
				I.AttributeType = FlagField ("Attribute Type", I.AttributeType);
				I.HitboxType    = EnumField ("Hitbox Type",    I.HitboxType);
				I.SenseRange    = Slider    ("Sense Range",    I.SenseRange, 0, 32);
				Space();

				LabelField("Rigidbody", EditorStyles.boldLabel);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	static List<Creature> creatureList = new List<Creature>();
	static List<Creature> creaturePool = new List<Creature>();
	static Creature creaturePrefab;
	static Creature creature;

	public static List<Creature> GetList() => creatureList;

	public static Creature Spawn(CreatureType type, Vector3 position) {
		if (creaturePool.Count == 0) {
			if (!creaturePrefab) creaturePrefab = Resources.Load<Creature>(PrefabPath);
			creature = Instantiate(creaturePrefab);
		}
		else {
			int i = creaturePool.Count - 1;
			creature = creaturePool[i];
			creature.gameObject.SetActive(true);
			creaturePool.RemoveAt(i);
		}
		creature.transform.position = position;
		creature.transform.rotation = Quaternion.identity;
		creature.CreatureType = type;
		return creature;
	}

	void OnSpawn() {
		creatureList.Add(this);
	}

	public static void Despawn(Creature creature) {
		if (!creature) return;
		creature.gameObject.SetActive(false);
	}

	void OnDespawn() {
		CreatureType   = CreatureType.None;
		AnimationType  = AnimationType.Idle;
		Offset         = 0;
		HitboxType     = HitboxType.Humanoid;
		AttributeType  = 0;
		SenseRange     = 0;

		Velocity       = Vector3.zero;
		ForcedVelocity = Vector3.zero;
		GroundVelocity = Vector3.zero;
		GravitVelocity = Vector3.zero;

		creatureList.Remove(this);
		creaturePool.Add   (this);
	}

	void OnDestroy() {
		creatureList.Remove(this);
		creaturePool.Remove(this);
	}



	void GetData(int[] data) {
		if (data == null) data = new int[14];
		int i = 0;
		data[i++] = Utility.ToInt(transform.position.x);
		data[i++] = Utility.ToInt(transform.position.y);
		data[i++] = Utility.ToInt(transform.position.z);
		data[i++] = Utility.ToInt(transform.rotation);

		data[i++] = Utility.ToInt(CreatureType);
		data[i++] = Utility.ToInt(AnimationType);
		data[i++] = Utility.ToInt(Offset);
		data[i++] = Utility.ToInt(HitboxType);
		data[i++] = Utility.ToInt(AttributeType);
		data[i++] = Utility.ToInt(SenseRange);

		data[i++] = Utility.ToInt(Velocity);
		data[i++] = Utility.ToInt(ForcedVelocity);
		data[i++] = Utility.ToInt(GroundVelocity);
		data[i++] = Utility.ToInt(GravitVelocity);
	}

	void SetData(int[] data) {
		if (data == null) return;
		int i = 0;
		Vector3 position;
		position.x         = Utility.ToFloat(data[i++]);
		position.y         = Utility.ToFloat(data[i++]);
		position.z         = Utility.ToFloat(data[i++]);
		transform.position = position;
		transform.rotation = Utility.ToQuaternion(data[i++]);

		CreatureType       = Utility.ToEnum<CreatureType>(data[i++]);
		AnimationType      = Utility.ToEnum<AnimationType>(data[i++]);
		Offset             = Utility.ToFloat(data[i++]);
		HitboxType         = Utility.ToEnum<HitboxType>(data[i++]);
		AttributeType      = Utility.ToEnum<AttributeType>(data[i++]);
		SenseRange         = Utility.ToFloat(data[i++]);

		Velocity           = Utility.ToVector3(data[i++]);
		ForcedVelocity     = Utility.ToVector3(data[i++]);
		GroundVelocity     = Utility.ToVector3(data[i++]);
		GravitVelocity     = Utility.ToVector3(data[i++]);
	}

	public static void GetData(List<int[]> data) {
		for (int i = 0; i < creatureList.Count; i++) {
			if (data.Count - 1 < i) data.Add(null);
			creatureList[i].GetData(data[i]);
		}
	}

	public static void SetData(List<int[]> data) {
		for (int i = 0; i < data.Count; i++) {
			if (creatureList.Count - 1 < i) Spawn(CreatureType.None, Vector3.zero);
			creatureList[i].SetData(data[i]);
		}
		for (int i = creatureList.Count - 1; data.Count <= i; i--) Despawn(creatureList[i]);
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	public float Opacity { get; set; }

	List<Collider> layers          = new List<Collider>();
	bool           layerChanged    = false;
	int            layerMask       = 0;

	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;
	bool           isGrounded      = false;

	void BeginTrigger() {
		layers.Clear();
		layerChanged = true;
		layerMask = Utility.GetLayerMaskAtPoint(transform.position, transform);
		if (layerMask == 0) layerMask |= CameraManager.ExteriorLayer;
		Opacity = ((CameraManager.CullingMask | layerMask) != 0) ? 1 : 0;

		grounds.Clear();
		groundChanged = true;
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.isTrigger) {
			layers.Add(collider);
			layerChanged = true;
		}
		else {
			grounds.Add(collider);
			groundChanged = true;
		}
	}

	void OnTriggerExit(Collider collider) {
		if (collider.isTrigger) {
			layers.Remove(collider);
			layerChanged = true;
		}
		else {
			grounds.Remove(collider);
			groundChanged = true;
		}
	}

	void EndTrigger() {
		if (layerChanged) {
			layerChanged = false;
			layerMask = 0;
			for(int i = 0; i < layers.Count; i++) layerMask |= 1 << layers[i].gameObject.layer;
			if (layerMask == 0) layerMask |= CameraManager.ExteriorLayer;
		}
		if (groundChanged) {
			groundChanged = false;
			if (0 < grounds.Count) {
				int i = grounds.Count - 1;
				Utility.TryGetComponentInParent(grounds[i].transform, out groundRigidbody);
				groundRotation  = grounds[i].transform.localRotation;
				isGrounded      = true;
			}
			else {
				groundRigidbody = null;
				groundRotation  = Quaternion.identity;
				isGrounded      = false;
			}
		}
	}

	void FixedUpdate() {
		EndTrigger();

		bool visible = (CameraManager.CullingMask & layerMask) != 0;
		if ((visible && Opacity < 1) || (!visible && 0 < Opacity)) {
			Opacity += (visible? 1 : -1) * Time.deltaTime / CameraManager.TransitionTime;
			Opacity = Mathf.Clamp01(Opacity);
		}

		if ((AttributeType & AttributeType.Pinned) != 0) return;
		if (groundRigidbody) GroundVelocity = groundRigidbody.velocity;
		if (!isGrounded && (AttributeType & AttributeType.Floating) == 0) {
			GravitVelocity += Physics.gravity * Time.deltaTime;
		}

		Vector3 linearVelocity = Vector3.zero;
		linearVelocity += groundRotation * Velocity;
		linearVelocity += ForcedVelocity + GroundVelocity + GravitVelocity;
		Body.velocity = linearVelocity;
		
		if (ForcedVelocity != Vector3.zero) {
			ForcedVelocity *= !isGrounded ? 0.97f : 0.91f;
			if (ForcedVelocity.sqrMagnitude < 0.01f) ForcedVelocity = Vector3.zero;
		}
		if (GroundVelocity != Vector3.zero) {
			GroundVelocity *= !isGrounded ? 0.97f : 0.91f;
			if (GroundVelocity.sqrMagnitude < 0.01f) GroundVelocity = Vector3.zero;
		}
		if (GravitVelocity != Vector3.zero) {
			if (isGrounded) GravitVelocity = Vector3.zero;
		}

		if (Body.position.y < -32) Despawn(this);
	}



	public bool IsInteractable() => (AttributeType & (AttributeType)(-1 & ~0xFFFF)) != 0;

	public AttributeType GetInteractableType() {
		if (IsInteractable()) for (int i = 16; i < 32; i++) {
			if ((AttributeType & (AttributeType)(1 << i)) != 0) return (AttributeType)(1 << i);
		}
		return 0;
	}

	public void Interact(Creature creature) {
		if (IsInteractable()) OnInteract?.Invoke(creature);
	}



	Action<Creature> OnInteract;
	Action OnUpdate;

	bool link = false;

	void OnEnable() {
		link = true;
		BeginTrigger();
		OnSpawn();
	}

	void Update() {
		if (link) {
			link = false;
			Initialize();
			LinkAction();
		}
		Offset += Time.deltaTime;
		OnUpdate?.Invoke();
	}

	void OnDisable() {
		OnDespawn();
	}



	// ------------------------------------------------------------------------------------------------
	// Creature
	// ------------------------------------------------------------------------------------------------

	void Initialize() {
		switch (CreatureType) {
			case CreatureType.None:
				break;
			case CreatureType.Player:
				InitializeAsPlayer();
				break;
			case CreatureType.Client:
				InitializeClient();
				break;
			
			case CreatureType.ItemPlatter:
			case CreatureType.ItemFlour:
			case CreatureType.ItemCheese:
			case CreatureType.ItemButter:
			case CreatureType.ItemTomato:
			case CreatureType.ItemCabbage:
			case CreatureType.ItemBlueberry:
			case CreatureType.ItemMeat:
				HitboxType = HitboxType.Item;
				break;

			case CreatureType.FoodSoup:
			case CreatureType.FoodSpaghetti:
			case CreatureType.FoodCheeseCake:
			case CreatureType.FoodSalad:
			case CreatureType.FoodSandwich:
			case CreatureType.FoodSteak:
			case CreatureType.FoodBeer:
			case CreatureType.FoodWine:
				HitboxType = HitboxType.Item;
				break;
		}
	}

	void LinkAction() {
		switch (CreatureType) {
			case CreatureType.None:
				OnInteract = null;
				OnUpdate   = null;
				break;
			case CreatureType.Player:
				OnInteract = null;
				OnUpdate   = () => UpdatePlayer();
				break;
			case CreatureType.Client:
				OnInteract = null;
				OnUpdate   = () => UpdateClient();
				break;
			
			case CreatureType.ItemPlatter:
			case CreatureType.ItemFlour:
			case CreatureType.ItemCheese:
			case CreatureType.ItemButter:
			case CreatureType.ItemTomato:
			case CreatureType.ItemCabbage:
			case CreatureType.ItemBlueberry:
			case CreatureType.ItemMeat:
				OnUpdate = () => transform.rotation = CameraManager.Rotation;
				break;

			case CreatureType.FoodSoup:
			case CreatureType.FoodSpaghetti:
			case CreatureType.FoodCheeseCake:
			case CreatureType.FoodSalad:
			case CreatureType.FoodSandwich:
			case CreatureType.FoodSteak:
			case CreatureType.FoodBeer:
			case CreatureType.FoodWine:
				OnUpdate = () => transform.rotation = CameraManager.Rotation;
				break;
		}
	}



	Queue<Vector3> queue = new Queue<Vector3>();

	static Predicate<Creature>  creatureMatch;
	static Predicate<Structure> structureMatch;

	Creature  targetCreature;
	Structure targetStructure;

	void FindPath(Vector3 target, ref Queue<Vector3> queue) {
		float offset = NavMeshManager.GetHitboxData(HitboxType).height / 2;
		NavMeshManager.FindPath(transform.position, target, ref queue, offset);
	}



	// ------------------------------------------------------------------------------------------------
	// Player
	// ------------------------------------------------------------------------------------------------

	void InitializeAsPlayer() {
		GameObject heldObj = new GameObject("HoldPosition"); // 새로운 빈 게임 오브젝트 생성
        heldObj.transform.parent = this.transform;           // 플레이어의 자식으로 설정
        heldObj.transform.localPosition = new Vector3(0f, 0.3f, 1f); // 플레이어 기준 상대 위치(일단 머리 위)
		m_HeldPosition = heldObj.transform;

		HitboxType = HitboxType.Humanoid;
		SenseRange = 1.5f;

		queue.Clear();
		targetCreature  = null;
		targetStructure = null;
	}

	Vector2 pointPosition;
	Vector3 rotation;

	void UpdatePlayer() {

		// Input

		Vector3 input = Vector3.zero;
		if (UIManager.ActiveCanvas == CanvasType.Game) {
			if (CameraManager.Target != gameObject) CameraManager.Target = gameObject;
			
			input += CameraManager.Instance.transform.right   * InputManager.MoveDirection.x;
			input += CameraManager.Instance.transform.forward * InputManager.MoveDirection.y;
			input.y = 0;
			input.Normalize();
			
			if (InputManager.GetKeyDown(KeyAction.LeftClick)) {
				if (CameraManager.TryRaycast(InputManager.PointPosition, out Vector3 hit)) {
					FindPath(hit, ref queue);
				}
			}
			if (InputManager.GetKeyDown(KeyAction.RightClick)) {
				pointPosition = InputManager.PointPosition;
				rotation = CameraManager.EulerRotation;
			}
			if (InputManager.GetKey(KeyAction.RightClick)) {
				float mouseSensitivity = UIManager.MouseSensitivity;
				float delta = InputManager.PointPosition.x - pointPosition.x;
				CameraManager.EulerRotation = rotation + new Vector3(0, delta * mouseSensitivity, 0);
			}
		}

		// Movement

		if (input == Vector3.zero && queue.Count == 0) {
			if (AnimationType != AnimationType.Idle) Offset = 0;
			AnimationType = AnimationType.Idle;
			Velocity = Vector3.zero;
		}
		else {
			if (AnimationType != AnimationType.Move) Offset = 0;
			AnimationType = AnimationType.Move;
			if (input != Vector3.zero) {
				Velocity = input * 5;
				if (0 < queue.Count) queue.Clear();
			}
			else {
				Vector3 delta = queue.Peek() - transform.position;
				Velocity = new Vector3(delta.x, 0, delta.z).normalized * 5;
				if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.02f) queue.Dequeue();
			}
			if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);
		}

		// Interaction

		creatureMatch  = (Creature  creature ) => creature .IsInteractable();
		structureMatch = (Structure structure) => structure.IsInteractable();
		Utility.GetMatched(transform.position, SenseRange, creatureMatch,  ref targetCreature );
		Utility.GetMatched(transform.position, SenseRange, structureMatch, ref targetStructure);

		if (targetCreature != null && targetCreature.CreatureType == CreatureType.Player) targetCreature = null; //Player가 Player을 타겟 지정 방지
		if (targetCreature && targetStructure) targetStructure = null;
		if (HeldCreature) {
			if (/*InputManager.GetKeyDown(KeyAction.Interact)*/Input.GetKeyDown(KeyCode.Z)) { // 주석처리한 방식으로 수정 필요
				creatureMatch  = (Creature  creature ) => creature .IsInteractable() && creature.CreatureType == CreatureType.Client;
				Utility.GetMatched(transform.position, SenseRange, creatureMatch,  ref targetCreature );
				if (targetCreature && targetCreature.CreatureType == CreatureType.Client) {
					targetCreature .Interact(this);
				}
				else DropItem();
			}
			return; // 무언가를 들고 있다면 상호작용 불가
		}
		if (targetCreature || targetStructure) {
			//print(targetCreature);
			//print(targetStructure);
			if (/*InputManager.GetKeyDown(KeyAction.Interact)*/Input.GetKeyDown(KeyCode.Z)) { // 주석처리한 방식으로 수정 필요
				if (targetCreature ) targetCreature .Interact(this);
				if (targetStructure) targetStructure.Interact(this);
			}
		}
	}



	// ------------------------------------------------------------------------------------------------
	// Client
	// ------------------------------------------------------------------------------------------------

	float delay = 0;
	int   state = 0;

	void InitializeClient() {
		if(!transform.Find("HoldPosition")) {
			GameObject heldObj = new GameObject("HoldPosition"); // 새로운 빈 게임 오브젝트 생성
        	heldObj.transform.parent = this.transform;           // 플레이어의 자식으로 설정
        	heldObj.transform.localPosition = new Vector3(0f, 0.3f, 1f); // 플레이어 기준 상대 위치(일단 머리 위)
			m_HeldPosition = heldObj.transform;
		}

		HitboxType = HitboxType.Humanoid;
		SenseRange = 1.5f;

		queue.Clear();
		targetCreature  = null;
		targetStructure = null;

		Array enumValues = Enum.GetValues(typeof(CreatureType));
		m_WantedFood = (CreatureType) enumValues.GetValue(UnityEngine.Random.Range(12, enumValues.Length));

		delay = 0;
		state = 0;
	}

	public void UpdateClient() {
		delay -= Time.deltaTime;

		Vector3[] array = queue.ToArray();
		for (int i = 0; i < array.Length - 1; i++) {
			Debug.DrawLine(array[i], array[i + 1], Color.red);
		}
		switch (state) {
			// Search Chair
			case 0:
				//print("search");
				if (AnimationType != AnimationType.Idle) Offset = 0;
				AnimationType = AnimationType.Idle;

				if (delay < 0) {
					delay = UnityEngine.Random.Range(1f, 3f);
						structureMatch = (Structure chair) =>
						chair.StructureType == StructureType.Chair &&
						(chair.AttributeType & AttributeType.Interact) != 0;
					if (Utility.GetMatched(transform.position, 128f, structureMatch, ref targetStructure)) {
						FindPath(targetStructure.transform.position, ref queue);
						targetStructure.AttributeType &= ~AttributeType.Interact; // 앉으려는 자리가 겹치지 않도록 수정
						delay = 0;
						state = 1;
					}
				}
				break;
			// Move
			case 1:
				//print("move");
				if (AnimationType != AnimationType.Move) Offset = 0;
				AnimationType = AnimationType.Move;

				if (!targetStructure/* || (targetStructure.AttributeType & AttributeType.Interact) == 0*/) { // 앉으려는 자리가 겹치지 않도록 수정
					Velocity = Vector3.zero;
					delay = 0;
					state = 0;
					break;
				}
				if (Vector3.Distance(transform.position, targetStructure.transform.position) < 1.5f) {
					Velocity = Vector3.zero;
					transform.position = targetStructure.transform.position + Vector3.up;
					//targetStructure.AttributeType &= ~AttributeType.Interact; // 앉으려는 자리가 겹치지 않도록 수정
					delay = UnityEngine.Random.Range(10f, 16f);
					state = 2;
					break;
				}
				Vector3 delta = queue.Peek() - transform.position;
				Velocity = new Vector3(delta.x, 0, delta.z).normalized * 5;
				if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.02f) queue.Dequeue();
				if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);
				break;
			// Wait
			case 2:
				//print("wait");

				if (AnimationType != AnimationType.Idle) Offset = 0;
				AnimationType = AnimationType.Idle;
				AttributeType = AttributeType.Serve;
				
				OnInteract = (Creature player) => {
					if (player.HeldCreature && player.HeldCreature.CreatureType == WantedFood) {
						//target(player)의 요리 받아오기
						player.HeldCreature.HoldThisItem(this);
						player.HeldCreature = null;
						HeldCreature.AttributeType = 0;
    					delay = UnityEngine.Random.Range(10f, 16f);
						state = 3;
						AttributeType = 0;
					}
				};
				

				if (delay < 0) { // 시간 내 상호작용 실패
					delay = 0;
					state = 5;
					GameManager.UpdateReputation(-0.3f);
				}
				break;
			// Eat
			case 3:
				//print("eating!");
				OnInteract = null;
				if (AnimationType != AnimationType.Idle) Offset = 0; // 식사 애니메이션 추가 시 수정
				AnimationType = AnimationType.Idle;
				AttributeType = 0;

				if (delay < 0) {
					Destroy(HeldCreature);
					HeldCreature = null;
					delay = 0;
					state = 5;
					GameManager.UpdateReputation(0.1f);
				}
				break;
			// Drink
			case 4:
				//print("drink");
				OnInteract = null;
				if (AnimationType != AnimationType.Idle) Offset = 0; // 마시기 애니메이션 추가 시 수정
				AnimationType = AnimationType.Idle;
				AttributeType = 0;

				if (delay < 0) {
					Destroy(HeldCreature);
					delay = 0;
					state = 5;
					GameManager.UpdateReputation(0.1f);
				}
				break;
			// Search Exit
			case 5:
				//print("exit search");
				OnInteract = null;
				if (AnimationType != AnimationType.Idle) Offset = 0;
				AnimationType = AnimationType.Idle;
				AttributeType = 0;

				if (delay < 0) {
					FindPath(GameManager.ClientSpawnPoint, ref queue);
					targetStructure.AttributeType |= AttributeType.Interact;
					delay = 0;
					state = 6;
				}
				break;
				// Move
			case 6:
				//print("exit");
				if (AnimationType != AnimationType.Move) Offset = 0;
				AnimationType = AnimationType.Move;

				if (queue.Count == 0) {
					Despawn(this);
					break;
				}
				Vector3 delta_ = queue.Peek() - transform.position;
				Velocity = new Vector3(delta_.x, 0, delta_.z).normalized * 5;
				if (new Vector3(delta_.x, 0, delta_.z).sqrMagnitude < 0.02f) queue.Dequeue();
				if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);
				break;
		}
	}
	public void HoldThisItem(Creature creature) { // Structure에 대해 대응불가 creature는 아이템을 집을 대상
		creatureMatch = (creature) =>
			creature.CreatureType == CreatureType.Player || creature.CreatureType == CreatureType.Client;
		if (Utility.GetMatched(transform.position, 1.5f, creatureMatch, ref targetCreature)) {
			//집기
			targetCreature.HeldCreature = this;
			GetComponent<Rigidbody>().isKinematic = true; // 물리 효과 제거
			transform.position = targetCreature.HeldPosition.position; // 집기
			transform.parent = targetCreature.HeldPosition; // 플레이어의 자식 컴포넌트로
		}
	}

	public void DropItem() {
		HeldCreature.GetComponent<Rigidbody>().isKinematic = false; // 물리 효과 복원
        HeldCreature.transform.parent = null;                       // 부모-자식 관계 해제
        HeldCreature = null;
	}
}

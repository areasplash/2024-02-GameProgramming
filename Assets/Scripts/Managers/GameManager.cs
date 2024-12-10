using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class GameManager : MonoSingleton<GameManager> {

	const EntityType Flour      = EntityType.ItemFlour;
	const EntityType Butter     = EntityType.ItemButter;
	const EntityType Cheese     = EntityType.ItemCheese;
	const EntityType Blueberry  = EntityType.ItemBlueberry;
	const EntityType Tomato     = EntityType.ItemTomato;
	const EntityType Potato     = EntityType.ItemPotato;
	const EntityType Cabbage    = EntityType.ItemCabbage;
	const EntityType Meat       = EntityType.ItemMeat;

	const EntityType Pancake    = EntityType.FoodPancake;
	const EntityType CheeseCake = EntityType.FoodCheeseCake;
	const EntityType Spaghetti  = EntityType.FoodSpaghetti;
	const EntityType Soup       = EntityType.FoodSoup;
	const EntityType Sandwich   = EntityType.FoodSandwich;
	const EntityType Salad      = EntityType.FoodSalad;
	const EntityType Steak      = EntityType.FoodSteak;

	const EntityType Wine       = EntityType.FoodWine;
	const EntityType Beer       = EntityType.FoodBeer;



	public static List<EntityType> Item { get; } = new() {
		Flour, Butter, Cheese, Blueberry, Tomato, Potato, Cabbage, Meat,
	};

	public static Dictionary<EntityType, List<EntityType>> Recipe { get; } = new() {
		{ Pancake,    new List<EntityType>() { Flour,   Butter,             }},
		{ CheeseCake, new List<EntityType>() { Flour,   Cheese,  Blueberry, }},
		{ Spaghetti,  new List<EntityType>() { Flour,   Tomato,             }},
		{ Soup,       new List<EntityType>() { Potato,                      }},
		{ Sandwich,   new List<EntityType>() { Flour,   Cabbage, Tomato,    }},
		{ Salad,      new List<EntityType>() { Cabbage, Tomato,             }},
		{ Steak,      new List<EntityType>() { Meat,                        }},
	};

	public static Dictionary<EntityType, int> Price { get; } = new() {
		{ Pancake,    100 },
		{ CheeseCake, 180 },
		{ Spaghetti,  120 },
		{ Soup,        80 },
		{ Sandwich,   120 },
		{ Salad,       80 },
		{ Steak,      240 },

		{ Wine,        80 },
		{ Beer,        40 },
	};



	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] float m_TimeMultiplier  =   300f;
	[SerializeField] float m_OpenHour        =     9f;
	[SerializeField] float m_CloseHour       =    22f;
	[SerializeField] float m_Hour            =     9f;
	[SerializeField] int   m_Day             =     1;
	[SerializeField] int   m_EndDay          =    30;
	[SerializeField] int   m_Money           =     0;
	[SerializeField] int   m_MoneyAllocation = 10000;

	[SerializeField] float m_ReputationBias =  1f;
	[SerializeField] float m_MaxReputation  = 20f;
	[SerializeField] float m_MaxSpawnPeriod = 30f;
	[SerializeField] float m_MinSpawnPeriod =  3f;

	[SerializeField] Transform m_CustomerSpawnTransform;

	[SerializeField] GameObject m_TablePrefab;
	[SerializeField] GameObject m_ChairPrefab;
	[SerializeField] GameObject m_ChestPrefab;
	[SerializeField] GameObject m_PotPrefab;
	[SerializeField] GameObject m_CustomerPrefab;
	[SerializeField] GameObject m_StaffPrefab;
	[SerializeField] GameObject m_MoneyPrefab;

	[SerializeField] int m_TableCost     =  360;
	[SerializeField] int m_ChairCost     =  180;
	[SerializeField] int m_PotCost       =  480;
	[SerializeField] int m_ExpansionCost = 1440;
	[SerializeField] int m_StaffCost     = 1680;

	[SerializeField] int m_TableCount     = 0;
	[SerializeField] int m_ChairCount     = 0;
	[SerializeField] int m_PotCount       = 0;
	[SerializeField] int m_ExpansionCount = 0;
	[SerializeField] int m_StaffCount     = 0;

	[SerializeField] List<GameObject> m_TableList = new List<GameObject>();
	[SerializeField] List<GameObject> m_ChairList = new List<GameObject>();
	[SerializeField] List<GameObject> m_PotList   = new List<GameObject>();
	[SerializeField] List<GameObject> m_ExpansionDList = new List<GameObject>();
	[SerializeField] List<GameObject> m_ExpansionEList = new List<GameObject>();



	public static float TimeMultiplier {
		get           =>  Instance ? Instance.m_TimeMultiplier : default;
		private set { if (Instance)  Instance.m_TimeMultiplier = Mathf.Max(0.1f, value); }
	}
	public static float OpenHour {
		get           =>  Instance ? Instance.m_OpenHour : default;
		private set { if (Instance)  Instance.m_OpenHour = Mathf.Clamp(value, 0f, CloseHour); }
	}
	public static float CloseHour {
		get           =>  Instance ? Instance.m_CloseHour : default;
		private set { if (Instance)  Instance.m_CloseHour = Mathf.Clamp(value, OpenHour, 24f); }
	}
	public static float Hour {
		get           =>  Instance ? Instance.m_Hour : default;
		private set { if (Instance)  Instance.m_Hour = Mathf.Clamp(value, OpenHour, CloseHour); }
	}
	public static int Day {
		get           =>  Instance ? Instance.m_Day : default;
		private set { if (Instance)  Instance.m_Day = value; }
	}
	public static int EndDay {
		get           =>  Instance ? Instance.m_EndDay : default;
		private set { if (Instance)  Instance.m_EndDay = value; }
	}
	public static int Money {
		get   =>  Instance ? Instance.m_Money : default;
		set { if (Instance)  Instance.m_Money = value; }
	}
	public static int MoneyAllocation {
		get           =>  Instance ? Instance.m_MoneyAllocation : default;
		private set { if (Instance)  Instance.m_MoneyAllocation = value; }
	}



	public static float ReputationBias {
		get   =>  Instance ? Instance.m_ReputationBias : default;
		set { if (Instance)  Instance.m_ReputationBias = Mathf.Max(-4.34f, value); }
	}
	public static float MaxReputation {
		get           =>  Instance ? Instance.m_MaxReputation : default;
		private set { if (Instance)  Instance.m_MaxReputation = value; }
	}
	public static float MaxSpawnPeriod {
		get           =>  Instance ? Instance.m_MaxSpawnPeriod : default;
		private set { if (Instance)  Instance.m_MaxSpawnPeriod = value; }
	}
	public static float MinSpawnPeriod {
		get           =>  Instance ? Instance.m_MinSpawnPeriod : default;
		private set { if (Instance)  Instance.m_MinSpawnPeriod = value; }
	}

	public static float Reputation {
		get => MaxReputation / (1f + Mathf.Exp(-0.4f * (ReputationBias - 4.34f)));
	}
	public static float SpawnPeriod {
		get => Mathf.Lerp(MaxSpawnPeriod, MinSpawnPeriod, Reputation / MaxReputation);
	}



	static Transform CustomerSpawnTransform {
		get   =>  Instance ? Instance.m_CustomerSpawnTransform : default;
		set { if (Instance)  Instance.m_CustomerSpawnTransform = value; }
	}
	public static Vector3 CustomerSpawnPoint {
		get => CustomerSpawnTransform ? CustomerSpawnTransform.position : default;
	}



	static GameObject MoneyPrefab {
		get   =>  Instance ? Instance.m_MoneyPrefab : default;
		set { if (Instance)  Instance.m_MoneyPrefab = value; }
	}
	static GameObject TablePrefab {
		get   =>  Instance ? Instance.m_TablePrefab : default;
		set { if (Instance)  Instance.m_TablePrefab = value; }
	}
	static GameObject ChairPrefab {
		get   =>  Instance ? Instance.m_ChairPrefab : default;
		set { if (Instance)  Instance.m_ChairPrefab = value; }
	}
	static GameObject ChestPrefab {
		get   =>  Instance ? Instance.m_ChestPrefab : default;
		set { if (Instance)  Instance.m_ChestPrefab = value; }
	}
	static GameObject PotPrefab {
		get   =>  Instance ? Instance.m_PotPrefab : default;
		set { if (Instance)  Instance.m_PotPrefab = value; }
	}
	static GameObject CustomerPrefab {
		get   =>  Instance ? Instance.m_CustomerPrefab : default;
		set { if (Instance)  Instance.m_CustomerPrefab = value; }
	}
	static GameObject StaffPrefab {
		get   =>  Instance ? Instance.m_StaffPrefab : default;
		set { if (Instance)  Instance.m_StaffPrefab = value; }
	}



	public static int TableCost {
		get   =>  Instance ? Instance.m_TableCost : default;
		set { if (Instance)  Instance.m_TableCost = value; }
	}
	public static int ChairCost {
		get   =>  Instance ? Instance.m_ChairCost : default;
		set { if (Instance)  Instance.m_ChairCost = value; }
	}
	public static int PotCost {
		get   =>  Instance ? Instance.m_PotCost : default;
		set { if (Instance)  Instance.m_PotCost = value; }
	}
	public static int ExpansionCost {
		get   =>  Instance ? Instance.m_ExpansionCost : default;
		set { if (Instance)  Instance.m_ExpansionCost = value; }
	}
	public static int StaffCost {
		get   =>  Instance ? Instance.m_StaffCost : default;
		set { if (Instance)  Instance.m_StaffCost = value; }
	}

	public static int TableCount {
		get   =>  Instance ? Instance.m_TableCount : default;
		set { if (Instance)  Instance.m_TableCount = value; }
	}
	public static int ChairCount {
		get   =>  Instance ? Instance.m_ChairCount : default;
		set { if (Instance)  Instance.m_ChairCount = value; }
	}
	public static int PotCount {
		get   =>  Instance ? Instance.m_PotCount : default;
		set { if (Instance)  Instance.m_PotCount = value; }
	}
	public static int ExpansionCount {
		get   =>  Instance ? Instance.m_ExpansionCount : default;
		set { if (Instance)  Instance.m_ExpansionCount = value; }
	}
	public static int StaffCount {
		get   =>  Instance ? Instance.m_StaffCount : default;
		set { if (Instance)  Instance.m_StaffCount = value; }
	}

	public static List<GameObject> TableList => Instance ? Instance.m_TableList : default;
	public static List<GameObject> ChairList => Instance ? Instance.m_ChairList : default;
	public static List<GameObject> PotList   => Instance ? Instance.m_PotList   : default;
	public static List<GameObject> ExpansionDList => Instance ? Instance.m_ExpansionDList : default;
	public static List<GameObject> ExpansionEList => Instance ? Instance.m_ExpansionEList : default;

	public static int TableAvailable {
		get {
			bool max = TableCount == TableList.Count;
			bool match = TableCost <= Money;
			match &= ExpansionCount == 0 ? TableCount < 3 : TableCount < 7;
			return max ? 2 : (match ? 1 : 0);
		}
	}
	public static int ChairAvailable {
		get {
			bool max = ChairCount == ChairList.Count;
			bool match = ChairCost <= Money;
			match &= ExpansionCount == 0 ? ChairCount < 3 : ChairCount < 7;
			match &= ChairCount < TableCount;
			return max ? 2 : (match ? 1 : 0);
		}
	}
	public static int PotAvailable {
		get {
			bool max = PotCount == PotList.Count;
			bool match = PotCost <= Money;
			return max ? 2 : (match ? 1 : 0);
		}
	}
	public static int ExpansionAvailable {
		get {
			bool max = ExpansionCount == ExpansionDList.Count;
			bool match = ExpansionCost <= Money;
			return max ? 2 : (match ? 1 : 0);
		}
	}
	public static int StaffAvailable {
		get {
			bool max = false;
			bool match = StaffCost <= Money;
			return max ? 2 : (match ? 1 : 0);
		}
	}



	public static bool IsOpen => Hour < CloseHour;



	#if UNITY_EDITOR
		[CustomEditor(typeof(GameManager))] class GameManagerEditor : ExtendedEditor {
			public override void OnInspectorGUI() {
				Begin("GameManager");

				LabelField("Game", EditorStyles.boldLabel);
				TimeMultiplier = FloatField("Time Multiplier", TimeMultiplier);
				OpenHour       = Slider    ("OpenTime",        OpenHour,  0, 24f);
				CloseHour      = Slider    ("CloseTime",       CloseHour, 0, 24f);
				Hour           = Slider    ("Time",            Hour, OpenHour, CloseHour);
				Day            = IntField  ("Day",             Day);
				EndDay         = IntField  ("EndDay",          EndDay);
				Money          = IntField  ("Money",           Money);
				Space();
				ReputationBias = FloatField("Reputation Bias",  ReputationBias);
				MaxReputation  = FloatField("Max Reputation",   MaxReputation );
				MaxSpawnPeriod = FloatField("Max Spawn Period", MaxSpawnPeriod);
				MinSpawnPeriod = FloatField("Min Spawn Period", MinSpawnPeriod);
				BeginHorizontal();
				PrefixLabel("Reputation");
				LabelField(" " + Reputation.ToString("F2"));
				EndHorizontal();
				BeginHorizontal();
				PrefixLabel("Spawn Period");
				LabelField(" " + SpawnPeriod.ToString("F2"));
				EndHorizontal();
				Space();

				LabelField("Point", EditorStyles.boldLabel);
				CustomerSpawnTransform = ObjectField("Customer Spawn Point", CustomerSpawnTransform);
				Space();

				LabelField("Prefab", EditorStyles.boldLabel);
				TablePrefab    = ObjectField("Table Prefab",    TablePrefab   );
				ChairPrefab    = ObjectField("Chair Prefab",    ChairPrefab   );
				ChestPrefab    = ObjectField("Chest Prefab",    ChestPrefab   );
				PotPrefab      = ObjectField("Pot Prefab",      PotPrefab     );
				CustomerPrefab = ObjectField("Customer Prefab", CustomerPrefab);
				StaffPrefab    = ObjectField("Staff Prefab",    StaffPrefab   );
				MoneyPrefab    = ObjectField("Money Prefab",    MoneyPrefab   );
				Space();

				LabelField("Upgrade", EditorStyles.boldLabel);
				TableCost     = IntField("Table Cost",     TableCost    );
				ChairCost     = IntField("Chair Cost",     ChairCost    );
				PotCost       = IntField("Pot Cost",       PotCost      );
				ExpansionCost = IntField("Expansion Cost", ExpansionCost);
				StaffCost     = IntField("Staff Cost",     StaffCost    );
				Space();
				TableCount     = IntField("Table Count",     TableCount    );
				ChairCount     = IntField("Chair Count",     ChairCount    );
				PotCount       = IntField("Pot Count",       PotCount      );
				ExpansionCount = IntField("Expansion Count", ExpansionCount);
				StaffCount     = IntField("Staff Count",     StaffCount    );
				Space();

				LabelField("List", EditorStyles.boldLabel);
				PropertyField("m_TableList");
				PropertyField("m_ChairList");
				PropertyField("m_PotList");
				PropertyField("m_ExpansionDList");
				PropertyField("m_ExpansionEList");
				Space();

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	static HashSet<EntityType> hashset = new HashSet<EntityType>();

	public static EntityType GetFoodFromRecipe(List<EntityType> items) {
		foreach (var recipe in Recipe) {
			hashset.Clear();
			foreach (var item in items) hashset.Add(item);
			if (hashset.SetEquals(recipe.Value)) return recipe.Key;
		}
		return EntityType.None;
	}



	public static Table SpawnTable(Vector3 position) {
		GameObject prefab = Instantiate(TablePrefab, position, Quaternion.identity);
		prefab.TryGetComponent(out Table table);
		return table;
	}
	public static Chair SpawnChair(Vector3 position) {
		GameObject prefab = Instantiate(ChairPrefab, position, Quaternion.identity);
		prefab.TryGetComponent(out Chair chair);
		return chair;
	}
	public static Chest SpawnChest(Vector3 position) {
		GameObject prefab = Instantiate(ChestPrefab, position, Quaternion.identity);
		prefab.TryGetComponent(out Chest chest);
		return chest;
	}
	public static Pot SpawnPot(Vector3 position) {
		GameObject prefab = Instantiate(PotPrefab, position, Quaternion.identity);
		prefab.TryGetComponent(out Pot pot);
		return pot;
	}
	public static Customer SpawnCustomer(Vector3 position) {
		GameObject prefab = Instantiate(CustomerPrefab, position, Quaternion.identity);
		prefab.TryGetComponent(out Customer customer);
		return customer;
	}
	public static Staff SpawnStaff(Vector3 position) {
		GameObject prefab = Instantiate(StaffPrefab, position, Quaternion.identity);
		prefab.TryGetComponent(out Staff staff);
		return staff;
	}
	public static Money SpawnMoney(Vector3 position) {
		GameObject prefab = Instantiate(MoneyPrefab, position, Quaternion.identity);
		prefab.TryGetComponent(out Money money);
		return money;
	}



	public static void OnTableClick() {
		TableList[TableCount++].SetActive(true);
		Money -= TableCost;
		NavMeshManager.Bake();
	}
	public static void OnChairClick() {
		ChairList[ChairCount++].SetActive(true);
		Money -= ChairCost;
		NavMeshManager.Bake();
	}
	public static void OnPotClick() {
		PotList[PotCount++].SetActive(true);
		Money -= PotCost;
		NavMeshManager.Bake();
	}
	public static void OnExpansionClick() {
		ExpansionDList[ExpansionCount].SetActive(false);
		ExpansionEList[ExpansionCount].SetActive(true );
		ExpansionCount++;
		Money -= ExpansionCost;
		NavMeshManager.Bake();
	}
	public static void OnStaffClick() {
		StaffCount++;
		Money -= StaffCost;
		float radian = Random.value * Mathf.PI * 2f;
		float radius = Random.value * 1f + 0.5f;
		Vector3 delta = new Vector3(Mathf.Cos(radian), 0f, Mathf.Sin(radian)) * radius;
		SpawnStaff(new Vector3(0, 0.75f, 0) + delta);
	}
	public static void OnNextClick() {
		Hour = OpenHour;
		Day++;
		if (Instance) {
			Instance.moneyDelta      = Money;
			Instance.reputationDelta = Reputation;
			Instance.customerCount   = 0;
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void Start() {
		NavMeshManager.Bake();
	}

	bool beginServiceMessage = true;
	bool closeServiceMessage = true;
	bool endingMessage       = true;

	int   moneyDelta      = 0;
	float reputationDelta = 0f;
	int   customerCount   = 0;
	float timer           = 3f;

	void Update() {
		if (!UIManager.IsGameRunning) return;
		
		if (beginServiceMessage) {
			beginServiceMessage = false;
			UIManager.EnqueueDialogue("Begin Service 0");
			UIManager.EnqueueDialogue("Begin Service 1");
			UIManager.EnqueueDialogue("Begin Service 2");
			UIManager.EnqueueDialogue("Begin Service 3");
			UIManager.OpenDialogue();
			UIManager.OnDialogueEnd.AddListener(() => GameRecipeUI.IsShowing = true);
		}
		if (closeServiceMessage && Hour == CloseHour) {
			closeServiceMessage = false;
			UIManager.EnqueueDialogue("Close Service 0");
			UIManager.EnqueueDialogue("Close Service 1");
			UIManager.OpenDialogue();
		}
		if (endingMessage && Day == EndDay && Hour == CloseHour) {
			endingMessage = false;
			if (MoneyAllocation <= Money) {
				Money -= MoneyAllocation;
				UIManager.EnqueueDialogue("Clear Ending 0");
				UIManager.EnqueueDialogue("Clear Ending 1");
				UIManager.EnqueueDialogue("Clear Ending 2");
			}
			else {
				UIManager.EnqueueDialogue("Fail Ending 0");
				UIManager.EnqueueDialogue("Fail Ending 1");
				UIManager.EnqueueDialogue("Fail Ending 2");
				UIManager.OnDialogueEnd.AddListener(() => UIManager.Quit());
			}
			UIManager.OpenDialogue();
		}

		if (Hour < CloseHour) {
			Hour += Time.deltaTime / 3600f * TimeMultiplier;
			if (Hour < CloseHour - 0.5f) {
				timer -= Time.deltaTime;
				if (Random.value * SpawnPeriod <= Time.deltaTime || timer <= 0f) {
					int count = 1;
					if (Random.value <= 0.1f) count = Random.Range(2, 5);
					for (int i = 0; i < count; i++) {
						float radian = Random.value * Mathf.PI * 2f;
						float radius = Random.value * 1f + 0.5f;
						Vector3 delta = new Vector3(Mathf.Cos(radian), 0f, Mathf.Sin(radian)) * radius;
						SpawnCustomer(CustomerSpawnPoint + delta);
						customerCount++;
						timer = SpawnPeriod;
					}
				}
			}
			if (Hour == CloseHour) {
				moneyDelta = Money - moneyDelta;
				reputationDelta = Reputation - reputationDelta;
				string a = customerCount.ToString();
				string b = (moneyDelta < 0 ? ' ' : '+') + moneyDelta.ToString();
				string c = (reputationDelta < 0 ? ' ' : '+') + reputationDelta.ToString("F2");
				string d = EndDay - Day < 0 ? "-" : ("D - " + (EndDay - Day).ToString());
				string e = Mathf.Max(0, MoneyAllocation - Money).ToString();
				for (int i = 3; i < e.Length; i += 4) e = e.Insert(e.Length - i, ",");
				GameReportUI.SetCustomerCount(a);
				GameReportUI.SetMoney(b);
				GameReportUI.SetReputation(c);
				GameReportUI.SetDebtDeadline(d);
				GameReportUI.SetMoneyNeeded(e);
			}
		}
	}
}

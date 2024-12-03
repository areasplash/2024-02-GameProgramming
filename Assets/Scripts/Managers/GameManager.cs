using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class GameManager : MonoSingleton<GameManager> {

	public static Dictionary<CreatureType, List<CreatureType>> Recipe { get; } = new() {
		{
			CreatureType.FoodFancake,
			new List<CreatureType>() {
				CreatureType.ItemFlour,
				CreatureType.ItemButter
			}
		}, {
			CreatureType.FoodCheeseCake,
			new List<CreatureType>() {
				CreatureType.ItemFlour,
				CreatureType.ItemCheese,
				CreatureType.ItemBlueberry
			}
		}, {
			CreatureType.FoodSpaghetti,
			new List<CreatureType>() {
				CreatureType.ItemFlour,
				CreatureType.ItemCheese,
				CreatureType.ItemTomato
			}
		}, {
			CreatureType.FoodSoup,
			new List<CreatureType>() {
				CreatureType.ItemFlour,
				CreatureType.ItemPotato,
				CreatureType.ItemButter
			}
		}, {
			CreatureType.FoodSandwich,
			new List<CreatureType>() {
				CreatureType.ItemFlour,
				CreatureType.ItemCabbage,
				CreatureType.ItemTomato
			}
		}, {
			CreatureType.FoodSalad,
			new List<CreatureType>() {
				CreatureType.ItemCabbage,
				CreatureType.ItemTomato
			}
		}, {
			CreatureType.FoodSteak,
			new List<CreatureType>() {
				CreatureType.ItemMeat
			}
		},
	};

	public static CreatureType GetFoodFromRecipe(List<CreatureType> items) {
		foreach (var recipe in Recipe) {
			if (items.Count != recipe.Value.Count) continue;
			bool match = true;
			for (int i = 0; i < items.Count; i++) {
				if (items[i] != recipe.Value[i]) {
					match = false;
					break;
				}
			}
			if (match) return recipe.Key;
		}
		return CreatureType.None;
	}



	// ================================================================================================
	// Fields
	// ================================================================================================

	public const float MoneyThreshold = 10000f;
	public const float MaxReputation  =    20f;

	[SerializeField] Transform m_ClientSpawnPoint;
	[SerializeField, Range(0f, 20f)] float m_SpawnPeriod = 5f;
	[SerializeField] float m_RepBias = 0f;
	[SerializeField, Range(0f, MaxReputation)] float m_Reputation = 3f;
	//[SerializeField, Range(0.02f, 0.15f)] float m_MultiSpawnProb = 0.03f;

	[SerializeField] int m_Hour = 9;
	[SerializeField] int m_Minute = 0;
	[SerializeField] int m_OpenHour = 9;
	[SerializeField] int m_CloseHour = 22;

	[SerializeField] int m_Money = 0;



	public static Vector3 ClientSpawnPoint =>
		Instance && Instance.m_ClientSpawnPoint ?
		Instance.m_ClientSpawnPoint.position : default;


	public static float RepBias {
		get           =>  Instance? Instance.m_RepBias : default;
        private set { if (Instance) Instance.m_RepBias = value; }
	}

	public static float Reputation {
		get           =>  Instance? Instance.m_Reputation : default;
        private set { if (Instance) Instance.m_Reputation = value; }
	}



	public static int Hour {
		get           =>  Instance? Instance.m_Hour : default;
		private set { if (Instance) Instance.m_Hour = value; }
	}

	public static int Minute {
		get           =>  Instance? Instance.m_Minute : default;
		private set { if (Instance) Instance.m_Minute = value; }
	}

	public static int OpenHour {
		get           =>  Instance? Instance.m_OpenHour : default;
		private set { if (Instance) Instance.m_OpenHour = value; }
	}

	public static int CloseHour {
		get           =>  Instance? Instance.m_CloseHour : default;
		private set { if (Instance) Instance.m_CloseHour = value; }
	}



	public static int Money {
		get   =>  Instance? Instance.m_Money : default;
		set { if (Instance) Instance.m_Money = value; }
	}
	
	/*
	public static float MultiSpawnProb {
		get => Instance ? Instance.m_MultiSpawnProb : default;
        private set
        {
            if (Instance)
            {
                Instance.m_MultiSpawnProb = value;
            }
        }
	}
	*/



	#if UNITY_EDITOR
		[CustomEditor(typeof(GameManager))] class GameManagerEditor : ExtendedEditor {
			public override void OnInspectorGUI() {
				Begin("GameManager");

				LabelField("Client", EditorStyles.boldLabel);
				PropertyField("m_ClientSpawnPoint");
				PropertyField("m_SpawnPeriod");
				Space();

				LabelField("Factor", EditorStyles.boldLabel);
				PropertyField("m_RepBias");
				FloatField("Reputation", Reputation);
				//PropertyField(m_MultiSpawnProb);
				FloatField("MultiSpawnProb", MultiSpawnProb);
				Space();

				LabelField("InGameTime", EditorStyles.boldLabel);
				PropertyField("m_Hour");
				PropertyField("m_Minute");
				OpenHour  = IntField("Open Hour",  OpenHour );
				CloseHour = IntField("Close Hour", CloseHour);
				Space();

				LabelField("Money", EditorStyles.boldLabel);
				Money = IntField("Money", Money);
				Space();

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public static float MultiSpawnProb = 0.03f;
	public static Action TimeHandle;
	private float minuteToRealTime = 3f;
	private float timer;

	public static void UpdateReputation(float delta) {
		RepBias += delta;

		Reputation = 20f / (1f + Mathf.Exp(-0.4f * (RepBias-4.34f))); //sigmoid function
		//SpawnPeriod = Mathf.Lerp(20f, 3f, Mathf.Log(1f + Mathf.InverseLerp(0f, 10f, Reputation) * 99f) / Mathf.Log(100f)); // 최소 3초 ~ 최대 20초
		MultiSpawnProb = Mathf.Lerp(0.02f, 0.15f, (Mathf.Exp(Mathf.InverseLerp(0f, 20f, Reputation) * 1.2f) - 1) / (Mathf.Exp(1.2f) - 1));
	}

	public void UpdateGameTime() {
		timer -= Time.deltaTime;

		if(timer <= 0) {
			m_Minute += 15;
			if(m_Minute >= 60) {
				m_Hour++;
				m_Minute = 0;
			}
			timer = minuteToRealTime;
		}
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	float spawnTimer = 0f;

	void Start() {
		m_Minute = 0;
		m_Hour = m_OpenHour;
		timer = minuteToRealTime;
		TimeHandle += UpdateGameTime;
	}
	void Update() {
		spawnTimer -= Time.deltaTime;
		
		if (m_Hour < m_CloseHour) {
			if (m_ClientSpawnPoint && spawnTimer <= 0f) {
				float prob = UnityEngine.Random.value;
				spawnTimer = m_SpawnPeriod;
				if(prob <= MultiSpawnProb) {

					for(int i = 0; i <= UnityEngine.Random.Range(2, 6); i++) {
						Vector3 randPosition = new Vector3(
							UnityEngine.Random.Range(-0.4f, 0.4f),
							0,
							UnityEngine.Random.Range(0f, 0.8f)
						);
						Creature.Spawn(CreatureType.Client, m_ClientSpawnPoint.position + randPosition); // 다중 스폰
					}
				}
				else Creature.Spawn(CreatureType.Client, m_ClientSpawnPoint.position); // 단일 스폰
			}
		}
		else {
			TimeHandle -= UpdateGameTime;
			// 결과창
			// 날짜 체크 및 엔딩 여부
		}
		

		TimeHandle?.Invoke();
	}
}

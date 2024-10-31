using UnityEngine;
using UnityEngine.Localization.Settings;


#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



public class UIManager : MonoSingleton<UIManager> {

	// Fields

	[SerializeField] Canvas mainmenu;
	[SerializeField] Canvas game;
	[SerializeField] Canvas menu;
	[SerializeField] Canvas settings;



	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(UIManager)), CanEditMultipleObjects]
		public class UIManagerEditor : Editor {
			UIManager I => target as UIManager;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				Space();
				LabelField("Canvas", EditorStyles.boldLabel);
				I.mainmenu = ObjectField("Main Menu", I.mainmenu);
				I.game     = ObjectField("Game",      I.game    );
				I.menu     = ObjectField("Menu",      I.menu    );
				I.settings = ObjectField("Settings",  I.settings);

				if (GUI.changed) EditorUtility.SetDirty(target);
			}
		}
	#endif



	// Methods

	public static void SetMainMenu() {
		Instance.mainmenu.gameObject.SetActive(true );
		Instance.game    .gameObject.SetActive(false);
		Instance.menu    .gameObject.SetActive(false);
		Instance.settings.gameObject.SetActive(false);
	}

	public static void SetGame() {
		Instance.mainmenu.gameObject.SetActive(false);
		Instance.game    .gameObject.SetActive(true );
		Instance.menu    .gameObject.SetActive(false);
		Instance.settings.gameObject.SetActive(false);
	}

	public static void OpenMenu() {
		Instance.menu    .gameObject.SetActive(true );
	}

	public static void OpenSettings() {
		Instance.settings.gameObject.SetActive(true );
	}

	public static void Back() {
		if      (Instance.settings.gameObject.activeSelf) Instance.settings.gameObject.SetActive(false);
		else if (Instance.menu    .gameObject.activeSelf) Instance.menu    .gameObject.SetActive(false);
		else if (Instance.game    .gameObject.activeSelf) Instance.menu    .gameObject.SetActive(true );
		else if (Instance.mainmenu.gameObject.activeSelf) {
			#if UNITY_EDITOR
				EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}
	}



	public void UpdateLanguage(CustomStepper stepper) {
		stepper.Length = LocalizationSettings.AvailableLocales.Locales.Count;
		//stepper.text   = LocalizationSettings.AvailableLocales.Locales[stepper.value].name;
	}

	public void SetLanguage(int value) {
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[value];
	}

	public void SetFullScreen(bool value) {
		Screen.fullScreen = value;
	}



	// Cycle

	void Start() {
		mainmenu.gameObject.SetActive(true );
		game.gameObject    .SetActive(false);
		menu.gameObject    .SetActive(false);
		settings.gameObject.SetActive(false);
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) Back();
	}
}

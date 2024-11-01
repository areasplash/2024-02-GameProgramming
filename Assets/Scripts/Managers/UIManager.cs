using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

#if UNITY_EDITOR
using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// UI Manager
// ====================================================================================================

public class UIManager : MonoSingleton<UIManager> {

	// Fields

	[SerializeField] Canvas mainmenu;
	[SerializeField] Canvas game;
	[SerializeField] Canvas menu;
	[SerializeField] Canvas settings;

	[SerializeField] CustomStepper language;
	[SerializeField] CustomStepper screenResolution;
	[SerializeField] Vector2Int    resolution   = new Vector2Int(1280,  720);
	[SerializeField] Vector2Int    reference    = new Vector2Int( 640,  360);
	[SerializeField] Vector2Int[]  presets      = new Vector2Int[] {
		new Vector2Int( 640,  360),
		new Vector2Int(1280,  720),
		new Vector2Int(1920, 1080),
		new Vector2Int(2560, 1440),
		new Vector2Int(3840, 2160),
	};
	[SerializeField] float         pixelPerUnit = 16.0f;
	[SerializeField] bool          pixelPerfect = true;



	// Properties

	public Vector2Int Resolution {
		get => resolution;
		set {
			resolution = value;
			if (Resolution != new Vector2Int(Screen.width, Screen.height)) {
				Screen.SetResolution(Resolution.x, Resolution.y, Screen.fullScreen);
			}
			int multiplier = Mathf.Max(1, Resolution.x / Reference.x, Resolution.y / Reference.y);
			if (TryGetComponent(out CanvasScaler scaler)) scaler.scaleFactor = multiplier;

			bool resized = true;
			resized &= !Screen.fullScreen;
			resized &= !System.Array.Exists(presets, p => p == Resolution);
			if (screenResolution) {
				screenResolution.interactable = !Screen.fullScreen;
				screenResolution.Text         = $"{Resolution.x} x {Resolution.y}";
				screenResolution.Length       = presets.Length;
				screenResolution.Loop         = resized;
			}

			Vector2 size = Resolution;
			if (PixelPerfect) {
				size.x = Mathf.Ceil(Resolution.x / multiplier);
				size.y = Mathf.Ceil(Resolution.y / multiplier);
			}
			if (CameraManager.Instance.MainCamera?.targetTexture) {
				CameraManager.Instance.MainCamera.targetTexture.Release();
				CameraManager.Instance.FadeCamera.targetTexture.Release();
				CameraManager.Instance.MainCamera.targetTexture.width  = (int)size.x;
				CameraManager.Instance.FadeCamera.targetTexture.width  = (int)size.x;
				CameraManager.Instance.MainCamera.targetTexture.height = (int)size.y;
				CameraManager.Instance.FadeCamera.targetTexture.height = (int)size.y;
				CameraManager.Instance.MainCamera.targetTexture.Create();
				CameraManager.Instance.FadeCamera.targetTexture.Create();
				CameraManager.Instance.OrthographicSize = size.y / 2 / PixelPerUnit;
			}
		}
	}

	public Vector2Int Reference {
		get => reference;
		set {
			reference  = value;
			Resolution = Resolution;
		}
	}

	public bool PixelPerfect {
		get => pixelPerfect;
		set {
			pixelPerfect = value;
			Resolution = Resolution;
			if (TryGetComponent(out Canvas canvas)) canvas.pixelPerfect = value;
		}
	}

	public float PixelPerUnit {
		get => pixelPerUnit;
		set {
			pixelPerUnit = value;
			if (TryGetComponent(out Canvas canvas)) canvas.referencePixelsPerUnit = value;
		}
	}



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
				I.mainmenu         = ObjectField    ("Main Menu",         I.mainmenu        );
				I.game             = ObjectField    ("Game",              I.game            );
				I.menu             = ObjectField    ("Menu",              I.menu            );
				I.settings         = ObjectField    ("Settings",          I.settings        );

				Space();
				LabelField("Settings", EditorStyles.boldLabel);
				I.language         = ObjectField    ("Language",          I.language        );
				I.screenResolution = ObjectField    ("Screen Resolution", I.screenResolution);
				Space();
				I.Resolution       = Vector2IntField("Resolution",        I.Resolution      );
				I.Reference        = Vector2IntField("Reference",         I.Reference       );
				PropertyField(serializedObject.FindProperty("presets"));
				I.PixelPerUnit     = FloatField     ("Pixel Per Unit",    I.PixelPerUnit    );
				I.PixelPerfect     = Toggle         ("Pixel Perfect",     I.PixelPerfect    );

				

				if (GUI.changed) EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
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



	static bool IsChangingLanguage { get; set; } = false;

	public static async void SetLanguage(int value) {
		if (IsChangingLanguage) return;
		IsChangingLanguage = true;
		await LocalizationSettings.InitializationOperation.Task;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[value];
		if (Instance.language) {
			Instance.language.Text   = LocalizationSettings.SelectedLocale.name;
			Instance.language.Length = LocalizationSettings.AvailableLocales.Locales.Count;
		}
		IsChangingLanguage = false;
	}



	static Vector2Int TempResolution { get; set; } = new Vector2Int(1280, 720);

	public static void SetFullScreen(bool value) {
		Vector2Int resolution = TempResolution;
		if (value) {
			if (!Screen.fullScreen) TempResolution = new Vector2Int(Screen.width, Screen.height);
			resolution.x = Screen.currentResolution.width;
			resolution.y = Screen.currentResolution.height;
		}
		Instance.Resolution = resolution;
		Screen.fullScreen = value;
	}

	public static void SetScreenResolution(int value) {
		Instance.Resolution = Instance.presets[value];
	}



	public static Vector3 GetPixelated(Vector3 position) {
		position = CameraManager.Instance.transform.InverseTransformDirection(position);
		position.x = Mathf.Round(position.x * Instance.PixelPerUnit) / Instance.PixelPerUnit;
		position.y = Mathf.Round(position.y * Instance.PixelPerUnit) / Instance.PixelPerUnit;
		position.z = Mathf.Round(position.z * Instance.PixelPerUnit) / Instance.PixelPerUnit;
		return CameraManager.Instance.transform.TransformDirection(position);
	}



	// Cycle

	void Start() {
		mainmenu.gameObject.SetActive(true );
		game    .gameObject.SetActive(false);
		menu    .gameObject.SetActive(false);
		settings.gameObject.SetActive(false);

		SetLanguage(0);
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) Back();

		if (Resolution.x != Screen.width || Resolution.y != Screen.height) {
			Resolution = new Vector2Int(Screen.width, Screen.height);
		}
	}
}

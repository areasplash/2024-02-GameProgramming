using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;

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

	[SerializeField] bool          pixelPerfect        = true;
	[SerializeField] float         pixelPerUnit        = 16.0f;
	[SerializeField] Vector2Int    referenceResolution = new Vector2Int(640, 360);
	[SerializeField] Vector2Int[]  presetResolution    = new Vector2Int[] {
		new Vector2Int( 640,  360),
		new Vector2Int(1280,  720),
		new Vector2Int(1920, 1080),
		new Vector2Int(2560, 1440),
		new Vector2Int(3840, 2160),
	};



	// Properties

	public bool PixelPerfect {
		get => pixelPerfect;
		set {
			pixelPerfect = value;
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

	public Vector2Int ReferenceResolution {
		get => referenceResolution;
		set {
			referenceResolution  = value;
		}
	}

	public Vector2Int[] PresetResolution => presetResolution;



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
				I.mainmenu            = ObjectField    ("Main Menu",            I.mainmenu           );
				I.game                = ObjectField    ("Game",                 I.game               );
				I.menu                = ObjectField    ("Menu",                 I.menu               );
				I.settings            = ObjectField    ("Settings",             I.settings           );

				Space();
				LabelField("Display Settings", EditorStyles.boldLabel);
				I.PixelPerfect        = Toggle         ("Pixel Perfect",        I.PixelPerfect       );
				I.PixelPerUnit        = FloatField     ("Pixel Per Unit",       I.PixelPerUnit       );
				I.ReferenceResolution = Vector2IntField("Reference Resolution", I.ReferenceResolution);
				PropertyField(serializedObject.FindProperty("presets"));

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



	static bool IsLanguageChanging { get; set; } = false;

	public static void FreshLanguage(CustomStepper stepper) {
		stepper.Text   = LocalizationSettings.AvailableLocales.Locales[stepper.Value].name;
		stepper.Length = LocalizationSettings.AvailableLocales.Locales.Count;
	}

	public static void SetLanguage(int value) {
		if (IsLanguageChanging == false) {
			IsLanguageChanging = true;
			Instance.StartCoroutine(SetLanguageRoutine(value));
		}
	}

	static IEnumerator SetLanguageRoutine(int value) {
		yield return LocalizationSettings.InitializationOperation;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[value];
		IsLanguageChanging = false;
	}





	static bool          FullScreen         { get; set; } = false;
	static Vector2Int    PreviousResolution { get; set; } = new Vector2Int(1280, 720);
	static Vector2Int    WindowedResolution { get; set; } = new Vector2Int(1280, 720);
	static CustomStepper ScreenResolution   { get; set; } = null;

	public static void FreshScreenResolution(CustomStepper stepper) {
		if (!Instance) return;
		ScreenResolution = stepper;

		int indexLast = System.Array.FindLastIndex(Instance.PresetResolution, preset =>
			preset.x < Screen.currentResolution.width &&
			preset.y < Screen.currentResolution.height);
		int indexNear = System.Array.FindLastIndex(Instance.PresetResolution, preset =>
			preset.x <= PreviousResolution.x &&
			preset.y <= PreviousResolution.y);
		int multiplier = Mathf.Max(1, Mathf.Min(
			PreviousResolution.x / Instance.ReferenceResolution.x,
			PreviousResolution.y / Instance.ReferenceResolution.y));

		if (ScreenResolution) {
			ScreenResolution.interactable = !FullScreen;
			ScreenResolution.Text         = $"{PreviousResolution.x} x {PreviousResolution.y}";
			ScreenResolution.Length       = indexLast + 1;
			ScreenResolution.SetValueForce(Mathf.Min(indexNear, indexLast));
			ScreenResolution.Fresh();
		}
		Vector2 size = PreviousResolution;
		if (Instance.PixelPerfect) {
			size.x = Mathf.Ceil(PreviousResolution.x / multiplier);
			size.y = Mathf.Ceil(PreviousResolution.y / multiplier);
		}
		if (Instance.TryGetComponent(out CanvasScaler scaler)) scaler.scaleFactor = multiplier;

		if (CameraManager.Instance.MainCamera?.targetTexture) {
			CameraManager.Instance.MainCamera.targetTexture.Release();
			CameraManager.Instance.FadeCamera.targetTexture.Release();
			CameraManager.Instance.MainCamera.targetTexture.width  = (int)size.x;
			CameraManager.Instance.FadeCamera.targetTexture.width  = (int)size.x;
			CameraManager.Instance.MainCamera.targetTexture.height = (int)size.y;
			CameraManager.Instance.FadeCamera.targetTexture.height = (int)size.y;
			CameraManager.Instance.MainCamera.targetTexture.Create();
			CameraManager.Instance.FadeCamera.targetTexture.Create();
			CameraManager.Instance.OrthographicSize = size.y / 2 / Instance.PixelPerUnit;
		}
	}

	public static void SetFullScreen(bool value) {
		FullScreen = value;
		Vector2Int resolution = WindowedResolution;
		if (value) {
			if (!Screen.fullScreen) WindowedResolution = new Vector2Int(Screen.width, Screen.height);
			resolution.x = Screen.currentResolution.width;
			resolution.y = Screen.currentResolution.height;
		}
		Screen.SetResolution(resolution.x, resolution.y, value);
	}

	public static void SetScreenResolution(int value) {
		Vector2Int resolution = Instance.PresetResolution[value];
		Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
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
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) Back();

		if (PreviousResolution.x != Screen.width || PreviousResolution.y != Screen.height) {
			PreviousResolution = new Vector2Int(Screen.width, Screen.height);
			FreshScreenResolution(ScreenResolution);
		}
	}
}

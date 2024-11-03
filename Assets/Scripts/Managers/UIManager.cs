using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



#if UNITY_EDITOR
	[CustomEditor(typeof(UIManager)), CanEditMultipleObjects]
	public class UIManagerEditor : Editor {

		SerializedProperty m_MainMenuCanvas;
		SerializedProperty m_GameCanvas;
		SerializedProperty m_BlurCanvas;
		SerializedProperty m_MenuCanvas;
		SerializedProperty m_SettingsCanvas;
		SerializedProperty m_DialogCanvas;
		SerializedProperty m_FadeCanvas;
		SerializedProperty m_ResolutionPresets;

		UIManager I => target as UIManager;

		void OnEnable() {
			m_MainMenuCanvas    = serializedObject.FindProperty("m_MainMenuCanvas");
			m_GameCanvas        = serializedObject.FindProperty("m_GameCanvas");
			m_BlurCanvas        = serializedObject.FindProperty("m_BlurCanvas");
			m_MenuCanvas        = serializedObject.FindProperty("m_MenuCanvas");
			m_SettingsCanvas    = serializedObject.FindProperty("m_SettingsCanvas");
			m_DialogCanvas      = serializedObject.FindProperty("m_DialogCanvas");
			m_FadeCanvas        = serializedObject.FindProperty("m_FadeCanvas");
			m_ResolutionPresets = serializedObject.FindProperty("m_ResolutionPresets");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Space();
			LabelField("UI Primary", EditorStyles.boldLabel);
			PropertyField(m_MainMenuCanvas);
			PropertyField(m_GameCanvas);
			Space();
			LabelField("UI Overlay", EditorStyles.boldLabel);
			PropertyField(m_BlurCanvas);
			PropertyField(m_MenuCanvas);
			PropertyField(m_SettingsCanvas);
			PropertyField(m_DialogCanvas);
			PropertyField(m_FadeCanvas);
			Space();
			LabelField("UI Properties", EditorStyles.boldLabel);
			I.pixelPerfect        = Toggle         ("Pixel Perfect",        I.pixelPerfect       );
			I.pixelPerUnit        = FloatField     ("Pixel Per Unit",       I.pixelPerUnit       );
			I.referenceResolution = Vector2IntField("Reference Resolution", I.referenceResolution);
			PropertyField(m_ResolutionPresets);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// UI Manager
// ====================================================================================================

public class UIManager : MonoSingleton<UIManager> {

	[Serializable] public enum PrimaryCanvas {
		MainMenu = 0,
		Game     = 1,
	};

	[Serializable, Flags] public enum OverlayCanvas {
		None     = 0,
		Blur     = 1 << 0,
		Menu     = 1 << 1,
		Settings = 1 << 2,
		Dialog   = 1 << 3,
		Fade     = 1 << 4,
	};



	// Fields

	[SerializeField] Canvas m_MainMenuCanvas;
	[SerializeField] Canvas m_GameCanvas;
	[SerializeField] Canvas m_BlurCanvas;
	[SerializeField] Canvas m_MenuCanvas;
	[SerializeField] Canvas m_SettingsCanvas;
	[SerializeField] Canvas m_DialogCanvas;
	[SerializeField] Canvas m_FadeCanvas;

	[SerializeField] bool         m_PixelPerfect        = true;
	[SerializeField] float        m_PixelPerUnit        = 16.0f;
	[SerializeField] Vector2Int   m_ReferenceResolution = new Vector2Int(640, 360);
	[SerializeField] Vector2Int[] m_ResolutionPresets   = new Vector2Int[] {
		new Vector2Int( 640,  360),
		new Vector2Int(1280,  720),
		new Vector2Int(1920, 1080),
		new Vector2Int(2560, 1440),
		new Vector2Int(3840, 2160),
	};



	// Properties

	PrimaryCanvas primaryCanvas {
		get {
			PrimaryCanvas value = PrimaryCanvas.MainMenu;
			if (m_MainMenuCanvas.gameObject.activeSelf) value = PrimaryCanvas.MainMenu;
			if (m_GameCanvas    .gameObject.activeSelf) value = PrimaryCanvas.Game;
			return value;
		}
		set {
			m_MainMenuCanvas.gameObject.SetActive(value == PrimaryCanvas.MainMenu);
			m_GameCanvas    .gameObject.SetActive(value == PrimaryCanvas.Game);
		}
	}

	OverlayCanvas overlayCanvas {
		get {
			OverlayCanvas value = OverlayCanvas.None;
			if (m_BlurCanvas    .gameObject.activeSelf) value |= OverlayCanvas.Blur;
			if (m_MenuCanvas    .gameObject.activeSelf) value |= OverlayCanvas.Menu;
			if (m_SettingsCanvas.gameObject.activeSelf) value |= OverlayCanvas.Settings;
			if (m_DialogCanvas  .gameObject.activeSelf) value |= OverlayCanvas.Dialog;
			if (m_FadeCanvas    .gameObject.activeSelf) value |= OverlayCanvas.Fade;
			return value;
		}
		set {
			m_BlurCanvas    .gameObject.SetActive(0 != (value & OverlayCanvas.Blur));
			m_MenuCanvas    .gameObject.SetActive(0 != (value & OverlayCanvas.Menu));
			m_SettingsCanvas.gameObject.SetActive(0 != (value & OverlayCanvas.Settings));
			m_DialogCanvas  .gameObject.SetActive(0 != (value & OverlayCanvas.Dialog));
			m_FadeCanvas    .gameObject.SetActive(0 != (value & OverlayCanvas.Fade));
		}
	}

	Canvas managerCanvas;

	public bool pixelPerfect {
		get => m_PixelPerfect;
		set {
			m_PixelPerfect = value;
			if (managerCanvas || TryGetComponent(out managerCanvas)) {
				managerCanvas.pixelPerfect = value;
			}
		}
	}

	public float pixelPerUnit {
		get => m_PixelPerUnit;
		set {
			m_PixelPerUnit = value;
			if (managerCanvas || TryGetComponent(out managerCanvas)) {
				managerCanvas.referencePixelsPerUnit = value;
			}
		}
	}

	public Vector2Int referenceResolution {
		get => m_ReferenceResolution;
		set {
			m_ReferenceResolution = value;
			UpdateScreenResolution(screenResolution);
		}
	}



	// Methods

	public static Vector3 GetPixelated(Vector3 position) {
		return Instance ? Instance.GetPixelated_Internal(position) : position;
	}
	Vector3 GetPixelated_Internal(Vector3 position) {
		if (!CameraManager.Instance) return position;
		position = CameraManager.Instance.transform.InverseTransformDirection(position);
		position.x = Mathf.Round(position.x * pixelPerUnit) / pixelPerUnit;
		position.y = Mathf.Round(position.y * pixelPerUnit) / pixelPerUnit;
		position.z = Mathf.Round(position.z * pixelPerUnit) / pixelPerUnit;
		return CameraManager.Instance.transform.TransformDirection(position);
	}

	public static void Back() {
		Instance?.Back_Internal();
	}
	void Back_Internal() {
		if (0 != (overlayCanvas & OverlayCanvas.Fade)) {
			return;
		}
		if (0 != (overlayCanvas & OverlayCanvas.Dialog)) {
			overlayCanvas &= ~OverlayCanvas.Dialog;
			if (overlayCanvas == OverlayCanvas.Blur) overlayCanvas = OverlayCanvas.None;
			return;
		}
		if (0 != (overlayCanvas & OverlayCanvas.Settings)) {
			overlayCanvas &= ~OverlayCanvas.Settings;
			if (overlayCanvas == OverlayCanvas.Blur) overlayCanvas = OverlayCanvas.None;
			return;
		}
		if (0 != (overlayCanvas & OverlayCanvas.Menu)) {
			overlayCanvas &= ~OverlayCanvas.Menu;
			if (overlayCanvas == OverlayCanvas.Blur) overlayCanvas = OverlayCanvas.None;
			return;
		}
		if (0 != (overlayCanvas & OverlayCanvas.Blur)) {
			overlayCanvas &= ~OverlayCanvas.Blur;
			return;
		}
		if (primaryCanvas == PrimaryCanvas.Game) {
			OpenMenu();
			return;
		}
		if (primaryCanvas == PrimaryCanvas.MainMenu) {
			OpenDialog("Quit", "Quit Dialog", "Quit", "Cancel");
			dialogPositive.onClick.AddListener(() => Quit());
			return;
		}
	}

	public static void Quit() {
		#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}



	// Cycle

	void Start() {
		primaryCanvas = PrimaryCanvas.MainMenu;
		overlayCanvas = OverlayCanvas.None;
		// Fade In
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) Back();
	}



	// ------------------------------------------------------------------------------------------------
	// Main Menu Scene
	// ------------------------------------------------------------------------------------------------

	public void SetMainMenu() {
		// Fade Out
		primaryCanvas = PrimaryCanvas.MainMenu;
		overlayCanvas = OverlayCanvas.None;
		// Fade In
	}



	// ------------------------------------------------------------------------------------------------
	// Game Scene
	// ------------------------------------------------------------------------------------------------

	public void SetGame() {
		// Fade Out
		primaryCanvas = PrimaryCanvas.Game;
		overlayCanvas = OverlayCanvas.None;
		// Fade In
	}



	// ------------------------------------------------------------------------------------------------
	// Menu Window
	// ------------------------------------------------------------------------------------------------

	public void OpenMenu() {
		overlayCanvas |= OverlayCanvas.Blur | OverlayCanvas.Menu;
	}



	// ------------------------------------------------------------------------------------------------
	// Settings Window
	// ------------------------------------------------------------------------------------------------

	public void OpenSettings() {
		overlayCanvas |= OverlayCanvas.Blur | OverlayCanvas.Settings;
	}



	bool isLanguageChanging = false;
	int  langaugeIndex      = -1;

	public void UpdateLanguage(CustomStepper stepper) {
		if (langaugeIndex == -1) langaugeIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
		if (langaugeIndex == -1) langaugeIndex = 0;
		Locale locale = LocalizationSettings.AvailableLocales.Locales[langaugeIndex];
		stepper.text  = locale.Identifier.CultureInfo.NativeName;
		//stepper.text  = LocalizationSettings.SelectedLocale.Identifier.CultureInfo.NativeName;
	}

	public void SetLanguage(int value) {
		if (isLanguageChanging == false) {
			isLanguageChanging = true;
			langaugeIndex = (int)Mathf.Repeat(langaugeIndex + value, LocalizationSettings.AvailableLocales.Locales.Count);
			StartCoroutine(SetLanguageRoutine(langaugeIndex));
		}
	}
	IEnumerator SetLanguageRoutine(int value) {
		yield return LocalizationSettings.InitializationOperation;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[value];
		isLanguageChanging = false;
	}



	bool          fullScreen            = false;
	Vector2Int    previousResolution    = new Vector2Int(1280, 720);
	Vector2Int    windowedResolution    = new Vector2Int(1280, 720);
	CustomStepper screenResolution;
	int           screenResolutionIndex = 1;
	CanvasScaler  managerCanvasScaler;

	public void UpdateScreenResolution(CustomStepper stepper) {
		screenResolution = stepper;

		int index = Array.FindIndex(m_ResolutionPresets, preset =>
			preset.x == previousResolution.x &&
			preset.y == previousResolution.y);
		int indexNear = Array.FindLastIndex(m_ResolutionPresets, preset =>
			preset.x <= previousResolution.x &&
			preset.y <= previousResolution.y);
		int indexLast = Array.FindLastIndex(m_ResolutionPresets, preset =>
			preset.x < Screen.currentResolution.width &&
			preset.y < Screen.currentResolution.height);
		int multiplier = Mathf.Max(1, Mathf.Min(
			previousResolution.x / Instance.referenceResolution.x,
			previousResolution.y / Instance.referenceResolution.y));

		if (managerCanvasScaler || TryGetComponent(out managerCanvasScaler)) {
			managerCanvasScaler.scaleFactor = multiplier;
		}
		if (screenResolution) {
			screenResolution.interactable = !fullScreen;
			screenResolution.canMovePrev  = !fullScreen && index != 0;
			screenResolution.canMoveNext  = !fullScreen && indexNear != indexLast;
			screenResolution.text         = $"{previousResolution.x} x {previousResolution.y}";
		}
		Vector2Int size = previousResolution;
		if (pixelPerfect) {
			size.x = (int)Mathf.Ceil(previousResolution.x / multiplier);
			size.y = (int)Mathf.Ceil(previousResolution.y / multiplier);
		}
		if (CameraManager.Instance) {
			CameraManager.Instance.renderTextureSize = size;
			CameraManager.Instance.orthographicSize  = size.y / 2 / pixelPerUnit;
		}
	}

	public void SetFullScreen(bool value) {
		fullScreen = value;
		Vector2Int resolution = windowedResolution;
		if (value) {
			if (!Screen.fullScreen) windowedResolution = new Vector2Int(Screen.width, Screen.height);
			resolution.x = Screen.currentResolution.width;
			resolution.y = Screen.currentResolution.height;
		}
		Screen.SetResolution(resolution.x, resolution.y, value);
	}

	public void SetScreenResolution(int value) {
		int index = Array.FindIndex(m_ResolutionPresets, preset =>
			preset.x == previousResolution.x &&
			preset.y == previousResolution.y);
		int indexLast = Array.FindLastIndex(m_ResolutionPresets, preset =>
			preset.x < Screen.currentResolution.width &&
			preset.y < Screen.currentResolution.height);

		if (index == -1 && value == -1) value = 0;
		screenResolutionIndex = Mathf.Clamp(screenResolutionIndex + value, 0, indexLast);
		Vector2Int resolution = m_ResolutionPresets[screenResolutionIndex];
		Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
	}

	void LateUpdate() {
		if (previousResolution.x != Screen.width || previousResolution.y != Screen.height) {
			previousResolution = new Vector2Int(Screen.width, Screen.height);
			UpdateScreenResolution(screenResolution);
		}
	}



	// ------------------------------------------------------------------------------------------------
	// Dialog Window
	// ------------------------------------------------------------------------------------------------

	CustomText   dialogTitle;
	CustomText   dialogMessage;
	CustomButton dialogPositive;
	CustomButton dialogNegative;

	public void UpdateDialogTitle   (CustomText   text  ) => dialogTitle    = text;
	public void UpdateDialogMessage (CustomText   text  ) => dialogMessage  = text;
	public void UpdateDialogPositive(CustomButton button) => dialogPositive = button;
	public void UpdateDialogNegative(CustomButton button) => dialogNegative = button;



	public void OpenDialog(string arg0, string arg1, string arg2, string arg3) {
		overlayCanvas |= OverlayCanvas.Blur | OverlayCanvas.Dialog;
		dialogTitle   ?.SetLocalizeText("UI Table", arg0);
		dialogMessage ?.SetLocalizeText("UI Table", arg1);
		dialogPositive?.SetLocalizeText("UI Table", arg2);
		dialogNegative?.SetLocalizeText("UI Table", arg3);
		dialogPositive?.onClick?.RemoveAllListeners();
		dialogNegative?.onClick?.RemoveAllListeners();
		dialogPositive?.onClick?.AddListener(() => Back());
		dialogNegative?.onClick?.AddListener(() => Back());
	}
}

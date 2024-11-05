using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;

using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// UI Manager Editor
// ====================================================================================================

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

		SerializedProperty m_MainMenuFirstSelected;
		SerializedProperty m_GameFirstSelected;
		SerializedProperty m_BlurFirstSelected;
		SerializedProperty m_MenuFirstSelected;
		SerializedProperty m_SettingsFirstSelected;
		SerializedProperty m_DialogFirstSelected;
		SerializedProperty m_FadeFirstSelected;

		SerializedProperty m_ResolutionPresets;

		UIManager I => target as UIManager;

		void OnEnable() {
			m_MainMenuCanvas = serializedObject.FindProperty("m_MainMenuCanvas");
			m_GameCanvas     = serializedObject.FindProperty("m_GameCanvas");
			m_BlurCanvas     = serializedObject.FindProperty("m_BlurCanvas");
			m_MenuCanvas     = serializedObject.FindProperty("m_MenuCanvas");
			m_SettingsCanvas = serializedObject.FindProperty("m_SettingsCanvas");
			m_DialogCanvas   = serializedObject.FindProperty("m_DialogCanvas");
			m_FadeCanvas     = serializedObject.FindProperty("m_FadeCanvas");

			m_MainMenuFirstSelected = serializedObject.FindProperty("m_MainMenuFirstSelected");
			m_GameFirstSelected     = serializedObject.FindProperty("m_GameFirstSelected");
			m_BlurFirstSelected     = serializedObject.FindProperty("m_BlurFirstSelected");
			m_MenuFirstSelected     = serializedObject.FindProperty("m_MenuFirstSelected");
			m_SettingsFirstSelected = serializedObject.FindProperty("m_SettingsFirstSelected");
			m_DialogFirstSelected   = serializedObject.FindProperty("m_DialogFirstSelected");
			m_FadeFirstSelected     = serializedObject.FindProperty("m_FadeFirstSelected");

			m_ResolutionPresets = serializedObject.FindProperty("m_ResolutionPresets");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Space();
			LabelField("Canvas", EditorStyles.boldLabel);
			PropertyField(m_MainMenuCanvas);
			PropertyField(m_GameCanvas);
			Space();
			PropertyField(m_BlurCanvas);
			PropertyField(m_MenuCanvas);
			PropertyField(m_SettingsCanvas);
			PropertyField(m_DialogCanvas);
			PropertyField(m_FadeCanvas);
			Space();
			LabelField("First Selected", EditorStyles.boldLabel);
			PropertyField(m_MainMenuFirstSelected);
			PropertyField(m_GameFirstSelected);
			Space();
			PropertyField(m_BlurFirstSelected);
			PropertyField(m_MenuFirstSelected);
			PropertyField(m_SettingsFirstSelected);
			PropertyField(m_DialogFirstSelected);
			PropertyField(m_FadeFirstSelected);
			Space();
			LabelField("UI Properties", EditorStyles.boldLabel);
			I.pixelPerfect        = Toggle         ("Pixel Perfect",        I.pixelPerfect);
			I.pixelPerUnit        = FloatField     ("Pixel Per Unit",       I.pixelPerUnit);
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

	static readonly Vector2Int DefaultResolution = new Vector2Int(1280, 720);

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

	[SerializeField] Selectable m_MainMenuFirstSelected;
	[SerializeField] Selectable m_GameFirstSelected;
	[SerializeField] Selectable m_BlurFirstSelected;
	[SerializeField] Selectable m_MenuFirstSelected;
	[SerializeField] Selectable m_SettingsFirstSelected;
	[SerializeField] Selectable m_DialogFirstSelected;
	[SerializeField] Selectable m_FadeFirstSelected;

	[SerializeField] bool         m_PixelPerfect        = true;
	[SerializeField] float        m_PixelPerUnit        = 16.0f;
	[SerializeField] Vector2Int   m_ReferenceResolution = DefaultResolution;
	[SerializeField] Vector2Int[] m_ResolutionPresets   = new Vector2Int[] {
		new Vector2Int( 640,  360),
		new Vector2Int(1280,  720),
		new Vector2Int(1920, 1080),
		new Vector2Int(2560, 1440),
		new Vector2Int(3840, 2160),
	};

	[SerializeField] string       m_Language   = null;
	[SerializeField] bool         m_FullScreen = false;
	[SerializeField] float        m_Music      = 1.0f;
	[SerializeField] float        m_SoundFX    = 1.0f;

	[SerializeField] float        m_MouseSensitivity = 1.0f;
	[SerializeField] List<string> m_KeysMoveUp       = new List<string>{ "upArrow"     };
	[SerializeField] List<string> m_KeysMoveDown     = new List<string>{ "leftArrow"   };
	[SerializeField] List<string> m_KeysMoveLeft     = new List<string>{ "downArrow"   };
	[SerializeField] List<string> m_KeysMoveRight    = new List<string>{ "rightArrow"  };
	[SerializeField] List<string> m_KeysInteract     = new List<string>{ "z", "enter"  };
	[SerializeField] List<string> m_KeysCancel       = new List<string>{ "x", "escape" };



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

	List<string> ToKeys(string data) {
		List<string> keys = new List<string>();
		foreach (string key in data.Split(", ")) keys.Add(key);
		return keys;
	}

	string ToData(List<string> keys) {
		string data = null;
		for (int i = 0; i < keys.Count; i++) data += keys[i] + (i != keys.Count - 1 ? ", " : "");
		return data;
	}

	void LoadSettings() {
		m_Language         = PlayerPrefs.GetString("Language",          m_Language);
		m_FullScreen	   = PlayerPrefs.GetInt   ("FullScreen",        m_FullScreen ? 1 : 0) != 0;
		m_Music            = PlayerPrefs.GetFloat ("Music",             m_Music);
		m_SoundFX          = PlayerPrefs.GetFloat ("SoundFX",           m_SoundFX);
		m_MouseSensitivity = PlayerPrefs.GetFloat ("MouseSensitivity",  m_MouseSensitivity);

		m_KeysMoveUp    = ToKeys(PlayerPrefs.GetString("KeysMoveUp",    ToData(m_KeysMoveUp)));
		m_KeysMoveDown  = ToKeys(PlayerPrefs.GetString("KeysMoveDown",  ToData(m_KeysMoveDown)));
		m_KeysMoveLeft  = ToKeys(PlayerPrefs.GetString("KeysMoveLeft",  ToData(m_KeysMoveLeft)));
		m_KeysMoveRight = ToKeys(PlayerPrefs.GetString("KeysMoveRight", ToData(m_KeysMoveRight)));
		m_KeysInteract  = ToKeys(PlayerPrefs.GetString("KeysInteract",  ToData(m_KeysInteract)));
		m_KeysCancel    = ToKeys(PlayerPrefs.GetString("KeysCancel",    ToData(m_KeysCancel)));

		UpdateLanguage(null);
		UpdateFullScreen(null);
		UpdateScreenResolution(null);
	}

	void SaveSettings() {
		PlayerPrefs.SetString("Language",         m_Language);
		PlayerPrefs.SetInt   ("FullScreen",       m_FullScreen ? 1 : 0);
		PlayerPrefs.SetFloat ("Music",            m_Music);
		PlayerPrefs.SetFloat ("SoundFX",          m_SoundFX);
		PlayerPrefs.SetFloat ("MouseSensitivity", m_MouseSensitivity);

		PlayerPrefs.SetString("KeysMoveUp",    ToData(m_KeysMoveUp));
		PlayerPrefs.SetString("KeysMoveDown",  ToData(m_KeysMoveDown));
		PlayerPrefs.SetString("KeysMoveLeft",  ToData(m_KeysMoveLeft));
		PlayerPrefs.SetString("KeysMoveRight", ToData(m_KeysMoveRight));
		PlayerPrefs.SetString("KeysInteract",  ToData(m_KeysInteract));
		PlayerPrefs.SetString("KeysCancel",    ToData(m_KeysCancel));

		PlayerPrefs.Save();
	}



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
			SaveSettings();
			overlayCanvas &= ~OverlayCanvas.Settings;
			if (primaryCanvas == PrimaryCanvas.Game) OpenMenu();
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
			dialogPositive?.onClick.RemoveAllListeners();
			dialogPositive?.onClick.AddListener(() => Quit());
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
		LoadSettings();
		primaryCanvas = PrimaryCanvas.MainMenu;
		overlayCanvas = OverlayCanvas.None;
		// Fade In
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
		overlayCanvas &= ~OverlayCanvas.Menu;
	}



	int languageIndex = -1;

	List<Locale> locales => LocalizationSettings.AvailableLocales.Locales;

	public void UpdateLanguage(CustomStepper stepper) {
		if (languageIndex == -1) {
			m_Language ??= Application.systemLanguage.ToString();
			languageIndex = Mathf.Max(0, locales.FindIndex(locale => {
				return locale.Identifier.CultureInfo.NativeName.Equals(m_Language);
			}));
		}
		LocalizationSettings.SelectedLocale = locales[languageIndex];
		if (stepper) stepper.text = m_Language;
	}

	public void SetLanguage(int value) {
		languageIndex = (int)Mathf.Repeat(languageIndex + value, locales.Count);
		m_Language = locales[languageIndex].Identifier.CultureInfo.NativeName;
	}



	Vector2Int windowedResolution = DefaultResolution;

	public void UpdateFullScreen(CustomToggle toggle) {
		if (toggle) toggle.value = m_FullScreen;
	}

	public void SetFullScreen(bool value) {
		m_FullScreen = value;
		Vector2Int resolution = windowedResolution;
		if (m_FullScreen) {
			if (!Screen.fullScreen) windowedResolution = new Vector2Int(Screen.width, Screen.height);
			resolution.x = Screen.currentResolution.width;
			resolution.y = Screen.currentResolution.height;
		}
		Screen.SetResolution(resolution.x, resolution.y, m_FullScreen);
		UpdateScreenResolution(screenResolution);
	}



	CustomStepper screenResolution;
	CanvasScaler  managerCanvasScaler;

	int screenIndex => Array.FindIndex(m_ResolutionPresets, preset =>
		preset.x == Screen.width &&
		preset.y == Screen.height);
	int screenIndexFloor => Array.FindLastIndex(m_ResolutionPresets, preset =>
		preset.x <= Screen.width &&
		preset.y <= Screen.height);
	int screenIndexMax => Array.FindLastIndex(m_ResolutionPresets, preset =>
		preset.x < Screen.currentResolution.width &&
		preset.y < Screen.currentResolution.height);
	int multiplier => Mathf.Max(1, Mathf.Min(
		Screen.width  / referenceResolution.x,
		Screen.height / referenceResolution.y));

	public void UpdateScreenResolution(CustomStepper stepper = null) {
		screenResolution = stepper;
		if (screenResolution) {
			stepper.text = $"{Screen.width} x {Screen.height}";
			stepper.interactable = !m_FullScreen;
			stepper.canMovePrev  = !m_FullScreen && screenIndex != 0 && screenIndexFloor != -1;
			stepper.canMoveNext  = !m_FullScreen && screenIndexFloor < screenIndexMax;
		}
		if (managerCanvasScaler || TryGetComponent(out managerCanvasScaler)) {
			managerCanvasScaler.scaleFactor = multiplier;
		}
		if (CameraManager.Instance) {
			Vector2Int size = new Vector2Int(Screen.width, Screen.height);
			if (pixelPerfect) {
				size.x = (int)Mathf.Ceil(Screen.width  / multiplier);
				size.y = (int)Mathf.Ceil(Screen.height / multiplier);
			}
			CameraManager.Instance.renderTextureSize = size;
			CameraManager.Instance.orthographicSize  = size.y / 2 / pixelPerUnit;
		}
	}

	public void SetScreenResolution(int value) {
		if (m_FullScreen) return;
		if (screenIndex == -1 && value == -1) value = 0;
		Vector2Int resolution = m_ResolutionPresets[Mathf.Min(screenIndexFloor + value, screenIndexMax)];
		Screen.SetResolution(resolution.x, resolution.y, m_FullScreen);
	}

	Vector2Int cacheResolution = DefaultResolution;

	void LateUpdate() {
		if (cacheResolution.x != Screen.width || cacheResolution.y != Screen.height) {
			cacheResolution = new Vector2Int(Screen.width, Screen.height);
			UpdateScreenResolution(screenResolution);
		}
	}



	public void UpdateMusic  (CustomSlider slider) => slider.value = m_Music;
	public void UpdateSoundFX(CustomSlider slider) => slider.value = m_SoundFX;

	public void SetMusic  (float value) => m_Music   = value;
	public void SetSoundFX(float value) => m_SoundFX = value;

	public void UpdateMouseSensitivity(CustomSlider slider) => slider.value = m_MouseSensitivity;
	public void SetMouseSensitivity(float value) => m_MouseSensitivity = value;

	void UpdateKeys(CustomButton button, KeyAction keyAction) {
		button.text = ToData(InputManager.GetKeysBinding(keyAction));
	}

	public void UpdateMoveUp   (CustomButton button) => UpdateKeys(button, KeyAction.MoveUp);
	public void UpdateMoveLeft (CustomButton button) => UpdateKeys(button, KeyAction.MoveLeft);
	public void UpdateMoveDown (CustomButton button) => UpdateKeys(button, KeyAction.MoveDown);
	public void UpdateMoveRight(CustomButton button) => UpdateKeys(button, KeyAction.MoveRight);
	public void UpdateInteract (CustomButton button) => UpdateKeys(button, KeyAction.Interact);
	public void UpdateCancel   (CustomButton button) => UpdateKeys(button, KeyAction.Cancel);

	void SetKeys(KeyAction keyAction) {
		OpenDialog("Key Binding", "Key Binding Dialog", "Save", "Cancel");
	}

	public void SetMoveUp   () => SetKeys(KeyAction.MoveUp);
	public void SetMoveLeft () => SetKeys(KeyAction.MoveLeft);
	public void SetMoveDown () => SetKeys(KeyAction.MoveDown);
	public void SetMoveRight() => SetKeys(KeyAction.MoveRight);
	public void SetInteract () => SetKeys(KeyAction.Interact);
	public void SetCancel   () => SetKeys(KeyAction.Cancel);



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
		dialogPositive?.onClick.RemoveAllListeners();
		dialogNegative?.onClick.RemoveAllListeners();
		dialogPositive?.onClick.AddListener(() => Back());
		dialogNegative?.onClick.AddListener(() => Back());
	}



	// ------------------------------------------------------------------------------------------------
	// Fade Window
	// ------------------------------------------------------------------------------------------------

	int   fadeState = 0;
	Image fadeImage;

	public void FadeOut() {
		if (fadeImage || m_FadeCanvas.TryGetComponent(out fadeImage)) {
			overlayCanvas |= OverlayCanvas.Fade;
			fadeState = 1;
		}
	}

	public void FadeIn() {
		if (fadeImage || m_FadeCanvas.TryGetComponent(out fadeImage)) {
			overlayCanvas |= OverlayCanvas.Fade;
			fadeState = 2;
		}
	}

	void Update() {
		if (fadeState != 0) {
			Color color = fadeImage.color;
			color.a = Mathf.MoveTowards(color.a, fadeState == 1 ? 1.0f : 0.0f, Time.deltaTime);
			fadeImage.color = color;
			if (color.a == 0.0f) {
				overlayCanvas &= ~OverlayCanvas.Fade;
				fadeState = 0;
			}
		}
	}
}

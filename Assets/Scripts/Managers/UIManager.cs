using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

using System;
using System.Collections;
using System.Collections.Generic;

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
		SerializedProperty m_DialogueCanvas;
		SerializedProperty m_MenuCanvas;
		SerializedProperty m_SettingsCanvas;
		SerializedProperty m_ConfirmationCanvas;
		SerializedProperty m_FadeCanvas;

		SerializedProperty m_MainMenuFirstSelected;
		SerializedProperty m_GameFirstSelected;
		SerializedProperty m_DialogueFirstSelected;
		SerializedProperty m_MenuFirstSelected;
		SerializedProperty m_SettingsFirstSelected;
		SerializedProperty m_ConfirmationFirstSelected;
		SerializedProperty m_FadeFirstSelected;

		SerializedProperty m_Canvas;
		SerializedProperty m_CanvasScaler;
		SerializedProperty m_ResolutionPresets;

		UIManager I => target as UIManager;

		void OnEnable() {
			m_MainMenuCanvas     = serializedObject.FindProperty("m_MainMenuCanvas");
			m_GameCanvas         = serializedObject.FindProperty("m_GameCanvas");
			m_DialogueCanvas     = serializedObject.FindProperty("m_DialogueCanvas");
			m_MenuCanvas         = serializedObject.FindProperty("m_MenuCanvas");
			m_SettingsCanvas     = serializedObject.FindProperty("m_SettingsCanvas");
			m_ConfirmationCanvas = serializedObject.FindProperty("m_ConfirmationCanvas");
			m_FadeCanvas         = serializedObject.FindProperty("m_FadeCanvas");

			m_MainMenuFirstSelected     = serializedObject.FindProperty("m_MainMenuFirstSelected");
			m_GameFirstSelected         = serializedObject.FindProperty("m_GameFirstSelected");
			m_DialogueFirstSelected     = serializedObject.FindProperty("m_DialogueFirstSelected");
			m_MenuFirstSelected         = serializedObject.FindProperty("m_MenuFirstSelected");
			m_SettingsFirstSelected     = serializedObject.FindProperty("m_SettingsFirstSelected");
			m_ConfirmationFirstSelected = serializedObject.FindProperty("m_ConfirmationFirstSelected");
			m_FadeFirstSelected         = serializedObject.FindProperty("m_FadeFirstSelected");

			m_Canvas            = serializedObject.FindProperty("m_Canvas");
			m_CanvasScaler      = serializedObject.FindProperty("m_CanvasScaler");
			m_ResolutionPresets = serializedObject.FindProperty("m_ResolutionPresets");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Space();
			LabelField("Canvas", EditorStyles.boldLabel);
			PropertyField(m_MainMenuCanvas);
			PropertyField(m_GameCanvas);
			PropertyField(m_DialogueCanvas);
			PropertyField(m_MenuCanvas);
			PropertyField(m_SettingsCanvas);
			PropertyField(m_ConfirmationCanvas);
			PropertyField(m_FadeCanvas);
			Space();
			LabelField("First Selected", EditorStyles.boldLabel);
			PropertyField(m_MainMenuFirstSelected);
			PropertyField(m_GameFirstSelected);
			PropertyField(m_DialogueFirstSelected);
			PropertyField(m_MenuFirstSelected);
			PropertyField(m_SettingsFirstSelected);
			PropertyField(m_ConfirmationFirstSelected);
			PropertyField(m_FadeFirstSelected);
			Space();
			LabelField("UI", EditorStyles.boldLabel);
			PropertyField(m_Canvas);
			PropertyField(m_CanvasScaler);
			Space();
			LabelField("UI Properties", EditorStyles.boldLabel);
			I.PixelPerfect        = Toggle         ("Pixel Perfect",        I.PixelPerfect);
			I.PixelPerUnit        = FloatField     ("Pixel Per Unit",       I.PixelPerUnit);
			I.ReferenceResolution = Vector2IntField("Reference Resolution", I.ReferenceResolution);
			PropertyField(m_ResolutionPresets);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// UI Manager
// ====================================================================================================

[Serializable, Flags] public enum CanvasType {
	None	     = 0,
	MainMenu     = 1 << 0,
	Game         = 1 << 1,
	Dialogue     = 1 << 2,
	Menu         = 1 << 3,
	Settings     = 1 << 4,
	Confirmation = 1 << 5,
	Fade         = 1 << 6,
}



public class UIManager : MonoSingleton<UIManager> {
	
	// Fields

	[SerializeField] Canvas m_MainMenuCanvas;
	[SerializeField] Canvas m_GameCanvas;
	[SerializeField] Canvas m_DialogueCanvas;
	[SerializeField] Canvas m_MenuCanvas;
	[SerializeField] Canvas m_SettingsCanvas;
	[SerializeField] Canvas m_ConfirmationCanvas;
	[SerializeField] Canvas m_FadeCanvas;

	[SerializeField] GameObject m_MainMenuFirstSelected;
	[SerializeField] GameObject m_GameFirstSelected;
	[SerializeField] GameObject m_DialogueFirstSelected;
	[SerializeField] GameObject m_MenuFirstSelected;
	[SerializeField] GameObject m_SettingsFirstSelected;
	[SerializeField] GameObject m_ConfirmationFirstSelected;
	[SerializeField] GameObject m_FadeFirstSelected;

	[SerializeField] Canvas       m_Canvas;
	[SerializeField] CanvasScaler m_CanvasScaler;

	[SerializeField] bool         m_PixelPerfect        = true;
	[SerializeField] float        m_PixelPerUnit        = 16.0f;
	[SerializeField] Vector2Int   m_ReferenceResolution = new(640, 360);
	[SerializeField] Vector2Int[] m_ResolutionPresets   = new Vector2Int[] {
		new( 640,  360),
		new(1280,  720),
		new(1920, 1080),
		new(2560, 1440),
		new(3840, 2160),
	};

	[SerializeField] string m_Language;
	[SerializeField] float  m_Music;
	[SerializeField] float  m_SoundFX;
	[SerializeField] float  m_MouseSensitivity;



	// Properties

	Canvas MainMenu     => m_MainMenuCanvas;
	Canvas Game         => m_GameCanvas;
	Canvas Dialogue     => m_DialogueCanvas;
	Canvas Menu         => m_MenuCanvas;
	Canvas Settings     => m_SettingsCanvas;
	Canvas Confirmation => m_ConfirmationCanvas;
	Canvas Fade         => m_FadeCanvas;

	public CanvasType ActiveCanvas {
		get {
			CanvasType value = CanvasType.None;
			if (MainMenu     && MainMenu    .enabled) value |= CanvasType.MainMenu;
			if (Game         && Game        .enabled) value |= CanvasType.Game;
			if (Dialogue     && Dialogue    .enabled) value |= CanvasType.Dialogue;
			if (Menu         && Menu        .enabled) value |= CanvasType.Menu;
			if (Settings     && Settings    .enabled) value |= CanvasType.Settings;
			if (Confirmation && Confirmation.enabled) value |= CanvasType.Confirmation;
			if (Fade         && Fade        .enabled) value |= CanvasType.Fade;
			return value;
		}
		private set {
			if (MainMenu    ) MainMenu    .enabled = 0 != (value & CanvasType.MainMenu);
			if (Game        ) Game        .enabled = 0 != (value & CanvasType.Game);
			if (Dialogue    ) Dialogue    .enabled = 0 != (value & CanvasType.Dialogue);
			if (Menu        ) Menu        .enabled = 0 != (value & CanvasType.Menu);
			if (Settings    ) Settings    .enabled = 0 != (value & CanvasType.Settings);
			if (Confirmation) Confirmation.enabled = 0 != (value & CanvasType.Confirmation);
			if (Fade        ) Fade        .enabled = 0 != (value & CanvasType.Fade);
		}
	}

	CanvasType HighestCanvas {
		get {
			CanvasType value = CanvasType.None;
			if (MainMenu     && MainMenu    .enabled) value = CanvasType.MainMenu;
			if (Game         && Game        .enabled) value = CanvasType.Game;
			if (Dialogue     && Dialogue    .enabled) value = CanvasType.Dialogue;
			if (Menu         && Menu        .enabled) value = CanvasType.Menu;
			if (Settings     && Settings    .enabled) value = CanvasType.Settings;
			if (Confirmation && Confirmation.enabled) value = CanvasType.Confirmation;
			if (Fade         && Fade        .enabled) value = CanvasType.Fade;
			return value;
		}
	}



	GameObject MainMenuFirstSelected     => m_MainMenuFirstSelected;
	GameObject GameFirstSelected         => m_GameFirstSelected;
	GameObject DialogueFirstSelected     => m_DialogueFirstSelected;
	GameObject MenuFirstSelected         => m_MenuFirstSelected;
	GameObject SettingsFirstSelected     => m_SettingsFirstSelected;
	GameObject ConfirmationFirstSelected => m_ConfirmationFirstSelected;
	GameObject FadeFirstSelected         => m_FadeFirstSelected;
	
	GameObject FirstSelected => HighestCanvas switch {
		CanvasType.MainMenu     => MainMenuFirstSelected,
		CanvasType.Game         => GameFirstSelected,
		CanvasType.Dialogue     => DialogueFirstSelected,
		CanvasType.Menu         => MenuFirstSelected,
		CanvasType.Settings     => SettingsFirstSelected,
		CanvasType.Confirmation => ConfirmationFirstSelected,
		CanvasType.Fade         => FadeFirstSelected,
		_                       => null,
	};

	GameObject MainMenuLastSelected     { get; set; }
	GameObject GameLastSelected         { get; set; }
	GameObject DialogueLastSelected     { get; set; }
	GameObject MenuLastSelected         { get; set; }
	GameObject SettingsLastSelected     { get; set; }
	GameObject ConfirmationLastSelected { get; set; }
	GameObject FadeLastSelected         { get; set; }

	GameObject LastSelected {
		get => HighestCanvas switch {
			CanvasType.MainMenu     => MainMenuLastSelected,
			CanvasType.Game         => GameLastSelected,
			CanvasType.Dialogue     => DialogueLastSelected,
			CanvasType.Menu         => MenuLastSelected,
			CanvasType.Settings     => SettingsLastSelected,
			CanvasType.Confirmation => ConfirmationLastSelected,
			CanvasType.Fade         => FadeLastSelected,
			_                       => null,
		};
		set {
			switch (HighestCanvas) {
				case CanvasType.MainMenu:     MainMenuLastSelected     = value; break;
				case CanvasType.Game:         GameLastSelected         = value; break;
				case CanvasType.Dialogue:     DialogueLastSelected     = value; break;
				case CanvasType.Menu:         MenuLastSelected         = value; break;
				case CanvasType.Settings:     SettingsLastSelected     = value; break;
				case CanvasType.Confirmation: ConfirmationLastSelected = value; break;
				case CanvasType.Fade:         FadeLastSelected         = value; break;
			}
		}
	}

	GameObject Selected {
		get =>    EventSystem.current? EventSystem.current.currentSelectedGameObject : null;
		set { if (EventSystem.current) EventSystem.current.SetSelectedGameObject(value); }
	}



	Canvas Canvas => m_Canvas;

	CanvasScaler CanvasScaler => m_CanvasScaler;

	public bool PixelPerfect {
		get => m_PixelPerfect;
		set {
			m_PixelPerfect = value;
			if (Canvas) Canvas.pixelPerfect = value;
		}
	}

	public float PixelPerUnit {
		get => m_PixelPerUnit;
		set {
			m_PixelPerUnit = value;
			if (Canvas) Canvas.referencePixelsPerUnit = value;
		}
	}

	public Vector2Int ReferenceResolution {
		get => m_ReferenceResolution;
		set {
			m_ReferenceResolution = value;
			UpdateScreenResolution(screenResolution);
		}
	}

	string Language {
		get => m_Language;
		set => m_Language = value;
	}

	float Music {
		get => m_Music;
		set => m_Music = value;
	}

	float SoundFX {
		get => m_SoundFX;
		set => m_SoundFX = value;
	}

	public float MouseSensitivity {
		get         => m_MouseSensitivity;
		private set => m_MouseSensitivity = value;
	}



	// Methods

	readonly Stack<CanvasType> stack = new();

	void SaveSelected() => LastSelected = FirstSelected? Selected : null;
	void LoadSelected() => Selected = LastSelected? LastSelected : FirstSelected;

	void Open(CanvasType canvas) {
		SaveSelected();
		CanvasType primary = ActiveCanvas & (CanvasType.MainMenu | CanvasType.Game);
		CanvasType fade    = ActiveCanvas & CanvasType.Fade;
		switch (canvas) {
			case CanvasType.MainMenu:
				stack.Clear();
				stack.Push(ActiveCanvas);
				ActiveCanvas = canvas | fade;
				break;
			
			case CanvasType.Game:
				stack.Clear();
				stack.Push(ActiveCanvas);
				ActiveCanvas = canvas | fade;
				break;
			
			case CanvasType.Dialogue:
				stack.Push(ActiveCanvas);
				ActiveCanvas = primary | canvas | fade;
				break;

			case CanvasType.Menu:
				stack.Push(ActiveCanvas);
				ActiveCanvas = primary | canvas | fade;
				break;

			case CanvasType.Settings:
				stack.Push(ActiveCanvas);
				ActiveCanvas = primary | canvas | fade;
				break;

			case CanvasType.Confirmation:
				stack.Push(ActiveCanvas);
				ActiveCanvas = primary | canvas | fade;
				break;
			
			case CanvasType.Fade:
				break;
		}
		LoadSelected();
	}

	public void Back() {
		SaveSelected();
		switch (HighestCanvas) {
			case CanvasType.MainMenu:
				OpenConfirmation("Quit", "Quit Message", "Quit", "Cancel");
				if (confirmationPositive) {
					confirmationPositive.OnClick.RemoveAllListeners();
					confirmationPositive.OnClick.AddListener(() => Quit());
				}
				break;
			
			case CanvasType.Game:
				OpenMenu();
				break;
			
			case CanvasType.Dialogue:
				ActiveCanvas = stack.Pop();
				break;

			case CanvasType.Menu:
				ActiveCanvas = stack.Pop();
				break;

			case CanvasType.Settings:
				SaveSettings();
				ActiveCanvas = stack.Pop();
				break;

			case CanvasType.Confirmation:
				ActiveCanvas = stack.Pop();
				break;
			
			case CanvasType.Fade:
				break;
		}
		LoadSelected();
	}

	public void Quit() {
		#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}



	public Vector3 GetPixelated(Vector3 position) {
		if (!CameraManager.I) return position;
		position = CameraManager.I.transform.InverseTransformDirection(position);
		position.x = Mathf.Round(position.x * PixelPerUnit) / PixelPerUnit;
		position.y = Mathf.Round(position.y * PixelPerUnit) / PixelPerUnit;
		position.z = Mathf.Round(position.z * PixelPerUnit) / PixelPerUnit;
		return CameraManager.I.transform.TransformDirection(position);
	}



	string defaultMoveUp;
	string defaultMoveLeft;
	string defaultMoveDown;
	string defaultMoveRight;
	string defaultInteract;
	string defaultCancel;

	List<string> ToKeys(string str) {
		List<string> keys = new();
		foreach (string key in str.Split(", ")) if (key != string.Empty) keys.Add(key);
		return keys;
	}

	string ToString(List<string> keys) {
		string str = "";
		for (int i = 0; i < keys.Count; i++) str += keys[i] + (i != keys.Count - 1 ? ", " : "");
		return str;
	}

	public void LoadSettings() {
		Language          = PlayerPrefs.GetString("Language", "");
		Music             = PlayerPrefs.GetFloat ("Music", 1f);
		SoundFX           = PlayerPrefs.GetFloat ("SoundFX", 1f);
		MouseSensitivity  = PlayerPrefs.GetFloat ("MouseSensitivity", 1f);

		defaultMoveUp    ??= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveUp));
		defaultMoveLeft  ??= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveLeft));
		defaultMoveDown  ??= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveDown));
		defaultMoveRight ??= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveRight));
		defaultInteract  ??= ToString(InputManager.I.GetKeysBinding(KeyAction.Interact));
		defaultCancel    ??= ToString(InputManager.I.GetKeysBinding(KeyAction.Cancel));

		string strMoveUp    = PlayerPrefs.GetString("MoveUp",    defaultMoveUp);
		string strMoveLeft  = PlayerPrefs.GetString("MoveLeft",  defaultMoveLeft);
		string strMoveDown  = PlayerPrefs.GetString("MoveDown",  defaultMoveDown);
		string strMoveRight = PlayerPrefs.GetString("MoveRight", defaultMoveRight);
		string strInteract  = PlayerPrefs.GetString("Interact",  defaultInteract);
		string strCancel    = PlayerPrefs.GetString("Cancel",    defaultCancel);

		InputManager.I.SetKeysBinding(KeyAction.MoveUp,    ToKeys(strMoveUp));
		InputManager.I.SetKeysBinding(KeyAction.MoveLeft,  ToKeys(strMoveLeft));
		InputManager.I.SetKeysBinding(KeyAction.MoveDown,  ToKeys(strMoveDown));
		InputManager.I.SetKeysBinding(KeyAction.MoveRight, ToKeys(strMoveRight));
		InputManager.I.SetKeysBinding(KeyAction.Interact,  ToKeys(strInteract));
		InputManager.I.SetKeysBinding(KeyAction.Cancel,    ToKeys(strCancel));
		
		UpdateLanguage();
		UpdateFullScreen();
		UpdateScreenResolution();
		UpdateMusic();
		UpdateSoundFX();

		UpdateMouseSensitivity();
		UpdateMoveUp();
		UpdateMoveLeft();
		UpdateMoveDown();
		UpdateMoveRight();
		UpdateInteract();
		UpdateCancel();
	}

	public void SaveSettings() {
		PlayerPrefs.SetString("Language",         Language);
		PlayerPrefs.SetFloat ("Music",            Music);
		PlayerPrefs.SetFloat ("SoundFX",          SoundFX);
		PlayerPrefs.SetFloat ("MouseSensitivity", MouseSensitivity);

		string strMoveUp	= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveUp));
		string strMoveLeft	= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveLeft));
		string strMoveDown	= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveDown));
		string strMoveRight	= ToString(InputManager.I.GetKeysBinding(KeyAction.MoveRight));
		string strInteract	= ToString(InputManager.I.GetKeysBinding(KeyAction.Interact));
		string strCancel	= ToString(InputManager.I.GetKeysBinding(KeyAction.Cancel));

		PlayerPrefs.SetString("MoveUp",    strMoveUp);
		PlayerPrefs.SetString("MoveLeft",  strMoveLeft);
		PlayerPrefs.SetString("MoveDown",  strMoveDown);
		PlayerPrefs.SetString("MoveRight", strMoveRight);
		PlayerPrefs.SetString("Interact",  strInteract);
		PlayerPrefs.SetString("Cancel",    strCancel);

		PlayerPrefs.Save();
	}



	// Lifcycle

	void Start() {
		LoadSettings();
		OpenMainMenu();
	}

	Selectable selectable;

	void Update() {
		if (InputManager.I.GetKeyDown(KeyAction.Move)) {
			if (!Selected) Selected = FirstSelected;
		}
		if (InputManager.I.GetKeyDown(KeyAction.Interact)) {
			if (Selected && Selected.TryGetComponent(out selectable)) {
				if (selectable is CustomButton    customButton   ) customButton   .OnSubmit();
				if (selectable is SettingsButton  settingsButton ) settingsButton .OnSubmit();
				if (selectable is SettingsToggle  settingsToggle ) settingsToggle .OnSubmit();
				if (selectable is SettingsStepper settingsStepper) settingsStepper.OnSubmit();
				if (selectable is SettingsSlider  settingsSlider ) settingsSlider .OnSubmit();
			}
		}
		if (InputManager.I.GetKeyDown(KeyAction.Cancel)) Back();
	}

	void LateUpdate() {
		PeekScreenResolution();
		UpdateFade();
	}



	// ------------------------------------------------------------------------------------------------
	// Main Menu Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenMainMenu() {
		// Fade Out
		Open(CanvasType.MainMenu);
		// Fade In
	}



	// ------------------------------------------------------------------------------------------------
	// Game Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenGame() {
		// Fade Out
		Open(CanvasType.Game);
		// Fade In
	}



	// ------------------------------------------------------------------------------------------------
	// Dialogue Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenDialogue() {
		Open(CanvasType.Dialogue);
	}



	// ------------------------------------------------------------------------------------------------
	// Menu Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenMenu() {
		Open(CanvasType.Menu);
	}



	// ------------------------------------------------------------------------------------------------
	// Settings Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenSettings() {
		Open(CanvasType.Settings);
	}



	SettingsStepper language;

	public void UpdateLanguage(SettingsStepper stepper = null) {
		if (stepper) language = stepper;
		if (string.IsNullOrEmpty(Language)) Language = Application.systemLanguage.ToString();
		int index = Mathf.Max(0, LocalizationSettings.AvailableLocales.Locales.FindIndex(locale => {
			return locale.Identifier.CultureInfo.NativeName.Equals(Language);
		}));
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
		if (language) language.Text = Language;
	}

	public void SetLanguage(int value) {
		int count = LocalizationSettings.AvailableLocales.Locales.Count;
		int index = Mathf.Max(0, LocalizationSettings.AvailableLocales.Locales.FindIndex(locale => {
			return locale.Identifier.CultureInfo.NativeName.Equals(Language);
		}));
		index = (int)Mathf.Repeat(index + value, count);
		Locale locale = LocalizationSettings.AvailableLocales.Locales[index];
		Language = locale.Identifier.CultureInfo.NativeName;
	}



	int        fullScreen;
	Vector2Int windowedResolutionSize;

	public void UpdateFullScreen(SettingsToggle toggle = null) {
		if (fullScreen == default) fullScreen = Screen.fullScreen ? 4 : 3;
		if (toggle) switch (fullScreen) {
			case  4: toggle.Value = true;  break;
			case  3: toggle.Value = false; break;
			default:
				Vector2Int resolution = windowedResolutionSize;
				if (toggle.Value) {
					resolution.x = Screen.currentResolution.width;
					resolution.y = Screen.currentResolution.height;
					windowedResolutionSize = new Vector2Int(Screen.width, Screen.height);
				}
				else resolution = windowedResolutionSize;
				Screen.SetResolution(resolution.x, resolution.y, toggle.Value);
				break;
		}
	}

	public void SetFullScreen(bool value) {
		fullScreen = value ? 2 : 1;
		Screen.fullScreen = value;
	}



	SettingsStepper screenResolution;

	public void UpdateScreenResolution(SettingsStepper stepper = null) {
		if (stepper) screenResolution = stepper;

		int multiplier = Mathf.Max(1, Mathf.Min(
			Screen.width  / ReferenceResolution.x,
			Screen.height / ReferenceResolution.y));

		int screenIndex = Array.FindIndex(m_ResolutionPresets, preset =>
			preset.x == Screen.width &&
			preset.y == Screen.height);
		int screenIndexFloor = Array.FindLastIndex(m_ResolutionPresets, preset =>
			preset.x <= Screen.width &&
			preset.y <= Screen.height);
		int screenIndexMax = Array.FindLastIndex(m_ResolutionPresets, preset =>
			preset.x < Screen.currentResolution.width &&
			preset.y < Screen.currentResolution.height);
		
		if (CanvasScaler) CanvasScaler.scaleFactor = multiplier;
		if (CameraManager.I) {
			Vector2Int size = new Vector2Int(Screen.width, Screen.height);
			if (PixelPerfect) {
				size.x = (int)Mathf.Ceil(Screen.width  / multiplier);
				size.y = (int)Mathf.Ceil(Screen.height / multiplier);
			}
			if (size != Vector2Int.zero) {
				CameraManager.I.RenderTextureSize = size;
				CameraManager.I. OrthographicSize = size.y / 2 / PixelPerUnit;
			}
		}
		if (screenResolution) {
			string text       = $"{Screen.width} x {Screen.height}";
			bool interactable = !Screen.fullScreen;
			bool activatePrev = !Screen.fullScreen && screenIndex != 0 && screenIndexFloor != -1;
			bool activateNext = !Screen.fullScreen && screenIndexFloor < screenIndexMax;

			screenResolution.Text         = text;
			screenResolution.interactable = interactable;
			screenResolution.ActivatePrev = activatePrev;
			screenResolution.ActivateNext = activateNext;
		}
	}

	public void SetScreenResolution(int value) {
		int screenIndex = Array.FindIndex(m_ResolutionPresets, preset =>
			preset.x == Screen.width &&
			preset.y == Screen.height);
		int screenIndexFloor = Array.FindLastIndex(m_ResolutionPresets, preset =>
			preset.x <= Screen.width &&
			preset.y <= Screen.height);
		int screenIndexMax = Array.FindLastIndex(m_ResolutionPresets, preset =>
			preset.x < Screen.currentResolution.width &&
			preset.y < Screen.currentResolution.height);
		
		if (value == -1 && screenIndex == -1) value = 0;
		int index = Mathf.Clamp(screenIndexFloor + value, 0, screenIndexMax);
		Vector2Int resolution = m_ResolutionPresets[index];
		Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
	}

	Vector2Int screenResolutionSize;

	void PeekScreenResolution() {
		if (screenResolutionSize.x != Screen.width || screenResolutionSize.y != Screen.height) {
			screenResolutionSize = new Vector2Int(Screen.width, Screen.height);
			UpdateScreenResolution(screenResolution);
		}
	}



	SettingsSlider music;

	public void UpdateMusic(SettingsSlider slider = null) {
		if (slider) music = slider;
		if (music) music.Value = m_Music;
	}

	public void SetMusic(float value) {
		m_Music = value;
	}

	SettingsSlider soundFX;

	public void UpdateSoundFX(SettingsSlider slider = null) {
		if (slider) soundFX = slider;
		if (soundFX) soundFX.Value = m_SoundFX;
	}

	public void SetSoundFX(float value) {
		m_SoundFX = value;
	}

	SettingsSlider mouseSensitivity;

	public void UpdateMouseSensitivity(SettingsSlider slider = null) {
		if (slider) mouseSensitivity = slider;
		if (mouseSensitivity) mouseSensitivity.Value = MouseSensitivity;
	}

	public void SetMouseSensitivity(float value) {
		MouseSensitivity = value;
	}



	readonly SettingsButton[] action = new SettingsButton[Enum.GetValues(typeof(KeyAction)).Length];

	public void UpdateMoveUp   (SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveUp);
	public void UpdateMoveLeft (SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveLeft);
	public void UpdateMoveDown (SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveDown);
	public void UpdateMoveRight(SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveRight);
	public void UpdateInteract (SettingsButton button = null) => UpdateKeys(button, KeyAction.Interact);
	public void UpdateCancel   (SettingsButton button = null) => UpdateKeys(button, KeyAction.Cancel);

	void UpdateKeys(SettingsButton button, KeyAction keyAction) {
		if (button) action[(int)keyAction] = button;
		string str = ToString(InputManager.I.GetKeysBinding(keyAction));
		str = str.Replace("upArrow",    "↑");
		str = str.Replace("leftArrow",  "←");
		str = str.Replace("downArrow",  "↓");
		str = str.Replace("rightArrow", "→");
		if (action[(int)keyAction]) action[(int)keyAction].Text = str;
	}

	public void SetMoveUp   () => SetKeys(KeyAction.MoveUp);
	public void SetMoveLeft () => SetKeys(KeyAction.MoveLeft);
	public void SetMoveDown () => SetKeys(KeyAction.MoveDown);
	public void SetMoveRight() => SetKeys(KeyAction.MoveRight);
	public void SetInteract () => SetKeys(KeyAction.Interact);
	public void SetCancel   () => SetKeys(KeyAction.Cancel);

	void SetKeys(KeyAction keyAction) {
		StartCoroutine(SetKeysCoroutine(keyAction));
	}
	IEnumerator SetKeysCoroutine(KeyAction keyAction) {
		OpenConfirmation("Binding", "Binding Message", "Apply Binding", "Cancel Binding");
		List<string> keys = new();
		if (confirmationPositive) confirmationPositive.OnClick.AddListener(() => {
			InputManager.I.SetKeysBinding(keyAction, keys);
		});
		InputManager.I.RecordKeys();
		while ((ActiveCanvas & CanvasType.Confirmation) != 0) {
			yield return null;
			switch (InputManager.I.RecordedKey) {
				case "":
					break;
				case "enter":
					if (!Selected || !Selected.TryGetComponent(out selectable)) {
						if (confirmationPositive) confirmationPositive.OnClick.Invoke();
					}
					break;
				case "escape":
					break;
				default:
					if (!keys.Exists(key => key.Equals(InputManager.I.RecordedKey))) {
						keys.Add(InputManager.I.RecordedKey);
						string str = ToString(keys);
						str = str.Replace("upArrow",    "↑");
						str = str.Replace("leftArrow",  "←");
						str = str.Replace("downArrow",  "↓");
						str = str.Replace("rightArrow", "→");
						if (confirmationMessage) confirmationMessage.Text = str;
					}
					break;
			}
		}
		InputManager.I.StopRecordKeys();
		if (action[(int)keyAction]) action[(int)keyAction].Refresh();
	}



	public void SetDeleteAllData() {
		OpenConfirmation("Delete All Data", "Delete All Data Message", "Delete", "Cancel");
		if (confirmationPositive) confirmationPositive.OnClick.AddListener(() => {
			PlayerPrefs.DeleteAll();
			LoadSettings();
			OpenMainMenu();
		});
	}



	// ------------------------------------------------------------------------------------------------
	// Dialog Canvas
	// ------------------------------------------------------------------------------------------------

	CustomText   confirmationTitle;
	CustomText   confirmationMessage;
	CustomButton confirmationPositive;
	CustomButton confirmationNegative;

	public void UpdateConfirmationTitle   (CustomText   text  ) => confirmationTitle    = text;
	public void UpdateConfirmationMessage (CustomText   text  ) => confirmationMessage  = text;
	public void UpdateConfirmationPositive(CustomButton button) => confirmationPositive = button;
	public void UpdateConfirmationNegative(CustomButton button) => confirmationNegative = button;

	public void OpenConfirmation(string arg0, string arg1, string arg2, string arg3) {
		Open(CanvasType.Confirmation);
		if (confirmationTitle   ) confirmationTitle  .SetLocalizeText("UI Table", arg0);
		if (confirmationMessage ) confirmationMessage.SetLocalizeText("UI Table", arg1);
		if (confirmationPositive) {
			confirmationPositive.SetLocalizeText("UI Table", arg2);
			confirmationPositive.OnClick.RemoveAllListeners();
			confirmationPositive.OnClick.AddListener(() => Back());
		}
		if (confirmationNegative) {
			confirmationNegative.SetLocalizeText("UI Table", arg3);
			confirmationNegative.OnClick.RemoveAllListeners();
			confirmationNegative.OnClick.AddListener(() => Back());
		}
	}



	// ------------------------------------------------------------------------------------------------
	// Fade Canvas
	// ------------------------------------------------------------------------------------------------

	int   fadeState;
	Image fadeImage;

	public void FadeOut() {
		if (fadeImage || m_FadeCanvas.TryGetComponent(out fadeImage)) {
			ActiveCanvas |= CanvasType.Fade;
			fadeState = 1;
		}
	}

	public void FadeIn(bool force = false) {
		if (fadeImage || m_FadeCanvas.TryGetComponent(out fadeImage)) {
			ActiveCanvas |= CanvasType.Fade;
			fadeState = 2;
			if (force) {
				Color color = fadeImage.color;
				color.a = 1.0f;
				fadeImage.color = color;
			}
		}
	}

	void UpdateFade() {
		if (fadeState != 0) {
			Color color = fadeImage.color;
			color.a = Mathf.MoveTowards(color.a, fadeState == 1 ? 1.0f : 0.0f, Time.fixedDeltaTime);
			fadeImage.color = color;
			if (color.a == 0.0f) {
				ActiveCanvas &= ~CanvasType.Fade;
				fadeState = 0;
			}
		}
	}
}

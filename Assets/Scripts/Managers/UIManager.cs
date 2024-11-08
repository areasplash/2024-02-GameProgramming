using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using System.Collections;
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
		SerializedProperty m_MenuCanvas;
		SerializedProperty m_SettingsCanvas;
		SerializedProperty m_DialogCanvas;
		SerializedProperty m_FadeCanvas;

		SerializedProperty m_MainMenuFirstSelected;
		SerializedProperty m_GameFirstSelected;
		SerializedProperty m_MenuFirstSelected;
		SerializedProperty m_SettingsFirstSelected;
		SerializedProperty m_DialogFirstSelected;
		SerializedProperty m_FadeFirstSelected;

		SerializedProperty m_ResolutionPresets;

		UIManager I => target as UIManager;

		void OnEnable() {
			m_MainMenuCanvas = serializedObject.FindProperty("m_MainMenuCanvas");
			m_GameCanvas     = serializedObject.FindProperty("m_GameCanvas");
			m_MenuCanvas     = serializedObject.FindProperty("m_MenuCanvas");
			m_SettingsCanvas = serializedObject.FindProperty("m_SettingsCanvas");
			m_DialogCanvas   = serializedObject.FindProperty("m_DialogCanvas");
			m_FadeCanvas     = serializedObject.FindProperty("m_FadeCanvas");

			m_MainMenuFirstSelected = serializedObject.FindProperty("m_MainMenuFirstSelected");
			m_GameFirstSelected     = serializedObject.FindProperty("m_GameFirstSelected");
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
			PropertyField(m_MenuCanvas);
			PropertyField(m_SettingsCanvas);
			PropertyField(m_DialogCanvas);
			PropertyField(m_FadeCanvas);
			Space();
			LabelField("First Selected", EditorStyles.boldLabel);
			PropertyField(m_MainMenuFirstSelected);
			PropertyField(m_GameFirstSelected);
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
// Canvas Type
// ====================================================================================================

[Serializable, Flags] public enum CanvasType {
	None	 = 0,
	MainMenu = 1 << 0,
	Game     = 1 << 1,
	Menu     = 1 << 2,
	Settings = 1 << 3,
	Dialog   = 1 << 4,
	Fade     = 1 << 5,
}



// ====================================================================================================
// UI Manager
// ====================================================================================================

public class UIManager : MonoSingleton<UIManager> {

	// Fields

	[SerializeField] Canvas m_MainMenuCanvas;
	[SerializeField] Canvas m_GameCanvas;
	[SerializeField] Canvas m_MenuCanvas;
	[SerializeField] Canvas m_SettingsCanvas;
	[SerializeField] Canvas m_DialogCanvas;
	[SerializeField] Canvas m_FadeCanvas;

	[SerializeField] GameObject m_MainMenuFirstSelected;
	[SerializeField] GameObject m_GameFirstSelected;
	[SerializeField] GameObject m_MenuFirstSelected;
	[SerializeField] GameObject m_SettingsFirstSelected;
	[SerializeField] GameObject m_DialogFirstSelected;
	[SerializeField] GameObject m_FadeFirstSelected;

	[SerializeField] GameObject m_MainMenuLastSelected;
	[SerializeField] GameObject m_GameLastSelected;
	[SerializeField] GameObject m_MenuLastSelected;
	[SerializeField] GameObject m_SettingsLastSelected;
	[SerializeField] GameObject m_DialogLastSelected;
	[SerializeField] GameObject m_FadeLastSelected;

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

	[SerializeField] string m_Language;
	[SerializeField] float  m_Music;
	[SerializeField] float  m_SoundFX;
	[SerializeField] float  m_MouseSensitivity;



	// Properties

	CanvasType activeCanvas {
		get {
			CanvasType value = CanvasType.None;
			if (m_MainMenuCanvas.enabled) value |= CanvasType.MainMenu;
			if (m_GameCanvas    .enabled) value |= CanvasType.Game;
			if (m_MenuCanvas    .enabled) value |= CanvasType.Menu;
			if (m_SettingsCanvas.enabled) value |= CanvasType.Settings;
			if (m_DialogCanvas  .enabled) value |= CanvasType.Dialog;
			if (m_FadeCanvas    .enabled) value |= CanvasType.Fade;
			return value;
		}
		set {
			if (value == activeCanvas) return;
			m_MainMenuCanvas.enabled = 0 != (value & CanvasType.MainMenu);
			m_GameCanvas    .enabled = 0 != (value & CanvasType.Game);
			m_MenuCanvas    .enabled = 0 != (value & CanvasType.Menu);
			m_SettingsCanvas.enabled = 0 != (value & CanvasType.Settings);
			m_DialogCanvas  .enabled = 0 != (value & CanvasType.Dialog);
			m_FadeCanvas    .enabled = 0 != (value & CanvasType.Fade);
		}
	}

	CanvasType highestCanvas {
		get {
			CanvasType value = CanvasType.None;
			if (m_MainMenuCanvas.enabled) value = CanvasType.MainMenu;
			if (m_GameCanvas    .enabled) value = CanvasType.Game;
			if (m_MenuCanvas    .enabled) value = CanvasType.Menu;
			if (m_SettingsCanvas.enabled) value = CanvasType.Settings;
			if (m_DialogCanvas  .enabled) value = CanvasType.Dialog;
			if (m_FadeCanvas    .enabled) value = CanvasType.Fade;
			return value;
		}
	}

	GameObject selected {
		get => EventSystem.current?.currentSelectedGameObject;
		set => EventSystem.current?.SetSelectedGameObject(value);
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

	public static CanvasType GetActiveCanvas() {
		return Instance ? Instance.activeCanvas : CanvasType.None;
	}

	void SaveLastSelected() {
		switch (highestCanvas) {
			case CanvasType.MainMenu: m_MainMenuLastSelected = selected; break;
			case CanvasType.Game:     m_GameLastSelected     = selected; break;
			case CanvasType.Menu:     m_MenuLastSelected     = selected; break;
			case CanvasType.Settings: m_SettingsLastSelected = selected; break;
			case CanvasType.Dialog:   m_DialogLastSelected   = selected; break;
			case CanvasType.Fade:     m_FadeLastSelected     = selected; break;
		}
	}

	void LoadLastSelected() {
		if (!m_MainMenuLastSelected) m_MainMenuLastSelected = m_MainMenuFirstSelected;
		if (!m_GameLastSelected    ) m_GameLastSelected     = m_GameFirstSelected;
		if (!m_MenuLastSelected    ) m_MenuLastSelected     = m_MenuFirstSelected;
		if (!m_SettingsLastSelected) m_SettingsLastSelected = m_SettingsFirstSelected;
		if (!m_DialogLastSelected  ) m_DialogLastSelected   = m_DialogFirstSelected;
		if (!m_FadeLastSelected    ) m_FadeLastSelected     = m_FadeFirstSelected;
		
		switch (highestCanvas) {
			case CanvasType.MainMenu: selected = m_MainMenuLastSelected; break;
			case CanvasType.Game:     selected = m_GameLastSelected;     break;
			case CanvasType.Menu:     selected = m_MenuLastSelected;     break;
			case CanvasType.Settings: selected = m_SettingsLastSelected; break;
			case CanvasType.Dialog:   selected = m_DialogLastSelected;   break;
			case CanvasType.Fade:     selected = m_FadeLastSelected;     break;
		}
	}

	Stack<CanvasType> stack = new Stack<CanvasType>();

	void Open(CanvasType canvas) {
		SaveLastSelected();
		CanvasType primary = activeCanvas & (CanvasType.MainMenu | CanvasType.Game);
		CanvasType fade    = activeCanvas & CanvasType.Fade;
		switch (canvas) {
			case CanvasType.MainMenu:
				stack.Clear();
				stack.Push(activeCanvas);
				activeCanvas = canvas | fade;
				break;
			
			case CanvasType.Game:
				stack.Clear();
				stack.Push(activeCanvas);
				activeCanvas = canvas | fade;
				break;

			case CanvasType.Menu:
				stack.Push(activeCanvas);
				activeCanvas = primary | canvas | fade;
				break;

			case CanvasType.Settings:
				stack.Push(activeCanvas);
				activeCanvas = primary | canvas | fade;
				break;

			case CanvasType.Dialog:
				stack.Push(activeCanvas);
				activeCanvas = primary | canvas | fade;
				break;
			
			case CanvasType.Fade:
				break;
		}
		LoadLastSelected();
	}

	public static void Back() {
		Instance?.Back_Internal();
	}
	void Back_Internal() {
		SaveLastSelected();
		switch (highestCanvas) {
			case CanvasType.MainMenu:
				OpenDialog("Quit", "Quit Message", "Quit", "Cancel");
				dialogPositive?.onClick.RemoveAllListeners();
				dialogPositive?.onClick.AddListener(() => Quit());
				break;
			
			case CanvasType.Game:
				OpenMenu();
				break;

			case CanvasType.Menu:
				activeCanvas = stack.Pop();
				break;

			case CanvasType.Settings:
				SaveSettings();
				activeCanvas = stack.Pop();
				break;

			case CanvasType.Dialog:
				activeCanvas = stack.Pop();
				break;
			
			case CanvasType.Fade:
				break;
		}
		LoadLastSelected();
	}

	public static void Quit() {
		#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
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



	List<string> ToKeys(string str) {
		List<string> keys = new List<string>();
		foreach (string key in str.Split(", ")) if (key != string.Empty) keys.Add(key);
		return keys;
	}

	string ToString(List<string> keys) {
		string str = "";
		for (int i = 0; i < keys.Count; i++) str += keys[i] + (i != keys.Count - 1 ? ", " : "");
		return str;
	}

	string defaultMoveUp;
	string defaultMoveLeft;
	string defaultMoveDown;
	string defaultMoveRight;
	string defaultInteract;
	string defaultCancel;

	public static void LoadSettings() {
		Instance?.LoadSettings_Internal();
	}
	void LoadSettings_Internal() {
		m_Language          = PlayerPrefs.GetString("Language", "");
		m_Music             = PlayerPrefs.GetFloat ("Music", 1f);
		m_SoundFX           = PlayerPrefs.GetFloat ("SoundFX", 1f);
		m_MouseSensitivity  = PlayerPrefs.GetFloat ("MouseSensitivity", 1f);

		defaultMoveUp    ??= ToString(InputManager.GetKeysBinding(KeyAction.MoveUp));
		defaultMoveLeft  ??= ToString(InputManager.GetKeysBinding(KeyAction.MoveLeft));
		defaultMoveDown  ??= ToString(InputManager.GetKeysBinding(KeyAction.MoveDown));
		defaultMoveRight ??= ToString(InputManager.GetKeysBinding(KeyAction.MoveRight));
		defaultInteract  ??= ToString(InputManager.GetKeysBinding(KeyAction.Interact));
		defaultCancel    ??= ToString(InputManager.GetKeysBinding(KeyAction.Cancel));

		string strMoveUp    = PlayerPrefs.GetString("MoveUp",    defaultMoveUp);
		string strMoveLeft  = PlayerPrefs.GetString("MoveLeft",  defaultMoveLeft);
		string strMoveDown  = PlayerPrefs.GetString("MoveDown",  defaultMoveDown);
		string strMoveRight = PlayerPrefs.GetString("MoveRight", defaultMoveRight);
		string strInteract  = PlayerPrefs.GetString("Interact",  defaultInteract);
		string strCancel    = PlayerPrefs.GetString("Cancel",    defaultCancel);

		InputManager.SetKeysBinding(KeyAction.MoveUp,    ToKeys(strMoveUp));
		InputManager.SetKeysBinding(KeyAction.MoveLeft,  ToKeys(strMoveLeft));
		InputManager.SetKeysBinding(KeyAction.MoveDown,  ToKeys(strMoveDown));
		InputManager.SetKeysBinding(KeyAction.MoveRight, ToKeys(strMoveRight));
		InputManager.SetKeysBinding(KeyAction.Interact,  ToKeys(strInteract));
		InputManager.SetKeysBinding(KeyAction.Cancel,    ToKeys(strCancel));
		
		UpdateLanguage(null);
		UpdateFullScreen(null);
		UpdateScreenResolution(null);
	}

	public static void SaveSettings() {
		Instance?.SaveSettings_Internal();
	}
	void SaveSettings_Internal() {
		PlayerPrefs.SetString("Language",         m_Language);
		PlayerPrefs.SetFloat ("Music",            m_Music);
		PlayerPrefs.SetFloat ("SoundFX",          m_SoundFX);
		PlayerPrefs.SetFloat ("MouseSensitivity", m_MouseSensitivity);

		string strMoveUp	= ToString(InputManager.GetKeysBinding(KeyAction.MoveUp));
		string strMoveLeft	= ToString(InputManager.GetKeysBinding(KeyAction.MoveLeft));
		string strMoveDown	= ToString(InputManager.GetKeysBinding(KeyAction.MoveDown));
		string strMoveRight	= ToString(InputManager.GetKeysBinding(KeyAction.MoveRight));
		string strInteract	= ToString(InputManager.GetKeysBinding(KeyAction.Interact));
		string strCancel	= ToString(InputManager.GetKeysBinding(KeyAction.Cancel));

		PlayerPrefs.SetString("MoveUp",    strMoveUp);
		PlayerPrefs.SetString("MoveLeft",  strMoveLeft);
		PlayerPrefs.SetString("MoveDown",  strMoveDown);
		PlayerPrefs.SetString("MoveRight", strMoveRight);
		PlayerPrefs.SetString("Interact",  strInteract);
		PlayerPrefs.SetString("Cancel",    strCancel);

		PlayerPrefs.Save();
	}



	// Cycle

	void Start() {
		LoadSettings();
		OpenMainMenu();
		// Fade In
	}

	void Update() {
		if (InputManager.GetKeyDown(KeyAction.Move)) {
			if (!selected) switch (highestCanvas) {
				case CanvasType.MainMenu: selected = m_MainMenuFirstSelected; break;
				case CanvasType.Game:     selected = m_GameFirstSelected;     break;
				case CanvasType.Menu:     selected = m_MenuFirstSelected;     break;
				case CanvasType.Settings: selected = m_SettingsFirstSelected; break;
				case CanvasType.Dialog:   selected = m_DialogFirstSelected;   break;
				case CanvasType.Fade:     selected = m_FadeFirstSelected;     break;
			}
		}
		if (InputManager.GetKeyDown(KeyAction.Interact)) {
			if (selected && selected.TryGetComponent(out Selectable selectable)) {
				if (selectable is CustomButton    customButton   ) customButton   .OnSubmit();
				if (selectable is SettingsButton  settingsButton ) settingsButton .OnSubmit();
				if (selectable is SettingsToggle  settingsToggle ) settingsToggle .OnSubmit();
				if (selectable is SettingsStepper settingsStepper) settingsStepper.OnSubmit();
				if (selectable is SettingsSlider  settingsSlider ) settingsSlider .OnSubmit();
			}
		}
		if (InputManager.GetKeyDown(KeyAction.Cancel)) Back();
	}



	// ------------------------------------------------------------------------------------------------
	// Main Menu Canvas
	// ------------------------------------------------------------------------------------------------

	public static void OpenMainMenu() {
		Instance?.OpenMainMenu_Internal();
	}
	void OpenMainMenu_Internal() {
		// Fade Out
		Open(CanvasType.MainMenu);
		// Fade In
	}



	// ------------------------------------------------------------------------------------------------
	// Game Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenGame() {
		Instance?.OpenGame_Internal();
	}
	void OpenGame_Internal() {
		// Fade Out
		Open(CanvasType.Game);
		// Fade In
	}



	// ------------------------------------------------------------------------------------------------
	// Menu Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenMenu() {
		Instance?.OpenMenu_Internal();
	}
	void OpenMenu_Internal() {
		Open(CanvasType.Menu);
	}



	// ------------------------------------------------------------------------------------------------
	// Settings Canvas
	// ------------------------------------------------------------------------------------------------

	public void OpenSettings() {
		Instance?.OpenSettings_Internal();
	}
	void OpenSettings_Internal() {
		Open(CanvasType.Settings);
	}



	public void UpdateLanguage(SettingsStepper stepper) {
		if (string.IsNullOrEmpty(m_Language)) m_Language = Application.systemLanguage.ToString();
		int index = Mathf.Max(0, LocalizationSettings.AvailableLocales.Locales.FindIndex(locale => {
			return locale.Identifier.CultureInfo.NativeName.Equals(m_Language);
		}));
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
		if (stepper) stepper.text = m_Language;
	}

	public void SetLanguage(int value) {
		int count = LocalizationSettings.AvailableLocales.Locales.Count;
		int index = Mathf.Max(0, LocalizationSettings.AvailableLocales.Locales.FindIndex(locale => {
			return locale.Identifier.CultureInfo.NativeName.Equals(m_Language);
		}));
		index = (int)Mathf.Repeat(index + value, count);
		Locale locale = LocalizationSettings.AvailableLocales.Locales[index];
		m_Language = locale.Identifier.CultureInfo.NativeName;
	}



	int fullScreenCache = -1;
	Vector2Int windowedResolution;

	public void UpdateFullScreen(SettingsToggle toggle) {
		if (fullScreenCache == -1) fullScreenCache = Screen.fullScreen ? 3 : 2;
		if (toggle) switch (fullScreenCache) {
			case  3: toggle.value = true;  break;
			case  2: toggle.value = false; break;
			default:
				Vector2Int resolution = windowedResolution;
				if (toggle.value) {
					resolution.x = Screen.currentResolution.width;
					resolution.y = Screen.currentResolution.height;
					windowedResolution = new Vector2Int(Screen.width, Screen.height);
				}
				else resolution = windowedResolution;
				Screen.SetResolution(resolution.x, resolution.y, toggle.value);
				break;
		}
	}

	public void SetFullScreen(bool value) {
		fullScreenCache = value ? 1 : 0;
		Screen.fullScreen = value;
	}



	SettingsStepper screenResolution;
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

	public void UpdateScreenResolution(SettingsStepper stepper) {
		screenResolution = stepper;

		bool interactable = !Screen.fullScreen;
		bool activatePrev = !Screen.fullScreen && screenIndex != 0 && screenIndexFloor != -1;
		bool activateNext = !Screen.fullScreen && screenIndexFloor < screenIndexMax;
		if (screenResolution) {
			screenResolution.text = $"{Screen.width} x {Screen.height}";
			screenResolution.interactable = interactable;
			screenResolution.activatePrev = activatePrev;
			screenResolution.activateNext = activateNext;
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
			CameraManager.RenderTextureSize = size;
			CameraManager. OrthographicSize = size.y / 2 / pixelPerUnit;
		}
	}

	public void SetScreenResolution(int value) {
		if (value == -1 && screenIndex == -1) value = 0;
		int index = Mathf.Clamp(screenIndexFloor + value, 0, screenIndexMax);
		Vector2Int resolution = m_ResolutionPresets[index];
		Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
	}

	Vector2Int screenResolutionCache;

	void LateUpdate() {
		if (screenResolutionCache.x != Screen.width || screenResolutionCache.y != Screen.height) {
			screenResolutionCache = new Vector2Int(Screen.width, Screen.height);
			UpdateScreenResolution(screenResolution);
		}
	}



	public void UpdateMusic  (SettingsSlider slider) => slider.value = m_Music;
	public void UpdateSoundFX(SettingsSlider slider) => slider.value = m_SoundFX;

	public void SetMusic  (float value) => m_Music   = value;
	public void SetSoundFX(float value) => m_SoundFX = value;

	public void UpdateMouseSensitivity(SettingsSlider slider) => slider.value = m_MouseSensitivity;
	public void SetMouseSensitivity(float value) => m_MouseSensitivity = value;

	public void UpdateMoveUp   (SettingsButton button) => UpdateKeys(button, KeyAction.MoveUp);
	public void UpdateMoveLeft (SettingsButton button) => UpdateKeys(button, KeyAction.MoveLeft);
	public void UpdateMoveDown (SettingsButton button) => UpdateKeys(button, KeyAction.MoveDown);
	public void UpdateMoveRight(SettingsButton button) => UpdateKeys(button, KeyAction.MoveRight);
	public void UpdateInteract (SettingsButton button) => UpdateKeys(button, KeyAction.Interact);
	public void UpdateCancel   (SettingsButton button) => UpdateKeys(button, KeyAction.Cancel);

	SettingsButton[] action = new SettingsButton[Enum.GetValues(typeof(KeyAction)).Length];

	void UpdateKeys(SettingsButton button, KeyAction keyAction) {
		action[(int)keyAction] = button;
		string str = ToString(InputManager.GetKeysBinding(keyAction));
		str = str.Replace("upArrow",    "↑");
		str = str.Replace("leftArrow",  "←");
		str = str.Replace("downArrow",  "↓");
		str = str.Replace("rightArrow", "→");
		if (button) button.text = str;
	}

	public void SetMoveUp   () => SetKeys(KeyAction.MoveUp);
	public void SetMoveLeft () => SetKeys(KeyAction.MoveLeft);
	public void SetMoveDown () => SetKeys(KeyAction.MoveDown);
	public void SetMoveRight() => SetKeys(KeyAction.MoveRight);
	public void SetInteract () => SetKeys(KeyAction.Interact);
	public void SetCancel   () => SetKeys(KeyAction.Cancel);

	void SetKeys(KeyAction keyAction) => StartCoroutine(SetKeysCoroutine(keyAction));
	IEnumerator SetKeysCoroutine(KeyAction keyAction) {
		OpenDialog("Binding", "Binding Message", "Apply Binding", "Cancel Binding");
		List<string> keys = new List<string>();
		dialogPositive?.onClick.AddListener(() => InputManager.SetKeysBinding(keyAction, keys));

		InputManager.RecordKeys();
		while (highestCanvas == CanvasType.Dialog) {
			yield return null;
			bool flag = true;
			flag &= !string.IsNullOrEmpty(InputManager.RecordedKey);
			flag &= !"escape".Equals(InputManager.RecordedKey);
			flag &= !keys.Exists(key => key.Equals(InputManager.RecordedKey));
			if ("enter".Equals(InputManager.RecordedKey)) {
				if (!selected || !selected.TryGetComponent(out Selectable selectable)) {
					dialogPositive?.onClick.Invoke();
				}
			}
			else if (flag) {
				keys.Add(InputManager.RecordedKey);
				string str = ToString(keys);
				str = str.Replace("upArrow",    "↑");
				str = str.Replace("leftArrow",  "←");
				str = str.Replace("downArrow",  "↓");
				str = str.Replace("rightArrow", "→");
				if (dialogMessage) dialogMessage.text = str;
			}
		}
		InputManager.StopRecordKeys();
		action[(int)keyAction]?.Refresh();
	}



	public void SetDeleteAllData() {
		OpenDialog("Delete All Data", "Delete All Data Message", "Delete", "Cancel");
		dialogPositive?.onClick.AddListener(() => {
			PlayerPrefs.DeleteAll();
			LoadSettings();
			Back();
		});
	}



	// ------------------------------------------------------------------------------------------------
	// Dialog Canvas
	// ------------------------------------------------------------------------------------------------

	CustomText   dialogTitle;
	CustomText   dialogMessage;
	CustomButton dialogPositive;
	CustomButton dialogNegative;

	public void UpdateDialogTitle   (CustomText   text  ) => dialogTitle    = text;
	public void UpdateDialogMessage (CustomText   text  ) => dialogMessage  = text;
	public void UpdateDialogPositive(CustomButton button) => dialogPositive = button;
	public void UpdateDialogNegative(CustomButton button) => dialogNegative = button;

	public static void OpenDialog(string arg0, string arg1, string arg2, string arg3) {
		Instance?.OpenDialog_Internal(arg0, arg1, arg2, arg3);
	}
	void OpenDialog_Internal(string arg0, string arg1, string arg2, string arg3) {
		Open(CanvasType.Dialog);
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
	// Fade Canvas
	// ------------------------------------------------------------------------------------------------

	int   fadeState = 0;
	Image fadeImage;

	public static void FadeOut() {
		Instance?.FadeOut_Internal();
	}
	void FadeOut_Internal() {
		if (fadeImage || m_FadeCanvas.TryGetComponent(out fadeImage)) {
			activeCanvas |= CanvasType.Fade;
			fadeState = 1;
		}
	}

	public static void FadeIn() {
		Instance?.FadeIn_Internal();
	}
	void FadeIn_Internal() {
		if (fadeImage || m_FadeCanvas.TryGetComponent(out fadeImage)) {
			activeCanvas |= CanvasType.Fade;
			fadeState = 2;
		}
	}

	void FixedUpdate() {
		if (fadeState != 0) {
			Color color = fadeImage.color;
			color.a = Mathf.MoveTowards(color.a, fadeState == 1 ? 1.0f : 0.0f, Time.fixedDeltaTime);
			fadeImage.color = color;
			if (color.a == 0.0f) {
				activeCanvas &= ~CanvasType.Fade;
				fadeState = 0;
			}
		}
	}
}

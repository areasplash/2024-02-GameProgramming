using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif



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
	
	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] Canvas m_MainMenu;
	[SerializeField] Canvas m_Game;
	[SerializeField] Canvas m_Dialogue;
	[SerializeField] Canvas m_Menu;
	[SerializeField] Canvas m_Settings;
	[SerializeField] Canvas m_Confirmation;
	[SerializeField] Canvas m_Fade;

	[SerializeField] GameObject m_MainMenuFS;
	[SerializeField] GameObject m_GameFS;
	[SerializeField] GameObject m_DialogueFS;
	[SerializeField] GameObject m_MenuFS;
	[SerializeField] GameObject m_SettingsFS;
	[SerializeField] GameObject m_ConfirmationFS;
	[SerializeField] GameObject m_FadeFS;

	[SerializeField] float        m_PixelPerUnit        = 16f;
	[SerializeField] Vector2Int   m_ReferenceResolution = new(640, 360);
	[SerializeField] Vector2Int[] m_ResolutionPresets   = new Vector2Int[] {
		new Vector2Int( 640,  360),
		new Vector2Int(1280,  720),
		new Vector2Int(1920, 1080),
		new Vector2Int(2560, 1440),
		new Vector2Int(3840, 2160),
	};

	[SerializeField] string m_Language         = "";
	[SerializeField] bool   m_PixelPerfect     = false;
	[SerializeField] float  m_Music            = 1f;
	[SerializeField] float  m_SoundFX          = 1f;
	[SerializeField] float  m_MouseSensitivity = 1f;



	static Canvas MainMenu {
		get   =>  Instance? Instance.m_MainMenu : default;
		set { if (Instance) Instance.m_MainMenu = value; }
	}
	static Canvas Game {
		get   =>  Instance? Instance.m_Game : default;
		set { if (Instance) Instance.m_Game = value; }
	}
	static Canvas Dialogue {
		get   =>  Instance? Instance.m_Dialogue : default;
		set { if (Instance) Instance.m_Dialogue = value; }
	}
	static Canvas Menu {
		get   =>  Instance? Instance.m_Menu : default;
		set { if (Instance) Instance.m_Menu = value; }
	}
	static Canvas Settings {
		get   =>  Instance? Instance.m_Settings : default;
		set { if (Instance) Instance.m_Settings = value; }
	}
	static Canvas Confirmation {
		get   =>  Instance? Instance.m_Confirmation : default;
		set { if (Instance) Instance.m_Confirmation = value; }
	}
	static Canvas Fade {
		get   =>  Instance? Instance.m_Fade : default;
		set { if (Instance) Instance.m_Fade = value; }
	}

	public static CanvasType ActiveCanvas {
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

	static CanvasType HighestCanvas {
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



	static GameObject MainMenuLS { get; set; }
	static GameObject MainMenuFS {
		get   =>  Instance? Instance.m_MainMenuFS : default;
		set { if (Instance) Instance.m_MainMenuFS = value; }
	}
	static GameObject GameLS { get; set; }
	static GameObject GameFS {
		get   =>  Instance? Instance.m_GameFS : default;
		set { if (Instance) Instance.m_GameFS = value; }
	}
	static GameObject DialogueLS { get; set; }
	static GameObject DialogueFS {
		get   =>  Instance? Instance.m_DialogueFS : default;
		set { if (Instance) Instance.m_DialogueFS = value; }
	}
	static GameObject MenuLS { get; set; }
	static GameObject MenuFS {
		get   =>  Instance? Instance.m_MenuFS : default;
		set { if (Instance) Instance.m_MenuFS = value; }
	}
	static GameObject SettingsLS { get; set; }
	static GameObject SettingsFS {
		get   =>  Instance? Instance.m_SettingsFS : default;
		set { if (Instance) Instance.m_SettingsFS = value; }
	}
	static GameObject ConfirmationLS { get; set; }
	static GameObject ConfirmationFS {
		get   =>  Instance? Instance.m_ConfirmationFS : default;
		set { if (Instance) Instance.m_ConfirmationFS = value; }
	}
	static GameObject FadeLS { get; set; }
	static GameObject FadeFS {
		get   =>  Instance? Instance.m_FadeFS : default;
		set { if (Instance) Instance.m_FadeFS = value; }
	}
	
	static GameObject LS {
		get => HighestCanvas switch {
			CanvasType.MainMenu     => MainMenuLS,
			CanvasType.Game         => GameLS,
			CanvasType.Dialogue     => DialogueLS,
			CanvasType.Menu         => MenuLS,
			CanvasType.Settings     => SettingsLS,
			CanvasType.Confirmation => ConfirmationLS,
			CanvasType.Fade         => FadeLS,
			_                       => null,
		};
		set {
			switch (HighestCanvas) {
				case CanvasType.MainMenu:     MainMenuLS     = value; break;
				case CanvasType.Game:         GameLS         = value; break;
				case CanvasType.Dialogue:     DialogueLS     = value; break;
				case CanvasType.Menu:         MenuLS         = value; break;
				case CanvasType.Settings:     SettingsLS     = value; break;
				case CanvasType.Confirmation: ConfirmationLS = value; break;
				case CanvasType.Fade:         FadeLS         = value; break;
			}
		}
	}
	static GameObject FS => HighestCanvas switch {
		CanvasType.MainMenu     => MainMenuFS,
		CanvasType.Game         => GameFS,
		CanvasType.Dialogue     => DialogueFS,
		CanvasType.Menu         => MenuFS,
		CanvasType.Settings     => SettingsFS,
		CanvasType.Confirmation => ConfirmationFS,
		CanvasType.Fade         => FadeFS,
		_                       => null,
	};

	static GameObject Selected {
		get   =>  EventSystem.current? EventSystem.current.currentSelectedGameObject : null;
		set { if (EventSystem.current) EventSystem.current.SetSelectedGameObject(value); }
	}



	static CanvasScaler canvasScaler;
	static CanvasScaler CanvasScaler {
		get {
			if (Instance && !canvasScaler) Instance.TryGetComponent(out canvasScaler);
			return canvasScaler;
		}
	}

	public static float PixelPerUnit {
		get => Instance? Instance.m_PixelPerUnit : default;
		set {
			if (Instance) {
				Instance.m_PixelPerUnit = value;
				if (CanvasScaler) CanvasScaler.referencePixelsPerUnit = value;
			}
		}
	}

	public static Vector2Int ReferenceResolution {
		get => Instance? Instance.m_ReferenceResolution : default;
		set {
			if (Instance) {
				Instance.m_ReferenceResolution = value;
				UpdateScreenResolution(screenResolutionStepper);
			}
		}
	}



	public static string Language {
		get           =>  Instance? Instance.m_Language : default;
		private set { if (Instance) Instance.m_Language = value; }
	}
	public static Vector2Int[] ResolutionPresets {
		get           =>  Instance? Instance.m_ResolutionPresets : default;
		private set { if (Instance) Instance.m_ResolutionPresets = value; }
	}
	public static bool PixelPerfect {
		get           =>  Instance? Instance.m_PixelPerfect : default;
		private set { if (Instance) Instance.m_PixelPerfect = value; }
	}
	public static float Music {
		get           =>  Instance? Instance.m_Music : default;
		private set { if (Instance) Instance.m_Music = value; }
	}
	public static float SoundFX {
		get           =>  Instance? Instance.m_SoundFX : default;
		private set { if (Instance) Instance.m_SoundFX = value; }
	}
	public static float MouseSensitivity {
		get           =>  Instance? Instance.m_MouseSensitivity : default;
		private set { if (Instance) Instance.m_MouseSensitivity = value; }
	}

	

	public static bool IsGameRunning { get; private set; }



	#if UNITY_EDITOR
		[CustomEditor(typeof(UIManager))] class UIManagerEditor : ExtendedEditor {
			public override void OnInspectorGUI() {
				Begin("UI Manager");

				LabelField("Canvas", EditorStyles.boldLabel);
				MainMenu     = ObjectField("Main Menu Canvas",    MainMenu);
				Game         = ObjectField("Game Canvas",         Game);
				Dialogue     = ObjectField("Dialogue Canvas",     Dialogue);
				Menu         = ObjectField("Menu Canvas",         Menu);
				Settings     = ObjectField("Settings Canvas",     Settings);
				Confirmation = ObjectField("Confirmation Canvas", Confirmation);
				Fade         = ObjectField("Fade Canvas",         Fade);
				Space();

				LabelField("First Selected", EditorStyles.boldLabel);
				MainMenuFS     = ObjectField("Main Menu First Selected",    MainMenuFS);
				GameFS         = ObjectField("Game First Selected",         GameFS);
				DialogueFS     = ObjectField("Dialogue First Selected",     DialogueFS);
				MenuFS         = ObjectField("Menu First Selected",         MenuFS);
				SettingsFS     = ObjectField("Settings First Selected",     SettingsFS);
				ConfirmationFS = ObjectField("Confirmation First Selected", ConfirmationFS);
				FadeFS         = ObjectField("Fade First Selected",         FadeFS);
				Space();

				LabelField("UI Properties", EditorStyles.boldLabel);
				PixelPerUnit        = FloatField     ("Pixel Per Unit",       PixelPerUnit);
				ReferenceResolution = Vector2IntField("Reference Resolution", ReferenceResolution);
				PropertyField("m_ResolutionPresets");
				Space();

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	static Stack<CanvasType> stack = new Stack<CanvasType>();

	static void SaveSelected() => LS = FS ? Selected : null;
	static void LoadSelected() => Selected = LS ? LS : FS;

	static void Open(CanvasType canvas) {
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

	public static void Back() {
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

	public static void Quit() {
		#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}



	static string defaultMoveUp;
	static string defaultMoveLeft;
	static string defaultMoveDown;
	static string defaultMoveRight;
	static string defaultInteract;
	static string defaultCancel;

	static List<string> ToKeys(string str) {
		List<string> keys = new();
		foreach (string key in str.Split(", ")) if (key != string.Empty) keys.Add(key);
		return keys;
	}

	static string ToString(List<string> keys) {
		string str = "";
		for (int i = 0; i < keys.Count; i++) str += keys[i] + (i != keys.Count - 1 ? ", " : "");
		return str;
	}

	static public void LoadSettings() {
		Language          = PlayerPrefs.GetString("Language", "");
		PixelPerfect      = PlayerPrefs.GetInt   ("PixelPerfect", 1) == 1;
		Music             = PlayerPrefs.GetFloat ("Music", 1f);
		SoundFX           = PlayerPrefs.GetFloat ("SoundFX", 1f);
		MouseSensitivity  = PlayerPrefs.GetFloat ("MouseSensitivity", 1f);

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
		
		UpdateLanguage();
		UpdateFullScreen();
		UpdateScreenResolution();
		UpdatePixelPerfect();
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

	public static void SaveSettings() {
		PlayerPrefs.SetString("Language",         Language);
		PlayerPrefs.SetInt   ("PixelPerfect",     PixelPerfect ? 1 : 0);
		PlayerPrefs.SetFloat ("Music",            Music);
		PlayerPrefs.SetFloat ("SoundFX",          SoundFX);
		PlayerPrefs.SetFloat ("MouseSensitivity", MouseSensitivity);

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



	// ================================================================================================
	// Lifcycle
	// ================================================================================================

	void Start() {
		FadeIn();
		LoadSettings();
		OpenMainMenu();
	}

	void Update() {
		if (InputManager.GetKeyDown(KeyAction.Move)) {
			if (!Selected) Selected = FS;
		}
		if (InputManager.GetKeyDown(KeyAction.Interact)) {
			if (Selected && Selected.TryGetComponent(out Selectable selectable)) {
				if (selectable is CustomButton    customButton   ) customButton   .OnSubmit();
				if (selectable is SettingsButton  settingsButton ) settingsButton .OnSubmit();
				if (selectable is SettingsToggle  settingsToggle ) settingsToggle .OnSubmit();
				if (selectable is SettingsStepper settingsStepper) settingsStepper.OnSubmit();
				if (selectable is SettingsSlider  settingsSlider ) settingsSlider .OnSubmit();
			}
		}
		UpdateDialogue();
		UpdateFade();
		if (InputManager.GetKeyDown(KeyAction.Cancel)) Back();
	}

	void LateUpdate() {
		IsGameRunning = HighestCanvas == CanvasType.Game;
		PeekScreenResolution();
	}



	// ------------------------------------------------------------------------------------------------
	// Main Menu Canvas
	// ------------------------------------------------------------------------------------------------

	public static void OpenMainMenu() {
		Open(CanvasType.MainMenu);
	}



	// ------------------------------------------------------------------------------------------------
	// Game Canvas
	// ------------------------------------------------------------------------------------------------

	public static void OpenGame() {
		Instance.StartCoroutine(OpenGameCoroutine());
	}
	
	static IEnumerator OpenGameCoroutine() {
		FadeOut();
		yield return new WaitForSeconds(1f);
		Open(CanvasType.Game);
		yield return new WaitForSeconds(1f);
		FadeIn();
	}



	// ------------------------------------------------------------------------------------------------
	// Dialogue Canvas
	// ------------------------------------------------------------------------------------------------

	static CustomText dialogueText;
	static Queue<string> dialogueQueue = new Queue<string>();
	static string dialogueString = "";
	static float  dialogueOffset = 0f;
	
	public static UnityEvent OnDialogueEnd { get; private set; } = new UnityEvent();

	public static void UpdateDialogueText(CustomText text) => dialogueText = text;

	public static void EnqueueDialogue(string text) => dialogueQueue.Enqueue(text);

	public static void OpenDialogue(string text = "") {
		if (!text.Equals("")) EnqueueDialogue(text);
		Open(CanvasType.Dialogue);
		Next();
	}

	public static void Next() {
		if (dialogueText.Text.Length < dialogueString.Length) {
			dialogueText.Text = dialogueString;
		}
		else if (dialogueQueue.TryDequeue(out string text)) {
			dialogueText.SetLocalizeText("UI Table", text);
			dialogueString = dialogueText.GetLocalizeText();
			dialogueOffset = 0f;
			dialogueText.Text = "";
		}
		else {
			OnDialogueEnd?.Invoke();
			OnDialogueEnd?.RemoveAllListeners();
			Back();
		}
	}

	static void UpdateDialogue() {
		if (HighestCanvas != CanvasType.Dialogue) return;
		if (InputManager.GetKeyDown(KeyAction.LeftClick)) Next();
		if (InputManager.GetKeyDown(KeyAction.Interact )) Next();
		
		if (dialogueText.Text.Length < dialogueString.Length) {
			dialogueOffset += Time.deltaTime * 20f;
			int l = Mathf.Min((int)dialogueOffset, dialogueString.Length);
			dialogueText.Text = dialogueString[..l];
		}
	}



	// ------------------------------------------------------------------------------------------------
	// Menu Canvas
	// ------------------------------------------------------------------------------------------------

	public static void OpenMenu() {
		Open(CanvasType.Menu);
	}



	// ------------------------------------------------------------------------------------------------
	// Settings Canvas
	// ------------------------------------------------------------------------------------------------

	public static void OpenSettings() {
		Open(CanvasType.Settings);
	}



	static SettingsStepper languageStepper;

	public static void UpdateLanguage(SettingsStepper stepper = null) {
		if (stepper) languageStepper = stepper;
		if (string.IsNullOrEmpty(Language)) Language = Application.systemLanguage.ToString();
		int index = Mathf.Max(0, LocalizationSettings.AvailableLocales.Locales.FindIndex(locale => {
			return locale.Identifier.CultureInfo.NativeName.Equals(Language);
		}));
		string name = LocalizationSettings.SelectedLocale.Identifier.CultureInfo.NativeName;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
		if (languageStepper) languageStepper.Text = Language;
	}

	public static void SetLanguage(int value) {
		int count = LocalizationSettings.AvailableLocales.Locales.Count;
		int index = Mathf.Max(0, LocalizationSettings.AvailableLocales.Locales.FindIndex(locale => {
			return locale.Identifier.CultureInfo.NativeName.Equals(Language);
		}));
		index = (int)Mathf.Repeat(index + value, count);
		Locale locale = LocalizationSettings.AvailableLocales.Locales[index];
		Language = locale.Identifier.CultureInfo.NativeName;
	}



	static int        fullScreen;
	static Vector2Int windowedResolutionSize;

	public static void UpdateFullScreen(SettingsToggle toggle = null) {
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

	public static void SetFullScreen(bool value) {
		fullScreen = value ? 2 : 1;
		Screen.fullScreen = value;

		if (screenResolutionStepper) {
			int screenIndex = Array.FindIndex(ResolutionPresets, preset =>
				preset.x == Screen.width &&
				preset.y == Screen.height);
			int screenIndexFloor = Array.FindLastIndex(ResolutionPresets, preset =>
				preset.x <= Screen.width &&
				preset.y <= Screen.height);
			int screenIndexMax = Array.FindLastIndex(ResolutionPresets, preset =>
				preset.x < Screen.currentResolution.width &&
				preset.y < Screen.currentResolution.height);

			string text       = $"{Screen.width} x {Screen.height}";
			bool interactable = !value;
			bool enablePrev   = !value && screenIndex != 0 && screenIndexFloor != -1;
			bool enableNext   = !value && screenIndexFloor < screenIndexMax;

			screenResolutionStepper.Text         = text;
			screenResolutionStepper.interactable = interactable;
			screenResolutionStepper.EnablePrev   = enablePrev;
			screenResolutionStepper.EnableNext   = enableNext;
		}
	}



	static SettingsStepper screenResolutionStepper;

	public static int ScreenMultiplier => Mathf.Max(1, Mathf.Min(
		Screen.width  / ReferenceResolution.x,
		Screen.height / ReferenceResolution.y));

	public static void UpdateScreenResolution(SettingsStepper stepper = null) {
		if (stepper) screenResolutionStepper = stepper;

		int multiplier = Mathf.Max(1, Mathf.Min(
			Screen.width  / ReferenceResolution.x,
			Screen.height / ReferenceResolution.y));

		int screenIndex = Array.FindIndex(ResolutionPresets, preset =>
			preset.x == Screen.width &&
			preset.y == Screen.height);
		int screenIndexFloor = Array.FindLastIndex(ResolutionPresets, preset =>
			preset.x <= Screen.width &&
			preset.y <= Screen.height);
		int screenIndexMax = Array.FindLastIndex(ResolutionPresets, preset =>
			preset.x < Screen.currentResolution.width &&
			preset.y < Screen.currentResolution.height);
		
		if (CanvasScaler) CanvasScaler.scaleFactor = multiplier;
		if (CameraManager.Instance) {
			Vector2Int size = new Vector2Int(Screen.width, Screen.height);
			if (PixelPerfect) {
				size.x = (int)Mathf.Ceil(Screen.width  / multiplier);
				size.y = (int)Mathf.Ceil(Screen.height / multiplier);
			}
			if (size != Vector2Int.zero) {
				CameraManager.RenderTextureSize = size;
				CameraManager. OrthographicSize = Screen.height / 2 / multiplier / PixelPerUnit;
			}
		}
		if (screenResolutionStepper) {
			string text       = $"{Screen.width} x {Screen.height}";
			bool interactable = !Screen.fullScreen;
			bool enablePrev   = !Screen.fullScreen && screenIndex != 0 && screenIndexFloor != -1;
			bool enableNext   = !Screen.fullScreen && screenIndexFloor < screenIndexMax;

			screenResolutionStepper.Text         = text;
			screenResolutionStepper.interactable = interactable;
			screenResolutionStepper.EnablePrev   = enablePrev;
			screenResolutionStepper.EnableNext   = enableNext;
		}
	}

	public static void SetScreenResolution(int value) {
		int screenIndex = Array.FindIndex(ResolutionPresets, preset =>
			preset.x == Screen.width &&
			preset.y == Screen.height);
		int screenIndexFloor = Array.FindLastIndex(ResolutionPresets, preset =>
			preset.x <= Screen.width &&
			preset.y <= Screen.height);
		int screenIndexMax = Array.FindLastIndex(ResolutionPresets, preset =>
			preset.x < Screen.currentResolution.width &&
			preset.y < Screen.currentResolution.height);
		
		if (value == -1 && screenIndex == -1) value = 0;
		int index = Mathf.Clamp(screenIndexFloor + value, 0, screenIndexMax);
		Vector2Int resolution = ResolutionPresets[index];
		Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
	}

	static Vector2Int screenResolutionSize;

	static void PeekScreenResolution() {
		if (screenResolutionSize.x != Screen.width || screenResolutionSize.y != Screen.height) {
			screenResolutionSize = new Vector2Int(Screen.width, Screen.height);
			UpdateScreenResolution(screenResolutionStepper);
		}
	}



	static SettingsToggle pixelPerfectToggle;

	public static void UpdatePixelPerfect(SettingsToggle toggle = null) {
		if (toggle) pixelPerfectToggle = toggle;
		UpdateScreenResolution();
		if (pixelPerfectToggle) pixelPerfectToggle.Value = PixelPerfect;
	}

	public static void SetPixelPerfect(bool value) {
		PixelPerfect = value;
	}



	static SettingsSlider musicSlider;

	public static void UpdateMusic(SettingsSlider slider = null) {
		if (slider) musicSlider = slider;
		if (musicSlider) musicSlider.Value = Music;
	}

	public static void SetMusic(float value) {
		Music = value;
	}

	static SettingsSlider soundFXSlider;

	public static void UpdateSoundFX(SettingsSlider slider = null) {
		if (slider) soundFXSlider = slider;
		if (soundFXSlider) soundFXSlider.Value = SoundFX;
	}

	public static void SetSoundFX(float value) {
		SoundFX = value;
	}

	static SettingsSlider mouseSensitivitySlider;

	public static void UpdateMouseSensitivity(SettingsSlider slider = null) {
		if (slider) mouseSensitivitySlider = slider;
		if (mouseSensitivitySlider) mouseSensitivitySlider.Value = MouseSensitivity;
	}

	public static void SetMouseSensitivity(float value) {
		MouseSensitivity = value;
	}



	static SettingsButton[] action = new SettingsButton[Enum.GetValues(typeof(KeyAction)).Length];

	public static void UpdateMoveUp   (SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveUp);
	public static void UpdateMoveLeft (SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveLeft);
	public static void UpdateMoveDown (SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveDown);
	public static void UpdateMoveRight(SettingsButton button = null) => UpdateKeys(button, KeyAction.MoveRight);
	public static void UpdateInteract (SettingsButton button = null) => UpdateKeys(button, KeyAction.Interact);
	public static void UpdateCancel   (SettingsButton button = null) => UpdateKeys(button, KeyAction.Cancel);

	static void UpdateKeys(SettingsButton button, KeyAction keyAction) {
		if (button) action[(int)keyAction] = button;
		string str = ToString(InputManager.GetKeysBinding(keyAction));
		str = str.Replace("upArrow",    "↑");
		str = str.Replace("leftArrow",  "←");
		str = str.Replace("downArrow",  "↓");
		str = str.Replace("rightArrow", "→");
		if (action[(int)keyAction]) action[(int)keyAction].Text = str;
	}

	public static void SetMoveUp   () => SetKeys(KeyAction.MoveUp);
	public static void SetMoveLeft () => SetKeys(KeyAction.MoveLeft);
	public static void SetMoveDown () => SetKeys(KeyAction.MoveDown);
	public static void SetMoveRight() => SetKeys(KeyAction.MoveRight);
	public static void SetInteract () => SetKeys(KeyAction.Interact);
	public static void SetCancel   () => SetKeys(KeyAction.Cancel);

	static void SetKeys(KeyAction keyAction) {
		if (Instance) Instance.StartCoroutine(SetKeysCoroutine(keyAction));
	}
	static IEnumerator SetKeysCoroutine(KeyAction keyAction) {
		OpenConfirmation("Binding", "Binding Message", "Apply Binding", "Cancel Binding");
		List<string> keys = new();
		if (confirmationPositive) confirmationPositive.OnClick.AddListener(() => {
			InputManager.SetKeysBinding(keyAction, keys);
		});
		InputManager.RecordKeys();
		while ((ActiveCanvas & CanvasType.Confirmation) != 0) {
			yield return null;
			switch (InputManager.RecordedKey) {
				case "":
					break;
				case "enter":
					if (!Selected || !Selected.TryGetComponent(out Selectable selectable)) {
						if (confirmationPositive) confirmationPositive.OnClick.Invoke();
					}
					break;
				case "escape":
					break;
				default:
					if (!keys.Exists(key => key.Equals(InputManager.RecordedKey))) {
						keys.Add(InputManager.RecordedKey);
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
		InputManager.StopRecordKeys();
		if (action[(int)keyAction]) action[(int)keyAction].Refresh();
	}



	public static void SetDeleteAllData() {
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

	static CustomText   confirmationTitle;
	static CustomText   confirmationMessage;
	static CustomButton confirmationPositive;
	static CustomButton confirmationNegative;

	public static void UpdateConfirmationTitle   (CustomText   text  ) => confirmationTitle    = text;
	public static void UpdateConfirmationMessage (CustomText   text  ) => confirmationMessage  = text;
	public static void UpdateConfirmationPositive(CustomButton button) => confirmationPositive = button;
	public static void UpdateConfirmationNegative(CustomButton button) => confirmationNegative = button;

	public static void OpenConfirmation(string arg0, string arg1, string arg2, string arg3) {
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

	static int   fadeState;
	static Image fadeImage;

	public static void FadeOut() {
		if (fadeImage || Fade.TryGetComponent(out fadeImage)) {
			ActiveCanvas |= CanvasType.Fade;
			fadeState = 1;
		}
	}

	public static void FadeIn(bool force = false) {
		if (fadeImage || Fade.TryGetComponent(out fadeImage)) {
			ActiveCanvas |= CanvasType.Fade;
			fadeState = 2;
			if (force) {
				Color color = fadeImage.color;
				color.a = 1.0f;
				fadeImage.color = color;
			}
		}
	}

	static void UpdateFade() {
		if (fadeState != 0) {
			Color color = fadeImage.color;
			color.a = Mathf.MoveTowards(color.a, fadeState == 1 ? 1.0f : 0.0f, Time.deltaTime * 2f);
			fadeImage.color = color;
			if (color.a == 0.0f) {
				ActiveCanvas &= ~CanvasType.Fade;
				fadeState = 0;
			}
		}
	}
}

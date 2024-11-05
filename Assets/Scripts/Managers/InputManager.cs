using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



[Serializable] public enum KeyAction {
	// Common (Bindable)
	MoveUp,
	MoveLeft,
	MoveDown,
	MoveRight,
	Interact,
	Cancel,
	// Game (Bindable)
	// - Skill, Dash, Etc.
	// UI
	Point,
	LeftClick,
	MiddleClick,
	RightClick,
	Scroll,
	Move,
	Control,
}



// ====================================================================================================
// Input Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(InputManager)), CanEditMultipleObjects]
	public class InputManagerEditor : Editor {

		InputManager I => target as InputManager;

		void OnEnable() {
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Space();
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Input Manager
// ====================================================================================================

public class InputManager : MonoSingleton<InputManager> {

	// Fields



	// Properties



	// Methods

	static bool[] keyPrev = new bool[Enum.GetValues(typeof(KeyAction)).Length];
	static bool[] keyNext = new bool[Enum.GetValues(typeof(KeyAction)).Length];

	public static bool GetKey    (KeyAction key) =>  keyNext[(int)key];
	public static bool GetKeyDown(KeyAction key) =>  keyNext[(int)key] && !keyPrev[(int)key];
	public static bool GetKeyUp  (KeyAction key) => !keyNext[(int)key] &&  keyPrev[(int)key];

	public static Vector2 moveDirection { get; private set; }
	public static Vector2 pointPosition { get; private set; }
	public static Vector2 scrollWheel   { get; private set; }

	static PlayerInput playerInput;
	static InputAction inputAction;

	public static List<string> GetKeysBinding(KeyAction keyAction) {
		List<string> keys = new List<string>();
		if (playerInput || (Instance && Instance.TryGetComponent(out playerInput))) {
			inputAction = playerInput.actions.FindAction(keyAction.ToString());
			if (inputAction != null) {
				for (int i = 0; i < inputAction.bindings.Count; i++) {
					string[] parts = inputAction.bindings[i].path.Split('/');
					if (parts[0].Equals("<Keyboard>")) keys.Add(parts[1]);
				}
			}
		}
		return keys;
	}

	public static void SetKeysBinding(KeyAction keyAction, List<string> keys) {
		if (playerInput || (Instance && Instance.TryGetComponent(out playerInput))) {
			inputAction = playerInput.actions.FindAction(keyAction.ToString());
			if (inputAction != null) {
				for (int i = inputAction.bindings.Count - 1; -1 < i; i--) {
					string[] parts = inputAction.bindings[i].path.Split('/');
					if (parts[0].Equals("<Keyboard>")) inputAction.ChangeBinding(i).Erase();
				}
				foreach (string key in keys) inputAction.AddBinding("<Keyboard>/" + key);
			}
		}
		SetMoveKeysBinding();
	}

	static void SetMoveKeysBinding() {
		List<string> keysUp    = GetKeysBinding(KeyAction.MoveUp   );
		List<string> keysLeft  = GetKeysBinding(KeyAction.MoveLeft );
		List<string> keysDown  = GetKeysBinding(KeyAction.MoveDown );
		List<string> keysRight = GetKeysBinding(KeyAction.MoveRight);
		if (playerInput || (Instance && Instance.TryGetComponent(out playerInput))) {
			inputAction = playerInput.actions.FindAction(KeyAction.Move.ToString());
			if (inputAction != null) {
				for (int i = inputAction.bindings.Count - 1; -1 < i; i--) {
					string[] parts = inputAction.bindings[i].path.Split('/');
					if (parts[0].Equals("<Keyboard>")) inputAction.ChangeBinding(i).Erase();
				}
				var composite = inputAction.AddCompositeBinding("2DVector");
				foreach (string key in keysUp   ) composite.With("Up",    "<Keyboard>/" + key);
				foreach (string key in keysLeft ) composite.With("Left",  "<Keyboard>/" + key);
				foreach (string key in keysDown ) composite.With("Down",  "<Keyboard>/" + key);
				foreach (string key in keysRight) composite.With("Right", "<Keyboard>/" + key);
			}
		}
	}

	static bool recordKeys = false;

	public static string  anyKey { get; private set; }

	public static void RecordKeys() {
		anyKey = null;
		recordKeys = true;
	}

	public static void StopRecordKeys() {
		recordKeys = false;
	}



	// Cycle

	void Start() {
		if (playerInput || TryGetComponent(out playerInput)) {
			for (int i = 0; i < keyNext.Length; i++) {
				int index = i;
				Action<InputAction.CallbackContext> callback = (KeyAction)index switch {
					KeyAction.Move   => context => moveDirection  = context.ReadValue<Vector2>(),
					KeyAction.Point  => context => pointPosition  = context.ReadValue<Vector2>(),
					KeyAction.Scroll => context => scrollWheel    = context.ReadValue<Vector2>(),
					_                => context => keyNext[index] = context.action.IsPressed(),
				};
				playerInput.actions[((KeyAction)i).ToString()].performed += callback;
			}
		}
		InputSystem.onAnyButtonPress.Call(inputControl => {
			if (recordKeys) {
				string[] parts = inputControl.path.Split('/');
				if (parts[1].Equals("Keyboard")) anyKey = parts[2];
				// path: "/Device/Key"
			}
		});
	}

	void LateUpdate() {
		for (int i = 0; i < keyPrev.Length; i++) keyPrev[i] = keyNext[i];
	}
}

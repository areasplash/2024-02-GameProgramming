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
	MoveUp,
	MoveLeft,
	MoveDown,
	MoveRight,
	Interact,
	Cancel,
	
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

		void OnEnable() {
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Input Manager Properties");
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

	// ------------------------------------------------------------------------------------------------
	// Fields
	// ------------------------------------------------------------------------------------------------



	// ------------------------------------------------------------------------------------------------
	// Properties
	// ------------------------------------------------------------------------------------------------

	public static Vector2 MoveDirection { get; private set; }
	public static Vector2 PointPosition { get; private set; }
	public static Vector2 ScrollWheel   { get; private set; }

	public static string RecordedKey { get; private set; }



	// ------------------------------------------------------------------------------------------------
	// Methods
	// ------------------------------------------------------------------------------------------------

	static PlayerInput playerInput;
	static InputAction inputAction;

	

	// Move Direction

	static Vector2 moveDirection;

	static void BindMoveDirection() {
		if (playerInput || (Instance && Instance.TryGetComponent(out playerInput))) {
			playerInput.actions[KeyAction.Move.ToString()].performed += context => {
				moveDirection = context.ReadValue<Vector2>();
			};
		}
	}

	static void UpdateMoveDirection() {
		Vector2 md = moveDirection;
		Vector2 dir = Vector2.zero;
		if (GetKey(KeyAction.MoveUp   ) && (0 < md.y || GetKeyUp(KeyAction.MoveDown ))) dir.y = +1;
		if (GetKey(KeyAction.MoveLeft ) && (md.x < 0 || GetKeyUp(KeyAction.MoveRight))) dir.x = -1;
		if (GetKey(KeyAction.MoveDown ) && (md.y < 0 || GetKeyUp(KeyAction.MoveUp   ))) dir.y = -1;
		if (GetKey(KeyAction.MoveRight) && (0 < md.x || GetKeyUp(KeyAction.MoveLeft ))) dir.x = +1;
		if (GetKeyDown(KeyAction.MoveUp   )) dir.y = +1;
		if (GetKeyDown(KeyAction.MoveLeft )) dir.x = -1;
		if (GetKeyDown(KeyAction.MoveDown )) dir.y = -1;
		if (GetKeyDown(KeyAction.MoveRight)) dir.x = +1;
		dir.Normalize();
		MoveDirection = dir != Vector2.zero? dir : moveDirection;
	}



	// Point Position

	static Vector2 pointPosition;

	static void BindPointPosition() {
		if (playerInput || (Instance && Instance.TryGetComponent(out playerInput))) {
			playerInput.actions[KeyAction.Point.ToString()].performed += context => {
				PointPosition = context.ReadValue<Vector2>();
			};
		}
	}

	static void UpdatePointPosition() {
		pointPosition = PointPosition;
	}



	// Scroll Wheel

	static void BindScrollWheel() {
		if (playerInput || (Instance && Instance.TryGetComponent(out playerInput))) {
			playerInput.actions[KeyAction.Scroll.ToString()].performed += context => {
				ScrollWheel = context.ReadValue<Vector2>();
			};
		}
	}



	// Record Keys

	public static void RecordKeys    () => RecordedKey = "";
	public static void StopRecordKeys() => RecordedKey = null;

	static void BindRecordKeys() {
		InputSystem.onAnyButtonPress.Call(inputControl => {
			if (RecordedKey != null) {
				string[] parts = inputControl.path.Split('/');
				if (parts[1].Equals("Keyboard")) RecordedKey = parts[2];
				// path: "/Device/Key"
			}
		});
	}



	// Keys

	static readonly bool[] keyPrev = new bool[Enum.GetValues(typeof(KeyAction)).Length];
	static readonly bool[] keyNext = new bool[Enum.GetValues(typeof(KeyAction)).Length];

	public static bool GetKey    (KeyAction key) =>  keyNext[(int)key];
	public static bool GetKeyDown(KeyAction key) =>  keyNext[(int)key] && !keyPrev[(int)key];
	public static bool GetKeyUp  (KeyAction key) => !keyNext[(int)key] &&  keyPrev[(int)key];

	static void BindKeys() {
		if (playerInput || (Instance && Instance.TryGetComponent(out playerInput))) {
			for (int i = 0; i < keyNext.Length; i++) {
				int index = i;
				playerInput.actions[((KeyAction)i).ToString()].performed += (KeyAction)index switch {
					KeyAction.Move   => context => keyNext[index] = moveDirection != Vector2.zero,
					KeyAction.Point  => context => keyNext[index] = PointPosition != pointPosition,
					KeyAction.Scroll => context => keyNext[index] = ScrollWheel   != Vector2.zero,
					_                => context => keyNext[index] = context.action.IsPressed(),
				};
			}
		}
	}

	static void UpdateKeys() {
		for (int i = 0; i < keyPrev.Length; i++) keyPrev[i] = keyNext[i];
	}



	// Bindings

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
		UpdateMoveBinding();
	}

	static void UpdateMoveBinding() {
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



	// ------------------------------------------------------------------------------------------------
	// Cycle
	// ------------------------------------------------------------------------------------------------

	void Start() {
		BindMoveDirection();
		BindPointPosition();
		BindScrollWheel();
		BindRecordKeys();
		BindKeys();
	}

	void LateUpdate() {
		UpdateMoveDirection();
		UpdatePointPosition();
		UpdateKeys();
	}
}

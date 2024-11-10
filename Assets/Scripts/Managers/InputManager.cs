using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Input Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(InputManager)), CanEditMultipleObjects]
	public class InputManagerEditor : Editor {

		SerializedProperty m_InputActionAsset;

		InputManager I => target as InputManager;

		void OnEnable() {
			m_InputActionAsset = serializedObject.FindProperty("m_InputActionAsset");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Input Manager Properties");
			Space();
			LabelField("Actions", EditorStyles.boldLabel);
			PropertyField(m_InputActionAsset);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Input Manager
// ====================================================================================================

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



public class InputManager : MonoSingleton<InputManager> {

	// Serialized Fields

	[SerializeField] InputActionAsset m_InputActionAsset;



	// Properties

	InputActionAsset Actions => m_InputActionAsset;

	public Vector2 MoveDirection { get; private set; }
	public Vector2 PointPosition { get; private set; }
	public Vector2 ScrollWheel   { get; private set; }

	public string RecordedKey { get; private set; }



	// Cached Variables

	readonly bool[] keyPrev = new bool[Enum.GetValues(typeof(KeyAction)).Length];
	readonly bool[] keyNext = new bool[Enum.GetValues(typeof(KeyAction)).Length];

	Vector2 moveDirection;
	Vector2 pointPosition;

	InputAction inputAction;



	// Methods

	public bool GetKey    (KeyAction key) =>  keyNext[(int)key];
	public bool GetKeyDown(KeyAction key) =>  keyNext[(int)key] && !keyPrev[(int)key];
	public bool GetKeyUp  (KeyAction key) => !keyNext[(int)key] &&  keyPrev[(int)key];

	void BindKeys() {
		if (!Actions) return;
		for (int i = 0; i < keyNext.Length; i++) {
			int index = i;
			Actions[((KeyAction)i).ToString()].performed += (KeyAction)index switch {
				KeyAction.Move   => context => moveDirection = context.ReadValue<Vector2>(),
				KeyAction.Point  => context => PointPosition = context.ReadValue<Vector2>(),
				KeyAction.Scroll => context => ScrollWheel   = context.ReadValue<Vector2>(),
				_                => context => {},
			};
			Actions[((KeyAction)i).ToString()].performed += (KeyAction)index switch {
				KeyAction.Move   => context => keyNext[index] = MoveDirection != Vector2.zero,
				KeyAction.Point  => context => keyNext[index] = PointPosition != pointPosition,
				KeyAction.Scroll => context => keyNext[index] = ScrollWheel   != Vector2.zero,
				_                => context => keyNext[index] = context.action.IsPressed(),
			};
		}
	}

	void PeekKeys() {
		Vector2 md = MoveDirection;
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
		pointPosition = PointPosition;
		for (int i = 0; i < keyPrev.Length; i++) keyPrev[i] = keyNext[i];
	}



	public List<string> GetKeysBinding(KeyAction keyAction) {
		List<string> keys = new List<string>();
		if (Actions) {
			inputAction = Actions.FindAction(keyAction.ToString());
			if (inputAction != null) {
				for (int i = 0; i < inputAction.bindings.Count; i++) {
					string[] parts = inputAction.bindings[i].path.Split('/');
					if (parts[0].Equals("<Keyboard>")) keys.Add(parts[1]);
					// path: "<Device>/Key"
				}
			}
		}
		return keys;
	}

	public void SetKeysBinding(KeyAction keyAction, List<string> keys) {
		if (Actions) {
			inputAction = Actions.FindAction(keyAction.ToString());
			if (inputAction != null) {
				for (int i = inputAction.bindings.Count - 1; -1 < i; i--) {
					string[] parts = inputAction.bindings[i].path.Split('/');
					if (parts[0].Equals("<Keyboard>")) inputAction.ChangeBinding(i).Erase();
					// path: "<Device>/Key"
				}
				foreach (string key in keys) inputAction.AddBinding("<Keyboard>/" + key);
			}
		}
		UpdateMoveBinding();
	}

	void UpdateMoveBinding() {
		if (!Actions) return;
		List<string> keysUp    = GetKeysBinding(KeyAction.MoveUp   );
		List<string> keysLeft  = GetKeysBinding(KeyAction.MoveLeft );
		List<string> keysDown  = GetKeysBinding(KeyAction.MoveDown );
		List<string> keysRight = GetKeysBinding(KeyAction.MoveRight);
		inputAction = Actions.FindAction(KeyAction.Move.ToString());
		if (inputAction != null) {
			for (int i = inputAction.bindings.Count - 1; -1 < i; i--) {
				string[] parts = inputAction.bindings[i].path.Split('/');
				if (parts[0].Equals("<Keyboard>")) inputAction.ChangeBinding(i).Erase();
				// path: "<Device>/Key"
			}
			var composite = inputAction.AddCompositeBinding("2DVector");
			foreach (string key in keysUp   ) composite.With("Up",    "<Keyboard>/" + key);
			foreach (string key in keysLeft ) composite.With("Left",  "<Keyboard>/" + key);
			foreach (string key in keysDown ) composite.With("Down",  "<Keyboard>/" + key);
			foreach (string key in keysRight) composite.With("Right", "<Keyboard>/" + key);
		}
	}



	public void RecordKeys    () => RecordedKey = "";
	public void StopRecordKeys() => RecordedKey = null;

	void BindRecordKeys() {
		InputSystem.onAnyButtonPress.Call(inputControl => {
			if (RecordedKey != null) {
				string[] parts = inputControl.path.Split('/');
				if (parts[1].Equals("Keyboard")) RecordedKey = parts[2];
				// path: "/Device/Key"
			}
		});
	}



	// Lifecycle

	void Start() {
		BindKeys();
		BindRecordKeys();
	}

	void LateUpdate() {
		PeekKeys();
	}
}

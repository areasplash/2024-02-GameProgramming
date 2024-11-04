using UnityEngine;
using UnityEngine.InputSystem;

using System;
using System.Collections.Generic;

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
	public static string  anyKey        { get; private set; }

	static PlayerInput playerInput;
	static InputAction inputAction;

	public static List<string> GetKeysBinding(KeyAction key) {
		List<string> keys = new List<string>();
		if (playerInput || Instance.TryGetComponent(out playerInput)) {
			inputAction = playerInput.actions.FindAction(key.ToString());
			if (inputAction != null) {
				for (int i = 0; i < inputAction.bindings.Count; i++) {
					string[] parts = inputAction.bindings[i].path.Split('/');
					if (parts[0].Equals("<Keyboard>")) keys.Add(parts[1]);
				}
			}
		}
		return keys;
	}

	public static void SetKeysBinding(KeyAction key, List<string> keys) {
		if (playerInput || Instance.TryGetComponent(out playerInput)) {
			inputAction = playerInput.actions.FindAction(key.ToString());
			if (inputAction != null) {
				for (int i = inputAction.bindings.Count - 1; -1 < i; i--) {
					inputAction.ChangeBinding(i).Erase();
				}
				for (int i = 0; i < keys.Count; i++) {
					inputAction.AddBinding("<Keyboard>/" + keys[i]);
				}
			}
		}
	}



	// Cycle

	void Start() {
		//InputSystem.onAnyButtonPress.CallOnce(ctrl => Debug.Log($"{ctrl} pressed"));
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
	}

	void LateUpdate() {
		for (int i = 0; i < keyPrev.Length; i++) keyPrev[i] = keyNext[i];
	}



	Vector3 mousePosition;
	Vector3 eulerAngles;

	void Update() {
		if (GetKeyDown(KeyAction.LeftClick)) {
			List<string> strings = GetKeysBinding(KeyAction.Interact);
			for (int i = 0; i < strings.Count; i++) Debug.Log(strings[i]);
		}
		if (GetKeyDown(KeyAction.RightClick)) {
			SetKeysBinding(KeyAction.Interact, new List<string> { "space" });
		}

		if (CameraManager.Instance) {
			if (GetKeyDown(KeyAction.RightClick)) {
				mousePosition = pointPosition;
				eulerAngles = CameraManager.Instance.transform.eulerAngles;
			}
			if (GetKey(KeyAction.RightClick)) {
				CameraManager.Instance.transform.rotation = Quaternion.Euler(
					eulerAngles.x,
					eulerAngles.y + (pointPosition.x - mousePosition.x) * 1f,
					eulerAngles.z);
			}
		}

		if (GetKeyDown(KeyAction.LeftClick)) {
			Ray ray = CameraManager.ScreenPointToRay(pointPosition);
			if (Physics.Raycast(ray, out RaycastHit hit)) {
				Debug.Log(hit.point);
			}
		}

		if (GetKeyDown(KeyAction.Cancel)) UIManager.Back();
	}
}

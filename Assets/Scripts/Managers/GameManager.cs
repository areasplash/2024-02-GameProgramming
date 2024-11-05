using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Game Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(GameManager)), CanEditMultipleObjects]
	public class GameManagerEditor : Editor {

		GameManager I => target as GameManager;

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
// Game Manager
// ====================================================================================================

public class GameManager : MonoSingleton<GameManager> {

	// Fields



	// Properties
	


	// Methods



	// Cycle

	Vector2 pointPosition;
	Vector3 eulerAngles;

	void Update() {
		if (CameraManager.Instance) {
			if (InputManager.GetKeyDown(KeyAction.RightClick)) {
				pointPosition = InputManager.pointPosition;
				eulerAngles = CameraManager.Instance.transform.eulerAngles;
			}
			if (InputManager.GetKey(KeyAction.RightClick)) {
				CameraManager.Instance.transform.rotation = Quaternion.Euler(
					eulerAngles.x,
					eulerAngles.y + (InputManager.pointPosition.x - pointPosition.x) * 1f,
					eulerAngles.z);
			}
		}

		if (InputManager.GetKeyDown(KeyAction.LeftClick)) {
			Ray ray = CameraManager.ScreenPointToRay(InputManager.pointPosition);
			if (Physics.Raycast(ray, out RaycastHit hit)) {
				Debug.Log(hit.point);
			}
		}

		if (InputManager.GetKeyDown(KeyAction.Cancel)) UIManager.Back();
	}
}

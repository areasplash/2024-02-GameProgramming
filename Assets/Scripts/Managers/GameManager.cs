using UnityEngine;

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
	Vector3 rotation;

	void Update() {
		if (UIManager.GetActiveCanvas() == CanvasType.Game) {
			if (InputManager.GetKeyDown(KeyAction.LeftClick)) {
				Ray ray = CameraManager.ScreenPointToRay(InputManager.PointPosition);
				if (Physics.Raycast(ray, out RaycastHit hit)) {
					Debug.Log(hit.point);
				}
			}
			if (InputManager.GetKeyDown(KeyAction.RightClick)) {
				pointPosition = InputManager.PointPosition;
				rotation = CameraManager.Rotation;
			}
			if (InputManager.GetKey(KeyAction.RightClick)) {
				float delta = (InputManager.PointPosition.x - pointPosition.x) * 1f;
				CameraManager.Rotation = rotation + new Vector3(0, delta, 0);
			}
		}
	}
}

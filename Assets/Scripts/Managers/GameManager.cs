using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
	using static GameManager;
#endif



// ====================================================================================================
// Game Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(GameManager)), CanEditMultipleObjects]
	public class GameManagerEditor : ExtendedEditor {

		SerializedProperty m_Player;

		GameManager I => target as GameManager;

		void OnEnable() {
			m_Player = serializedObject.FindProperty("m_Player");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			LabelField("Player", EditorStyles.boldLabel);
			PropertyField(m_Player);
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

	// Serialized Fields

	[SerializeField] Creature m_Player;



	// Lifecycle

	Vector2 pointPosition;
	Vector3 rotation;

	void Update() {
		if (m_Player && UIManager.Instance.ActiveCanvas == CanvasType.Game) {
			{
				Vector3 direction = Vector3.zero;
				direction += CameraManager.Instance.transform.right   * InputManager.MoveDirection.x;
				direction += CameraManager.Instance.transform.forward * InputManager.MoveDirection.y;
				direction.y = 0;
				direction.Normalize();
				m_Player.input = direction;
			}
			if (InputManager.GetKeyDown(KeyAction.LeftClick)) {
				Ray ray = CameraManager.ScreenPointToRay(InputManager.PointPosition);
				if (Physics.Raycast(ray, out RaycastHit hit)) {
					Vector3 start = m_Player.transform.position;
					m_Player.queue.Clear();
					NavMeshManager.Instance.FindPath(start, hit.point, ref m_Player.queue, 0.75f);
				}
			}
			if (InputManager.GetKeyDown(KeyAction.RightClick)) {
				pointPosition = InputManager.PointPosition;
				rotation = CameraManager.EulerRotation;
			}
			if (InputManager.GetKey(KeyAction.RightClick)) {
				float mouseSensitivity = UIManager.Instance.MouseSensitivity;
				float delta = InputManager.PointPosition.x - pointPosition.x;
				CameraManager.EulerRotation = rotation + new Vector3(0, delta * mouseSensitivity, 0);
			}
		}
	}
}

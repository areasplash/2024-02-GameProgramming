using UnityEngine;
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

		SerializedProperty m_Player;

		GameManager I => target as GameManager;

		void OnEnable() {
			m_Player = serializedObject.FindProperty("m_Player");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Space();
			LabelField("Player", EditorStyles.boldLabel);
			PropertyField(m_Player);
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



	// Properties



	// Methods



	// Lifecycle

	Vector2 pointPosition;
	Vector3 rotation;

	void Update() {
		if (m_Player && UIManager.I.ActiveCanvas == CanvasType.Game) {
			if (InputManager.I.GetKeyDown(KeyAction.LeftClick)) {
				Ray ray = CameraManager.I.ScreenPointToRay(InputManager.I.PointPosition);
				if (Physics.Raycast(ray, out RaycastHit hit)) {
					
					Vector3 start = m_Player.transform.position;
					m_Player.queue.Clear();
					NavMeshManager.I.FindPath(start, hit.point, ref m_Player.queue, 0.75f);
				}
			}
			if (InputManager.I.GetKeyDown(KeyAction.RightClick)) {
				pointPosition = InputManager.I.PointPosition;
				rotation = CameraManager.I.Rotation;
			}
			if (InputManager.I.GetKey(KeyAction.RightClick)) {
				float mouseSensitivity = UIManager.I.MouseSensitivity;
				float delta = InputManager.I.PointPosition.x - pointPosition.x;
				CameraManager.I.Rotation = rotation + new Vector3(0, delta * mouseSensitivity, 0);
			}
			//Vector3 direction = Vector3.zero;
			//direction += CameraManager.I.transform.right   * InputManager.I.MoveDirection.x;
			//direction += CameraManager.I.transform.forward * InputManager.I.MoveDirection.y;
			//direction.y = 0;
			//direction.Normalize();
			//m_Player.transform.position += direction * 5 * Time.deltaTime;
		}
	}
}

using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ====================================================================================================
// Camera Manager
// ====================================================================================================

public class CameraManager : MonoSingleton<CameraManager> {

	[SerializeField] bool isPerspective = false;

	[SerializeField] Camera mainCamera;
	[SerializeField] Camera fadeCamera;


	public bool IsPerspective {
		get => isPerspective;
		set {
			isPerspective = value;
			mainCamera.orthographic = !isPerspective;
			fadeCamera.orthographic = !isPerspective;
		}
	}

	
	void Update() {
		if (transform.hasChanged) {
			transform.position = GetPixelated(transform.position);
			transform.hasChanged = false;
		}
	}

	public static Vector3 GetPixelated(Vector3 position) {
		position.x = Mathf.Round(position.x);
		position.y = Mathf.Round(position.y);
		position.z = Mathf.Round(position.z);
		return position;
	}
}



// ====================================================================================================
// Game Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(CameraManager))]
	public class GameManagerEditor : Editor {
		CameraManager CameraManager;

		//SerializedProperty AtlasMapSO;

		void OnEnable() {
			CameraManager = target as CameraManager;

			//AtlasMapSO = serializedObject.FindProperty("atlasMapSO");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Camera Manager", EditorStyles.boldLabel);
			//EditorGUILayout.PropertyField(AtlasMapSO);

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif

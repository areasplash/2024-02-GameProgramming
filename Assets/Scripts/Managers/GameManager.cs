using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ====================================================================================================
// Game Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(GameManager)), CanEditMultipleObjects]
	public class GameManagerEditor : ExtendedEditor {

		SerializedProperty m_ClientSpawnPoint;
		SerializedProperty m_SpawnPeriod;

		GameManager I => target as GameManager;

		void OnEnable() {
			m_ClientSpawnPoint = serializedObject.FindProperty("m_ClientSpawnPoint");
			m_SpawnPeriod      = serializedObject.FindProperty("m_SpawnPeriod");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			LabelField("Client", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(m_ClientSpawnPoint);
			EditorGUILayout.PropertyField(m_SpawnPeriod);
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

	[SerializeField] Transform m_ClientSpawnPoint;
	[SerializeField, Range(0f, 20f)] float m_SpawnPeriod = 5f;



	// Properties

	public static Vector3 ClientSpawnPoint =>
		Instance && Instance.m_ClientSpawnPoint ?
		Instance.m_ClientSpawnPoint.position : default;



	// Lifecycle

	float spawnTimer = 0f;

	void Update() {
		spawnTimer -= Time.deltaTime;
		if (m_ClientSpawnPoint && spawnTimer <= 0f) {
			spawnTimer = m_SpawnPeriod;
			Creature.Spawn(CreatureType.Client, m_ClientSpawnPoint.position);
		}
	}
}

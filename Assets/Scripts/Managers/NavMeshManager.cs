using UnityEngine;

using System.Collections.Generic;

using UnityEngine.AI;
using Unity.AI.Navigation;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// NavMesh Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(NavMeshManager)), CanEditMultipleObjects]
	public class NavMeshManagerEditor : Editor {

		SerializedProperty m_NavMeshSurfaces;

		NavMeshManager I => target as NavMeshManager;

		void OnEnable() {
			m_NavMeshSurfaces = serializedObject.FindProperty("m_NavMeshSurfaces");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change NavMesh Manager Properties");
			Space();
			LabelField("NavMesh", EditorStyles.boldLabel);
			PropertyField(m_NavMeshSurfaces);
			Space();
			LabelField("Actions", EditorStyles.boldLabel);
			BeginHorizontal();
			{
				PrefixLabel("Bake NavMesh");
				if (GUILayout.Button("Bake")) I.Bake();
			}
			EndHorizontal();
			BeginHorizontal();
			{
				PrefixLabel("Clear NavMesh");
				if (GUILayout.Button("Clear")) I.Clear();
			}
			EndHorizontal();
			/*if (GUILayout.Button("Test")) {
				for (int i = 0; i < m_NavMeshSurfaces.arraySize; i++) {
					var navMeshSurface = m_NavMeshSurfaces.GetArrayElementAtIndex(i)
						.objectReferenceValue as NavMeshSurface;
					Debug.Log(navMeshSurface.agentTypeID);
				}
			}*/
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// NavMesh Manager
// ====================================================================================================

public class NavMeshManager : MonoSingleton<NavMeshManager> {

	[SerializeField] List<NavMeshSurface> m_NavMeshSurfaces = new List<NavMeshSurface>();

	

	public void Bake() {
		foreach (var navMeshSurface in m_NavMeshSurfaces) navMeshSurface.BuildNavMesh();
	}

	public void Clear() {
		foreach (var navMeshSurface in m_NavMeshSurfaces) navMeshSurface.RemoveData();
	}



	NavMeshPath path;

	void CalculatePath(Vector3 start, Vector3 end) {
		path.ClearCorners();
		if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path)) {
			for (int i = 0; i < path.corners.Length - 1; i++) {
				Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 1);
			}
		}
	}
}

using UnityEngine;

using System.Collections.Generic;

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

		//SerializedProperty m_MainCamera;

		NavMeshManager I => target as NavMeshManager;

		void OnEnable() {
			//m_MainCamera     = serializedObject.FindProperty("m_MainCamera");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change NavMes hManager Properties");
			Space();
			LabelField("NavMesh", EditorStyles.boldLabel);
			//PropertyField(m_MainCamera);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// NavMesh Manager
// ====================================================================================================

public class NavMeshManager : MonoSingleton<NavMeshManager> {

	[SerializeField] List<NavMeshSurface> navMeshSurfaces = new();



	public enum Hitbox {
		None = 0,
		S   = -1923039037,
		M   =  -902729914,
		L   =   287145453,
		XL  =   658490984,
		XXL =    65107623,
	}
	static readonly Dictionary<Hitbox, Vector2> HITBOX = new() {
		{ Hitbox.None, new Vector2(0.00f, 0.00f) },
		{ Hitbox.S,    new Vector2(0.00f, 0.00f) },
		{ Hitbox.M,    new Vector2(0.00f, 0.00f) },
		{ Hitbox.L,    new Vector2(0.00f, 0.00f) },
		{ Hitbox.XL,   new Vector2(0.00f, 0.00f) },
		{ Hitbox.XXL,  new Vector2(0.00f, 0.00f) },
	};

	

	public void Bake() {
		foreach (var navMeshSurface in navMeshSurfaces) navMeshSurface.BuildNavMesh();
	}

	public void Clear() {
		foreach (var navMeshSurface in navMeshSurfaces) navMeshSurface.RemoveData();
	}
}

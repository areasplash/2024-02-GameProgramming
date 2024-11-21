using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



[Serializable] public enum HitboxType {
	Humanoid,
}

[Serializable] public struct HitboxData {
	public int agentTypeID;
	public float radius;
	public float height;
}



// ====================================================================================================
// NavMesh Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(NavMeshManager)), CanEditMultipleObjects]
	public class NavMeshManagerEditor : Editor {

		SerializedProperty m_HumanoidMesh;
		SerializedProperty m_SampleDistance;

		NavMeshManager I => target as NavMeshManager;

		void OnEnable() {
			m_HumanoidMesh   = serializedObject.FindProperty("m_HumanoidMesh");
			m_SampleDistance = serializedObject.FindProperty("m_SampleDistance");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change NavMesh Manager Properties");
			Space();
			LabelField("NavMesh", EditorStyles.boldLabel);
			PropertyField(m_HumanoidMesh);
			BeginHorizontal();
			{
				PrefixLabel("Bake NavMesh");
				if (GUILayout.Button("Bake" )) I.Bake ();
				if (GUILayout.Button("Clear")) I.Clear();
			}
			EndHorizontal();
			Space();
			LabelField("NavMesh Properties", EditorStyles.boldLabel);
			I.SampleDistance = Slider("Sample Distance", I.SampleDistance, 1f, 16f);
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

	// Fields

	[SerializeField] NavMeshSurface m_HumanoidMesh;

	[SerializeField] float m_SampleDistance = 1f;

	

	// Properties

	public float SampleDistance {
		get => m_SampleDistance;
		set => m_SampleDistance = value;
	}



	// Methods

	public void Bake() {
		m_HumanoidMesh.BuildNavMesh();
	}

	public void Clear() {
		m_HumanoidMesh.RemoveData();
	}



	Dictionary<HitboxType, HitboxData> hitboxData = new Dictionary<HitboxType, HitboxData>();

	public HitboxData GetHitboxData(HitboxType hitbox) {
		if (!hitboxData.ContainsKey(hitbox)) hitboxData[hitbox] = hitbox switch {
			HitboxType.Humanoid => new HitboxData() {
				agentTypeID = m_HumanoidMesh.agentTypeID,
				radius      = m_HumanoidMesh.GetBuildSettings().agentRadius,
				height      = m_HumanoidMesh.GetBuildSettings().agentHeight,
			},
			_ => throw new NotImplementedException(),
		};
		return hitboxData[hitbox];
	}



	NavMeshPath path;
	NavMeshHit  hit;

	public bool FindPath(Vector3 start, Vector3 end, ref Queue<Vector3> queue, float offset = 1f) {
		path ??= new NavMeshPath();
		if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path)) {
			queue.Clear();
			for (int i = 0; i < path.corners.Length - 1; i++) {
				float s = Vector3.Distance(path.corners[i], path.corners[i + 1]);
				for (float j = 0; j < s; j += m_SampleDistance) {
					Vector3 point = Vector3.Lerp(path.corners[i], path.corners[i + 1], j / s);
					if (NavMesh.SamplePosition(point, out hit, offset * 2, NavMesh.AllAreas)) {
						queue.Enqueue(new Vector3(point.x, hit.position.y + offset, point.z));
					}
				}
			}
			queue.Enqueue(new Vector3(end.x, end.y + offset, end.z));
			return true;
		}
		else return false;
	}
}

using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
	using static NavMeshManager;
#endif



[Serializable] public enum HitboxType {
	Humanoid,
	Item,
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
	public class NavMeshManagerEditor : ExtendedEditor {

		SerializedProperty m_HumanoidMesh;
		SerializedProperty m_ItemMesh;

		void OnEnable() {
			m_HumanoidMesh = serializedObject.FindProperty("m_HumanoidMesh");
			m_ItemMesh     = serializedObject.FindProperty("m_ItemMesh");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change NavMesh Manager Properties");

			LabelField("NavMesh", EditorStyles.boldLabel);
			PropertyField(m_HumanoidMesh);
			PropertyField(m_ItemMesh);
			BeginHorizontal();
			{
				PrefixLabel("Bake NavMesh");
				if (GUILayout.Button("Bake" )) Bake ();
				if (GUILayout.Button("Clear")) Clear();
			}
			EndHorizontal();
			Space();

			LabelField("NavMesh Properties", EditorStyles.boldLabel);
			SampleDistance = Slider("Sample Distance", SampleDistance, 1f, 16f);
			/*if (GUILayout.Button("Test")) {
				for (int i = 0; i < m_NavMeshSurfaces.arraySize; i++) {
					var navMeshSurface = m_NavMeshSurfaces.GetArrayElementAtIndex(i)
						.objectReferenceValue as NavMeshSurface;
					Debug.Log(navMeshSurface.agentTypeID);
				}
			}*/
			Space();
			
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
	[SerializeField] NavMeshSurface m_ItemMesh;

	[SerializeField] float m_SampleDistance = 1f;

	

	// Properties

	static NavMeshSurface HumanoidMesh => Instance? Instance.m_HumanoidMesh : default;
	static NavMeshSurface ItemMesh     => Instance? Instance.m_ItemMesh     : default;

	public static float SampleDistance {
		get   =>  Instance? Instance.m_SampleDistance : default;
		set { if (Instance) Instance.m_SampleDistance = value; }
	}



	// Methods

	public static void Bake() {
		if (HumanoidMesh) HumanoidMesh.BuildNavMesh();
	}

	public static void Clear() {
		if (HumanoidMesh) HumanoidMesh.RemoveData();
	}



	static Dictionary<HitboxType, HitboxData> hitboxData = new Dictionary<HitboxType, HitboxData>();

	public static HitboxData GetHitboxData(HitboxType hitbox) {
		if (!hitboxData.ContainsKey(hitbox)) hitboxData[hitbox] = hitbox switch {
			HitboxType.Humanoid => HumanoidMesh? new HitboxData() {
					agentTypeID = HumanoidMesh.agentTypeID,
					radius      = HumanoidMesh.GetBuildSettings().agentRadius,
					height      = HumanoidMesh.GetBuildSettings().agentHeight,
				} : default,
			HitboxType.Item => ItemMesh? new HitboxData() {
					agentTypeID = ItemMesh.agentTypeID,
					radius      = ItemMesh.GetBuildSettings().agentRadius,
					height      = ItemMesh.GetBuildSettings().agentHeight,
				} : default,
			_ => throw new NotImplementedException(),
		};
		return hitboxData[hitbox];
	}



	static NavMeshPath path;

	public static bool FindPath(Vector3 start, Vector3 end, ref Queue<Vector3> queue, float offset) {
		path ??= new NavMeshPath();
		NavMeshHit hit;
		NavMesh.SamplePosition(start, out hit, 8f, NavMesh.AllAreas);
		start = hit.position;
		NavMesh.SamplePosition(end,   out hit, 8f, NavMesh.AllAreas);
		end   = hit.position;
		queue.Clear();
		if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path)) {
			for (int i = 0; i < path.corners.Length - 1; i++) {
				float s = Vector3.Distance(path.corners[i], path.corners[i + 1]);
				for (float j = 0; j < s; j += SampleDistance) {
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

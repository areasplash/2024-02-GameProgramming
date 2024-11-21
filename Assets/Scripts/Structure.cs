using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Structure Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(Structure)), CanEditMultipleObjects]
	public class StructureEditor : Editor {

		SerializedProperty m_StructureType;

		Structure I => target as Structure;

		void OnEnable() {
			m_StructureType = serializedObject.FindProperty("m_StructureType");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Space();
			LabelField("Structure", EditorStyles.boldLabel);
			PropertyField(m_StructureType);
			serializedObject.ApplyModifiedProperties();
		}
	}
#endif



// ====================================================================================================
// Structure
// ====================================================================================================

[Serializable] public enum StructureType {
	None,
	Table,
	Chair,
}



public class Structure : MonoBehaviour {

	// Constants

	const string PrefabPath = "Prefabs/Structure";



	// Fields

	[SerializeField] StructureType m_StructureType;



	// Properties

	public StructureType structureType {
		get => m_StructureType;
		set => m_StructureType = value;
	}



	// Methods

	static List<Structure> structureList = new List<Structure>();
	static List<Structure> structurePool = new List<Structure>();
	static Structure structurePrefab;
	static Structure structure;



	public static Structure Spawn(Vector3 position) {
		if (structurePool.Count == 0) {
			if (!structurePrefab) structurePrefab = Resources.Load<Structure>(PrefabPath);
			structure = Instantiate(structurePrefab);
		}
		else {
			int i = structurePool.Count - 1;
			structure = structurePool[i];
			structure.gameObject.SetActive(true);
			structurePool.RemoveAt(i);
		}
		structureList.Add(structure);
		structure.transform.position = position;
		return structure;
	}

	public static void Despawn(Structure structure) {
		structureList.Remove(structure);
		structurePool.Add   (structure);
		structure.gameObject.SetActive(false);
	}

	void OnRemove() {
		structureList.Remove(this);
		structurePool.Add   (this);
	}



	// Lifecycle

	void OnStart() {
		int layer = 0;
		RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.5f, Vector3.down, 0.0f);
		for (int i = 0; i < hits.Length; i++) if (hits[i].collider.gameObject != gameObject) {
			int hitLayer = hits[i].collider.gameObject.layer;
			if (layer < hitLayer) layer = hitLayer;
		}
		gameObject.layer = layer;
	}



	void Start() {
		OnStart();
	}

	void OnDestory() {
		OnRemove();
	}
}

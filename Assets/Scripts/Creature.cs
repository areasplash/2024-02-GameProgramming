using UnityEngine;

using System;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Creature Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(Creature)), CanEditMultipleObjects]
	public class CreatureEditor : Editor {

		SerializedProperty m_CreatureType;
		SerializedProperty m_AnimationType;

		Creature I => target as Creature;

		void OnEnable() {
			m_CreatureType  = serializedObject.FindProperty("m_CreatureType");
			m_AnimationType = serializedObject.FindProperty("m_AnimationType");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Creature Properties");
			Space();
			LabelField("Creature", EditorStyles.boldLabel);
			PropertyField(m_CreatureType);
			PropertyField(m_AnimationType);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Creature
// ====================================================================================================

[Serializable] public enum AnimationType {
	Idle,
	Move,
	Attack,
	Dead,
}

[Serializable] public enum CreatureType {
	None,
	Player,
}



public class Creature : MonoBehaviour {

	// Fields

	[SerializeField] CreatureType  m_CreatureType  = CreatureType.None;
	[SerializeField] AnimationType m_AnimationType = AnimationType.Idle;



	// Properties

	public CreatureType CreatureType {
		get         => m_CreatureType;
		private set => m_CreatureType = value;
	}

	public AnimationType AnimationType {
		get         => m_AnimationType;
		private set => m_AnimationType = value;
	}

	public float Offset { get; private set; }

	void Update() => Offset += Time.deltaTime;



	public int   CurrentMask  { get; private set; }
	public float LayerOpacity { get; set; }



	// Methods



	// Lifecycle

	void BeginDetectLayer() {
		CurrentMask = 0;
	}

	void DetectLayer(Collider collider) {
		if (collider.isTrigger) CurrentMask |= 1 << collider.gameObject.layer;
	}

	void EndDetectLayer() {
	}



	void FixedUpdate() {
		EndDetectLayer();
		BeginDetectLayer();
	}

	void OnTriggerStay(Collider collider) {
		DetectLayer(collider);
	}
}

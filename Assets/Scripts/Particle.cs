using UnityEngine;

using System;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Particle Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(Particle)), CanEditMultipleObjects]
	public class ParticleEditor : Editor {

		//SerializedProperty m_MainCamera;

		Particle I => target as Particle;

		void OnEnable() {
			//m_MainCamera     = serializedObject.FindProperty("m_MainCamera");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Particle Properties");
			Space();
			LabelField("Particle", EditorStyles.boldLabel);
			//PropertyField(m_MainCamera);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Particle
// ====================================================================================================

[Serializable] public enum ParticleType {
	None,
}



public class Particle : MonoBehaviour {

	

}

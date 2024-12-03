using UnityEngine;

using System;

#if UNITY_EDITOR
	using UnityEditor;
#endif



#if UNITY_EDITOR
	public class ExtendedEditor : Editor {

		public void Begin(string className) {
			serializedObject.Update();
			Undo.RecordObject(target, $"Change {className} Properties");
		}

		public void End() {
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}



		public void LabelField(string label) {
			EditorGUILayout.LabelField(label);
		}

		public void LabelField(string label, GUIStyle style) {
			EditorGUILayout.LabelField(label, style);
		}

		public void PrefixLabel(string label) {
			EditorGUILayout.PrefixLabel(label);
		}

		public void Space() {
			EditorGUILayout.Space();
		}

		public void BeginHorizontal() {
			EditorGUILayout.BeginHorizontal();
		}

		public void EndHorizontal() {
			EditorGUILayout.EndHorizontal();
		}



		public void PropertyField(string name) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
		}

		public int IntField(string label, int value) {
			return EditorGUILayout.IntField(label, value);
		}

		public float FloatField(string label, float value) {
			return EditorGUILayout.FloatField(label, value);
		}

		public float Slider(string label, float value, float min, float max) {
			return EditorGUILayout.Slider(label, value, min, max);
		}

		public int IntSlider(string label, int value, int min, int max) {
			return EditorGUILayout.IntSlider(label, value, min, max);
		}

		public bool Toggle(string label, bool value) {
			return EditorGUILayout.Toggle(label, value);
		}

		public bool ToggleLeft(string label, bool value, params GUILayoutOption[] options) {
			return EditorGUILayout.ToggleLeft(label, value, options);
		}

		public Vector3 Vector3Field(string label, Vector3 value) {
			return EditorGUILayout.Vector3Field(label, value);
		}

		public Vector2 Vector2Field(string label, Vector2 value) {
			return EditorGUILayout.Vector2Field(label, value);
		}

		public Vector3Int Vector3IntField(string label, Vector3Int value) {
			return EditorGUILayout.Vector3IntField(label, value);
		}

		public Vector2Int Vector2IntField(string label, Vector2Int value) {
			return EditorGUILayout.Vector2IntField(label, value);
		}

		public Color ColorField(string label, Color value) {
			return EditorGUILayout.ColorField(label, value);
		}

		public string TextField(string label, string value) {
			return EditorGUILayout.TextField(label, value);
		}

		public T EnumField<T>(string label, T value) where T : Enum {
			return (T)EditorGUILayout.EnumPopup(label, value);
		}

		public T FlagField<T>(string label, T value) where T : Enum {
			return (T)EditorGUILayout.EnumFlagsField(label, value);
		}

		static string[] layerNames;
		static string[] LayerNames {
			get {
				if (layerNames == null) {
					layerNames = new string[32];
					for (int i = 0; i < 32; i++) layerNames[i] = LayerMask.LayerToName(i);
				}
				return layerNames;
			}
		}

		public int MaskField(string label, int layer) {
			return EditorGUILayout.MaskField(label, layer, LayerNames);
		}

		public T ObjectField<T>(string label, T value) where T : UnityEngine.Object {
			return (T)EditorGUILayout.ObjectField(label, value, typeof(T), true);
		}
	}
#endif

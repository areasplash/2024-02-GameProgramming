using UnityEngine;

using System;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ====================================================================================================
// Extended Editor
// ====================================================================================================

public class ExtendedEditor : Editor {

	public static T EnumField<T>(string label, T value) where T : Enum {
		return (T)EditorGUILayout.EnumPopup(label, value);
	}

	public static T FlagField<T>(string label, T value) where T : Enum {
		return (T)EditorGUILayout.EnumFlagsField(label, value);
	}



	static string[] layerNames;

	public static int LayerMaskField(string label, int layer) {
		if (layerNames == null) {
			layerNames = new string[32];
			for (int i = 0; i < 32; i++) layerNames[i] = LayerMask.LayerToName(i);
		}
		return EditorGUILayout.MaskField(label, layer, layerNames);
	}



	public static T ObjectField<T>(string label, T value) where T : UnityEngine.Object {
		return (T)EditorGUILayout.ObjectField(label, value, typeof(T), true);
	}
}

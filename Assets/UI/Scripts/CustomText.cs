using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;

using UnityEngine.Localization.Components;
using TMPro;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UI;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Custom Text Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(CustomText)), CanEditMultipleObjects]
	public class CustomTextEditor : SelectableEditor {

		SerializedProperty m_TextTMP;
		SerializedProperty m_LocalizeStringEvent;
		SerializedProperty m_OnStateUpdated;

		CustomText I => target as CustomText;

		protected override void OnEnable() {
			base.OnEnable();
			m_TextTMP             = serializedObject.FindProperty("m_TextTMP");
			m_LocalizeStringEvent = serializedObject.FindProperty("m_LocalizeStringEvent");
			m_OnStateUpdated      = serializedObject.FindProperty("m_OnStateUpdated");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Undo.RecordObject(target, "Custom Text Properties");
			Space();
			PropertyField(m_TextTMP);
			PropertyField(m_LocalizeStringEvent);
			Space();
			PropertyField(m_OnStateUpdated);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif


 
// ====================================================================================================
// Custom Text
// ====================================================================================================

[RequireComponent(typeof(LocalizeStringEvent))]
public class CustomText : Selectable {

	[Serializable] public class TextUpdatedEvent : UnityEvent<CustomText> {}



	// Fields

	[SerializeField] TextMeshProUGUI     m_TextTMP;
	[SerializeField] LocalizeStringEvent m_LocalizeStringEvent;

	[SerializeField] TextUpdatedEvent m_OnStateUpdated = new TextUpdatedEvent();



	// Properties

	RectTransform RectTransform => transform as RectTransform;

	LocalizeStringEvent LocalizeStringEvent => m_LocalizeStringEvent;

	public string Text {
		get => m_TextTMP ? m_TextTMP.text : string.Empty;
		set {
			if (LocalizeStringEvent) LocalizeStringEvent.StringReference.Clear();
			if (m_TextTMP) m_TextTMP.text = value;
		}
	}

	public TextUpdatedEvent OnStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}



	// Methods

	public void SetLocalizeText(string table, string tableEntry) {
		if (LocalizeStringEvent) LocalizeStringEvent.StringReference.SetReference(table, tableEntry);
	}

	public void Refresh() {
		OnStateUpdated?.Invoke(this);
	}



	// Lifecycle

	protected override void OnEnable() {
		base.OnEnable();
		Refresh();
	}
}

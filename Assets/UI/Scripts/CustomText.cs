using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;

using UnityEngine.Localization;
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
		SerializedProperty m_OnStateUpdated;

		CustomText I => target as CustomText;

		protected override void OnEnable() {
			base.OnEnable();
			m_TextTMP        = serializedObject.FindProperty("m_TextTMP");
			m_OnStateUpdated = serializedObject.FindProperty("m_OnStateUpdated");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Space();
			PropertyField(m_TextTMP);
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

	[SerializeField] TextMeshProUGUI m_TextTMP;

	[SerializeField] TextUpdatedEvent m_OnStateUpdated = new TextUpdatedEvent();



	// Properties

	RectTransform rectTransform => transform as RectTransform;

	LocalizeStringEvent localizeStringEvent;

	public string text {
		get => m_TextTMP ? m_TextTMP.text : string.Empty;
		set {
			if (localizeStringEvent || TryGetComponent(out localizeStringEvent)) {
				localizeStringEvent.StringReference = null;
			}
			if (m_TextTMP) m_TextTMP.text = value;
		}
	}

	public TextUpdatedEvent onStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}



	// Methods

	public void SetLocalizeText(string table, string tableEntry) {
		if (localizeStringEvent || TryGetComponent(out localizeStringEvent)) {
			localizeStringEvent.StringReference = new LocalizedString {
				TableReference      = table,
				TableEntryReference = tableEntry
			};
			localizeStringEvent.RefreshString();
		}
	}

	public void Refresh() {
		onStateUpdated?.Invoke(this);
	}



	// Cycle

	protected override void OnEnable() {
		base.OnEnable();
		Refresh();
	}
}

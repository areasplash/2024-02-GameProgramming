using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using System;

using TMPro;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UI;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Settings Toggle Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(SettingsToggle)), CanEditMultipleObjects]
	public class SettingsToggleEditor : SelectableEditor {

		SerializedProperty m_PositiveRect;
		SerializedProperty m_NegativeRect;
		SerializedProperty m_PositiveTextTMP;
		SerializedProperty m_NegativeTextTMP;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnValueChanged;

		SettingsToggle I => target as SettingsToggle;

		protected override void OnEnable() {
			base.OnEnable();
			m_PositiveRect    = serializedObject.FindProperty("m_PositiveRect");
			m_NegativeRect    = serializedObject.FindProperty("m_NegativeRect");
			m_PositiveTextTMP = serializedObject.FindProperty("m_PositiveTextTMP");
			m_NegativeTextTMP = serializedObject.FindProperty("m_NegativeTextTMP");
			m_OnStateUpdated  = serializedObject.FindProperty("m_OnStateUpdated");
			m_OnValueChanged  = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Undo.RecordObject(target, "Settings Toggle Properties");
			Space();
			PropertyField(m_PositiveRect);
			PropertyField(m_NegativeRect);
			PropertyField(m_PositiveTextTMP);
			PropertyField(m_NegativeTextTMP);
			Space();
			I.Value = Toggle("Value", I.Value);
			Space();
			PropertyField(m_OnStateUpdated);
			PropertyField(m_OnValueChanged);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Settings Toggle
// ====================================================================================================

public class SettingsToggle : Selectable, IPointerClickHandler {

	[Serializable] public class ToggleUpdatedEvent : UnityEvent<SettingsToggle> {}
	[Serializable] public class ToggleChangedEvent : UnityEvent<bool> {}



	// Fields

	[SerializeField] RectTransform   m_PositiveRect;
	[SerializeField] RectTransform   m_NegativeRect;
	[SerializeField] TextMeshProUGUI m_PositiveTextTMP;
	[SerializeField] TextMeshProUGUI m_NegativeTextTMP;

	[SerializeField] bool m_Value = false;

	[SerializeField] ToggleUpdatedEvent m_OnStateUpdated = new ToggleUpdatedEvent();
	[SerializeField] ToggleChangedEvent m_OnValueChanged = new ToggleChangedEvent();



	// Properties

	RectTransform RectTransform => transform as RectTransform;

	public bool Value {
		get => m_Value;
		set {
			if (m_Value == value) return;
			m_Value = value;
			OnValueChanged?.Invoke(m_Value);
			Refresh();
		}
	}

	public ToggleUpdatedEvent OnStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public ToggleChangedEvent OnValueChanged {
		get => m_OnValueChanged;
		set => m_OnValueChanged = value;
	}



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) Value = !Value;
	}

	public void OnSubmit() {
		if (interactable) {
			DoStateTransition(SelectionState.Pressed, false);
			Value = !Value;
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				Value = !Value;
				return;
		}
		base.OnMove(eventData);
	}



	Transform  parent;
	ScrollRect scrollRect;

	bool TryGetComponentInParent<T>(out T component) where T : Component {
		parent = transform.parent;
		while (parent) {
			if (parent.TryGetComponent(out component)) return true;
			else parent = parent.parent;
		}
		component = null;
		return false;
	}

	public override void OnSelect(BaseEventData eventData) {
		base.OnSelect(eventData);
		if (eventData is AxisEventData) {
			if (scrollRect || TryGetComponentInParent(out scrollRect)) {
				Vector2 anchoredPosition = scrollRect.content.anchoredPosition;
				float pivot = RectTransform.rect.height / 2 - RectTransform.anchoredPosition.y;
				anchoredPosition.y = pivot - scrollRect.viewport.rect.height / 2;
				scrollRect.content.anchoredPosition = anchoredPosition;
			}
		}
	}

	

	public void Refresh() {
		if (m_PositiveRect) m_PositiveRect.gameObject.SetActive( Value);
		if (m_NegativeRect) m_NegativeRect.gameObject.SetActive(!Value);
		if (m_PositiveTextTMP) m_PositiveTextTMP.gameObject.SetActive( Value);
		if (m_NegativeTextTMP) m_NegativeTextTMP.gameObject.SetActive(!Value);
		OnStateUpdated?.Invoke(this);
	}



	// Lifecycle

	protected override void OnEnable() {
		base.OnEnable();
		Refresh();
	}
}

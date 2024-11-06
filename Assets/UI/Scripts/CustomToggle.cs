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
// Custom Toggle Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(CustomToggle)), CanEditMultipleObjects]
	public class CustomToggleEditor : SelectableEditor {

		SerializedProperty m_PositiveRect;
		SerializedProperty m_NegativeRect;
		SerializedProperty m_PositiveTextTMP;
		SerializedProperty m_NegativeTextTMP;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnValueChanged;

		CustomToggle I => target as CustomToggle;

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
			Space();
			PropertyField(m_PositiveRect);
			PropertyField(m_NegativeRect);
			PropertyField(m_PositiveTextTMP);
			PropertyField(m_NegativeTextTMP);
			Space();
			I.value = Toggle("Value", I.value);
			Space();
			PropertyField(m_OnStateUpdated);
			PropertyField(m_OnValueChanged);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Custom Toggle
// ====================================================================================================

public class CustomToggle : Selectable, IPointerClickHandler {

	[Serializable] public class ToggleUpdatedEvent : UnityEvent<CustomToggle> {}
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

	RectTransform rectTransform => transform as RectTransform;

	public bool value {
		get => m_Value;
		set {
			if (m_Value == value) return;
			m_Value = value;
			onValueChanged?.Invoke(m_Value);
			Refresh();
		}
	}

	public ToggleUpdatedEvent onStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public ToggleChangedEvent onValueChanged {
		get => m_OnValueChanged;
		set => m_OnValueChanged = value;
	}



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) value = !value;
	}

	public void OnSubmit() {
		if (interactable) {
			DoStateTransition(SelectionState.Pressed, false);
			value = !value;
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				value = !value;
				return;
		}
		base.OnMove(eventData);
	}

	ScrollRect scrollRect;

	bool TryGetComponentInParent<T>(out T component) where T : Component {
		Transform parent = transform.parent;
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
				float pivot = rectTransform.rect.height / 2 - rectTransform.anchoredPosition.y;
				anchoredPosition.y = pivot - scrollRect.viewport.rect.height / 2;
				scrollRect.content.anchoredPosition = anchoredPosition;
			}
		}
	}

	public void Refresh() {
		if (m_PositiveRect) m_PositiveRect.gameObject.SetActive( value);
		if (m_NegativeRect) m_NegativeRect.gameObject.SetActive(!value);
		if (m_PositiveTextTMP) m_PositiveTextTMP.gameObject.SetActive( value);
		if (m_NegativeTextTMP) m_NegativeTextTMP.gameObject.SetActive(!value);
		onStateUpdated?.Invoke(this);
	}



	// Cycle

	protected override void OnEnable() {
		base.OnEnable();
		Refresh();
	}
}

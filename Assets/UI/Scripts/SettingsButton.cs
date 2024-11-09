using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using UnityEngine.Localization.Components;
using TMPro;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UI;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Custom Button Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(SettingsButton)), CanEditMultipleObjects]
	public class SettingsButtonEditor : SelectableEditor {

		SerializedProperty m_TextTMP;
		SerializedProperty m_LocalizeStringEvent;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnClick;

		SettingsButton I => target as SettingsButton;

		protected override void OnEnable() {
			base.OnEnable();
			m_TextTMP             = serializedObject.FindProperty("m_TextTMP");
			m_LocalizeStringEvent = serializedObject.FindProperty("m_LocalizeStringEvent");
			m_OnStateUpdated      = serializedObject.FindProperty("m_OnStateUpdated");
			m_OnClick             = serializedObject.FindProperty("m_OnClick");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Space();
			PropertyField(m_TextTMP);
			PropertyField(m_LocalizeStringEvent);
			Space();
			PropertyField(m_OnStateUpdated);
			PropertyField(m_OnClick);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Custom Button
// ====================================================================================================

public class SettingsButton : Selectable, IPointerClickHandler {

	[Serializable] public class ButtonUpdatedEvent : UnityEvent<SettingsButton> {}
	[Serializable] public class ButtonClickedEvent : UnityEvent {}



	// Serialized Fields

	[SerializeField] TextMeshProUGUI     m_TextTMP;
	[SerializeField] LocalizeStringEvent m_LocalizeStringEvent;

	[SerializeField] ButtonUpdatedEvent m_OnStateUpdated = new ButtonUpdatedEvent();
	[SerializeField] ButtonClickedEvent m_OnClick        = new ButtonClickedEvent();



	// Properties

	RectTransform RectTransform => transform as RectTransform;

	LocalizeStringEvent LocalizeStringEvent => m_LocalizeStringEvent;

	public string Text {
		get => m_TextTMP ? m_TextTMP.text : string.Empty;
		set {
			if (LocalizeStringEvent) LocalizeStringEvent.StringReference = null;
			if (m_TextTMP) m_TextTMP.text = value;
		}
	}

	public ButtonUpdatedEvent OnStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public ButtonClickedEvent OnClick {
		get => m_OnClick;
		set => m_OnClick = value;
	}



	// Cached Variables

	Transform  parent;
	ScrollRect scrollRect;



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) OnClick?.Invoke();
	}

	public void OnSubmit() {
		if (interactable) {
			DoStateTransition(SelectionState.Pressed, false);
			OnClick?.Invoke();
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				OnClick?.Invoke();
				return;
		}
		base.OnMove(eventData);
	}

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

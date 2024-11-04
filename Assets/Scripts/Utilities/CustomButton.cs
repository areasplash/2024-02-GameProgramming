using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
// Custom Button Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(CustomButton)), CanEditMultipleObjects]
	public class CustomButtonEditor : SelectableEditor {

		SerializedProperty m_TextTMP;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnClick;

		CustomButton I => target as CustomButton;

		protected override void OnEnable() {
			base.OnEnable();
			m_TextTMP        = serializedObject.FindProperty("m_TextTMP");
			m_OnStateUpdated = serializedObject.FindProperty("m_OnStateUpdated");
			m_OnClick        = serializedObject.FindProperty("m_OnClick");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Space();
			PropertyField(m_TextTMP);
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

public class CustomButton : Selectable, IPointerClickHandler {

	[Serializable] public class ButtonUpdatedEvent : UnityEvent<CustomButton> {}
	[Serializable] public class ButtonClickedEvent : UnityEvent {}



	// Fields

	[SerializeField] TextMeshProUGUI m_TextTMP;

	[SerializeField] ButtonUpdatedEvent m_OnStateUpdated = new ButtonUpdatedEvent();
	[SerializeField] ButtonClickedEvent m_OnClick        = new ButtonClickedEvent();



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

	public ButtonUpdatedEvent onStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public ButtonClickedEvent onClick {
		get => m_OnClick;
		set => m_OnClick = value;
	}



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) onClick?.Invoke();
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				onClick?.Invoke();
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

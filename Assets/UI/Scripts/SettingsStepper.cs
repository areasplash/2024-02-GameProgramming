using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
// Settings Stepper Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(SettingsStepper)), CanEditMultipleObjects]
	public class SettingsStepperEditor : SelectableEditor {

		SerializedProperty m_PrevRect;
		SerializedProperty m_NextRect;
		SerializedProperty m_TextTMP;
		SerializedProperty m_LocalizeStringEvent;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnValueChanged;

		SettingsStepper I => target as SettingsStepper;

		protected override void OnEnable() {
			base.OnEnable();
			m_PrevRect            = serializedObject.FindProperty("m_PrevRect");
			m_NextRect            = serializedObject.FindProperty("m_NextRect");
			m_TextTMP             = serializedObject.FindProperty("m_TextTMP");
			m_LocalizeStringEvent = serializedObject.FindProperty("m_LocalizeStringEvent");
			m_OnStateUpdated      = serializedObject.FindProperty("m_OnStateUpdated");
			m_OnValueChanged      = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Undo.RecordObject(target, "Settings Stepper Properties");
			Space();
			PropertyField(m_PrevRect);
			PropertyField(m_NextRect);
			PropertyField(m_TextTMP);
			PropertyField(m_LocalizeStringEvent);
			Space();
			I.ActivatePrev = Toggle("Can Move Prev", I.ActivatePrev);
			I.ActivateNext = Toggle("Can Move Next", I.ActivateNext);
			Space();
			PropertyField(m_OnStateUpdated);
			PropertyField(m_OnValueChanged);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Settings Stepper
// ====================================================================================================

public class SettingsStepper : Selectable, IPointerClickHandler {

	[Serializable] public class StepperUpdatedEvent : UnityEvent<SettingsStepper> {}
	[Serializable] public class StepperChangedEvent : UnityEvent<int> {}



	// Fields

	[SerializeField] RectTransform       m_PrevRect;
	[SerializeField] RectTransform       m_NextRect;
	[SerializeField] TextMeshProUGUI     m_TextTMP;
	[SerializeField] LocalizeStringEvent m_LocalizeStringEvent;

	[SerializeField] StepperUpdatedEvent m_OnStateUpdated;
	[SerializeField] StepperChangedEvent m_OnValueChanged;



	// Properties

	RectTransform RectTransform => transform as RectTransform;
	
	LocalizeStringEvent LocalizeStringEvent => m_LocalizeStringEvent;

	public bool ActivatePrev {
		get => m_PrevRect && m_PrevRect.gameObject.activeSelf;
		set {
			if (m_PrevRect) m_PrevRect.gameObject.SetActive(value);
		}
	}

	public bool ActivateNext {
		get => m_NextRect && m_NextRect.gameObject.activeSelf;
		set {
			if (m_NextRect) m_NextRect.gameObject.SetActive(value);
		}
	}

	public string Text {
		get => m_TextTMP ? m_TextTMP.text : string.Empty;
		set {
			if (LocalizeStringEvent) LocalizeStringEvent.StringReference.Clear();
			if (m_TextTMP) m_TextTMP.text = value;
		}
	}

	public int Value {
		get => 0;
		set {
			OnValueChanged?.Invoke(value);
			Refresh();
		}
	}
	
	public StepperUpdatedEvent OnStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public StepperChangedEvent OnValueChanged {
		get => m_OnValueChanged;
		set => m_OnValueChanged = value;
	}



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = RectTransform.InverseTransformPoint(eventData.position);
			Value = (0 <= point.x) && (point.x < RectTransform.rect.width / 3) ? -1 : 1;
		}
	}

	public void OnSubmit() {
		if (interactable) {
			DoStateTransition(SelectionState.Pressed, false);
			Value = 1;
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
				DoStateTransition(SelectionState.Pressed, false);
				Value = - 1;
				return;
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				Value = + 1;
				return;
		}
		base.OnMove(eventData);
	}



	ScrollRect scrollRect;

	public override void OnSelect(BaseEventData eventData) {
		base.OnSelect(eventData);
		if (eventData is AxisEventData) {
			if (scrollRect || Utility.TryGetComponentInParent(transform, out scrollRect)) {
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

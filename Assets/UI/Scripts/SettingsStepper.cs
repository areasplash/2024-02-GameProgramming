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



public class SettingsStepper : Selectable, IPointerClickHandler {

	[Serializable] public class StepperUpdatedEvent : UnityEvent<SettingsStepper> {}
	[Serializable] public class StepperChangedEvent : UnityEvent<int> {}



	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] RectTransform       m_PrevRect;
	[SerializeField] RectTransform       m_NextRect;
	[SerializeField] TextMeshProUGUI     m_TextTMP;
	[SerializeField] LocalizeStringEvent m_LocalizeStringEvent;
	[SerializeField] StepperUpdatedEvent m_OnStateUpdated;
	[SerializeField] StepperChangedEvent m_OnValueChanged;



	RectTransform PrevRect {
		get => m_PrevRect;
		set => m_PrevRect = value;
	}

	RectTransform NextRect {
		get => m_NextRect;
		set => m_NextRect = value;
	}

	TextMeshProUGUI TextTMP {
		get => m_TextTMP;
		set => m_TextTMP = value;
	}

	LocalizeStringEvent LocalizeStringEvent {
		get => m_LocalizeStringEvent;
		set => m_LocalizeStringEvent = value;
	}

	public StepperUpdatedEvent OnStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public StepperChangedEvent OnValueChanged {
		get => m_OnValueChanged;
		set => m_OnValueChanged = value;
	}



	public bool EnablePrev {
		get   =>  m_PrevRect && m_PrevRect.gameObject.activeSelf;
		set { if (m_PrevRect) m_PrevRect.gameObject.SetActive(value); }
	}

	public bool EnableNext {
		get   =>  m_NextRect && m_NextRect.gameObject.activeSelf;
		set { if (m_NextRect) m_NextRect.gameObject.SetActive(value); }
	}

	public int Value {
		get => 0;
		set {
			OnValueChanged?.Invoke(value);
			Refresh();
		}
	}



	RectTransform Rect => transform as RectTransform;

	public string Text {
		get => m_TextTMP ? m_TextTMP.text : string.Empty;
		set {
			if (LocalizeStringEvent) LocalizeStringEvent.StringReference.Clear();
			if (m_TextTMP) m_TextTMP.text = value;
		}
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(SettingsStepper))] class SettingsStepperEditor : SelectableEditor {

			SerializedProperty m_PrevRect;
			SerializedProperty m_NextRect;
			SerializedProperty m_TextTMP;
			SerializedProperty m_LocalizeStringEvent;
			SerializedProperty m_OnStateUpdated;
			SerializedProperty m_OnValueChanged;

			SettingsStepper i => target as SettingsStepper;

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

				PropertyField(m_PrevRect);
				PropertyField(m_NextRect);
				PropertyField(m_TextTMP);
				PropertyField(m_LocalizeStringEvent);
				Space();

				i.EnablePrev = Toggle("Enable Prev", i.EnablePrev);
				i.EnableNext = Toggle("Enable Next", i.EnableNext);
				Space();

				PropertyField(m_OnStateUpdated);
				PropertyField(m_OnValueChanged);
				Space();

				serializedObject.ApplyModifiedProperties();
				if (GUI.changed) EditorUtility.SetDirty(target);
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = Rect.InverseTransformPoint(eventData.position);
			Value = (0 <= point.x) && (point.x < Rect.rect.width / 3) ? -1 : 1;
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

	bool TryGetComponentInParent<T>(out T component) where T : Component {
		component = null;
		Transform parent = Rect;
		while (parent != null) {
			if  (parent.TryGetComponent(out component)) return true;
			else parent = parent.parent;
		}
		return false;
	}

	public override void OnSelect(BaseEventData eventData) {
		base.OnSelect(eventData);
		if (eventData is AxisEventData) {
			if (scrollRect || TryGetComponentInParent(out scrollRect)) {
				Vector2 anchoredPosition = scrollRect.content.anchoredPosition;
				float pivot = Rect.rect.height / 2 - Rect.anchoredPosition.y;
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



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	protected override void OnEnable() {
		base.OnEnable();
		Refresh();
	}
}

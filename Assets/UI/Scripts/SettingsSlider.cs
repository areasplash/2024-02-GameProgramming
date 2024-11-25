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
// Settings Slider Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(SettingsSlider)), CanEditMultipleObjects]
	public class SettingsSliderEditor : SelectableEditor {

		SerializedProperty m_BodyRect;
		SerializedProperty m_HandleRect;
		SerializedProperty m_TextTMP;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnValueChanged;

		SettingsSlider I => target as SettingsSlider;

		protected override void OnEnable() {
			base.OnEnable();
			m_BodyRect       = serializedObject.FindProperty("m_BodyRect");
			m_HandleRect     = serializedObject.FindProperty("m_HandleRect");
			m_TextTMP        = serializedObject.FindProperty("m_TextTMP");
			m_OnStateUpdated = serializedObject.FindProperty("m_OnStateUpdated");
			m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Undo.RecordObject(target, "Settings Slider Properties");
			Space();
			PropertyField(m_BodyRect);
			PropertyField(m_HandleRect);
			PropertyField(m_TextTMP);
			Space();
			I.MinValue = FloatField("Min Value", I.MinValue);
			I.MaxValue = FloatField("Max Value", I.MaxValue);
			I.Value    = Slider    ("Value",     I.Value,    I.MinValue, I.MaxValue);
			I.Step     = Slider    ("Step",      I.Step,     I.MinValue, I.MaxValue);
			I.FineStep = Slider    ("Fine Step", I.FineStep, I.MinValue, I.MaxValue);
			I.Format   = TextField ("Format",    I.Format);
			TextField(I.Text, "{0} = Value, {1} = Min Value, {2} = Max Value");
			Space();
			PropertyField(m_OnStateUpdated);
			PropertyField(m_OnValueChanged);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Settings Slider
// ====================================================================================================

public class SettingsSlider : Selectable, IPointerClickHandler, IDragHandler {

	[Serializable] public class SliderUpdatedEvent : UnityEvent<SettingsSlider> {}
	[Serializable] public class SliderChangedEvent : UnityEvent<float> {}



	// Fields

	[SerializeField] RectTransform   m_BodyRect;
	[SerializeField] RectTransform   m_HandleRect;
	[SerializeField] TextMeshProUGUI m_TextTMP;

	[SerializeField] float  m_MinValue = 0;
	[SerializeField] float  m_MaxValue = 1;
	[SerializeField] float  m_Value    = 1;
	[SerializeField] float  m_Step     = 0.10f;
	[SerializeField] float  m_FineStep = 0.02f;
	[SerializeField] string m_Format   = "{0:P0}";

	[SerializeField] SliderUpdatedEvent m_OnStateUpdated = new SliderUpdatedEvent();
	[SerializeField] SliderChangedEvent m_OnValueChanged = new SliderChangedEvent();



	// Properties

	RectTransform RectTransform => transform as RectTransform;

	public float MinValue {
		get => m_MinValue;
		set {
			m_MinValue = Mathf.Min(value, MaxValue);
			Value = Value;
		}
	}

	public float MaxValue {
		get => m_MaxValue;
		set {
			m_MaxValue = Mathf.Max(value, MinValue);
			Value = Value;
		}
	}

	public float Value {
		get => m_Value;
		set {
			if (m_Value == value) return;
			m_Value = Mathf.Clamp(value, MinValue, MaxValue);
			OnValueChanged?.Invoke(m_Value);
			Refresh();
		}
	}

	public float Step {
		get => m_Step;
		set => m_Step = Mathf.Clamp(value, 0, MaxValue - MinValue);
	}

	public float FineStep {
		get => m_FineStep;
		set => m_FineStep = Mathf.Clamp(value, 0, Step);
	}

	public string Format {
		get => m_Format;
		set {
			m_Format = value;
			Refresh();
		}
	}

	public SliderUpdatedEvent OnStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public SliderChangedEvent OnValueChanged {
		get => m_OnValueChanged;
		set => m_OnValueChanged = value;
	}

	public string Text => string.Format(Format, Value, MinValue, MaxValue);
	
	float Ratio => (Value - MinValue) / (MaxValue - MinValue);
	int   Width => Mathf.RoundToInt(Ratio * (RectTransform.rect.width - m_HandleRect.rect.width));
	bool  Fine  => InputManager.GetKey(KeyAction.Control);



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable && !eventData.dragging) {
			Vector2 point = RectTransform.InverseTransformPoint(eventData.position);
			if (point.x < Width) Value -= Fine ? FineStep : Step;
			if (Width < point.x) Value += Fine ? FineStep : Step;
		}
	}
	
	public void OnDrag(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = RectTransform.InverseTransformPoint(eventData.position);
			float a = m_HandleRect.rect.width / 2;
			float b = RectTransform.rect.width - m_HandleRect.rect.width / 2;
			Value = Mathf.Lerp(MinValue, MaxValue, Mathf.InverseLerp(a, b, point.x));
		}
	}

	public void OnSubmit() {
		if (interactable) {
			DoStateTransition(SelectionState.Pressed, false);
			Value += Fine ? FineStep : Step;
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
				DoStateTransition(SelectionState.Pressed, false);
				Value -= Fine ? FineStep : Step;
				return;
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				Value += Fine ? FineStep : Step;
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

	

	public void Refresh() {
		if (m_HandleRect) {
			if (m_BodyRect) {
				Vector2 sizeDelta = m_BodyRect.sizeDelta;
				sizeDelta.x = m_HandleRect.rect.width / 2 + Width;
				m_BodyRect.sizeDelta = sizeDelta;
			}
			Vector2 anchoredPosition = m_HandleRect.anchoredPosition;
			anchoredPosition.x = Width;
			m_HandleRect.anchoredPosition = anchoredPosition;
		}
		if (m_TextTMP) m_TextTMP.text = Text;
		OnStateUpdated?.Invoke(this);
	}



	// Lifecycle

	protected override void OnEnable() {
		base.OnEnable();
		Refresh();
	}
}

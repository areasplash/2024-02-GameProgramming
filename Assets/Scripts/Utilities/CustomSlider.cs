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



#if UNITY_EDITOR
	[CustomEditor(typeof(CustomSlider)), CanEditMultipleObjects]
	public class CustomSliderEditor : SelectableEditor {

		SerializedProperty m_BodyRect;
		SerializedProperty m_HandleRect;
		SerializedProperty m_TextTMP;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnValueChanged;

		CustomSlider I => target as CustomSlider;

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
			Space();
			PropertyField(m_BodyRect);
			PropertyField(m_HandleRect);
			PropertyField(m_TextTMP);
			Space();
			I.minValue = FloatField("Min Value", I.minValue);
			I.maxValue = FloatField("Max Value", I.maxValue);
			I.value    = Slider    ("Value",     I.value,    I.minValue, I.maxValue);
			I.step     = Slider    ("Step",      I.step,     I.minValue, I.maxValue);
			I.fineStep = Slider    ("Fine Step", I.fineStep, I.minValue, I.maxValue);
			I.format   = TextField ("Format",    I.format);
			TextField(I.text, "{0} = Value, {1} = Min Value, {2} = Max Value");
			Space();
			PropertyField(m_OnStateUpdated);
			PropertyField(m_OnValueChanged);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Custom Slider
// ====================================================================================================

public class CustomSlider : Selectable, IPointerClickHandler, IDragHandler {

	[Serializable] public class SliderUpdatedEvent : UnityEvent<CustomSlider> {}
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

	RectTransform rectTransform => transform as RectTransform;

	public float minValue {
		get => m_MinValue;
		set {
			m_MinValue = Mathf.Min(value, maxValue);
			this.value = this.value;
		}
	}

	public float maxValue {
		get => m_MaxValue;
		set {
			m_MaxValue = Mathf.Max(value, minValue);
			this.value = this.value;
		}
	}

	public float value {
		get => m_Value;
		set {
			m_Value = Mathf.Clamp(value, minValue, maxValue);
			onValueChanged?.Invoke(m_Value);
			Update();
		}
	}

	public float step {
		get => m_Step;
		set => m_Step = Mathf.Clamp(value, 0, maxValue - minValue);
	}

	public float fineStep {
		get => m_FineStep;
		set => m_FineStep = Mathf.Clamp(value, 0, step);
	}

	public string format {
		get => m_Format;
		set {
			m_Format = value;
			Update();
		}
	}

	public SliderUpdatedEvent onStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public SliderChangedEvent onValueChanged {
		get => m_OnValueChanged;
		set => m_OnValueChanged = value;
	}

	public string text => string.Format(format, value, minValue, maxValue);
	
	float ratio => (value - minValue) / (maxValue - minValue);
	int   width => Mathf.RoundToInt(ratio * (rectTransform.rect.width - m_HandleRect.rect.width));
	bool  fine  => Input.GetKey(KeyCode.LeftShift);



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable && !eventData.dragging) {
			Vector2 point = rectTransform.InverseTransformPoint(eventData.position);
			if (point.x < width) value -= fine ? fineStep : step;
			if (width < point.x) value += fine ? fineStep : step;
		}
	}
	
	public void OnDrag(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = rectTransform.InverseTransformPoint(eventData.position);
			float a = m_HandleRect.rect.width / 2;
			float b = rectTransform.rect.width - m_HandleRect.rect.width / 2;
			value = Mathf.Lerp(minValue, maxValue, Mathf.InverseLerp(a, b, point.x));
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
				DoStateTransition(SelectionState.Pressed, false);
				value -= fine ? fineStep : step;
				return;
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				value += fine ? fineStep : step;
				return;
		}
		base.OnMove(eventData);
	}

	ScrollRect scrollRect;

	bool TryGetComponentInParent<T>(out T component) where T : Component {
		Transform parent = transform.parent;
		while (parent) {
			if (TryGetComponent(out component)) return true;
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

	public void Update() {
		if (m_HandleRect) {
			if (m_BodyRect) {
				Vector2 sizeDelta = m_BodyRect.sizeDelta;
				sizeDelta.x = m_HandleRect.rect.width / 2 + width;
				m_BodyRect.sizeDelta = sizeDelta;
			}
			Vector2 anchoredPosition = m_HandleRect.anchoredPosition;
			anchoredPosition.x = width;
			m_HandleRect.anchoredPosition = anchoredPosition;
		}
		if (m_TextTMP) m_TextTMP.text = text;
		onStateUpdated?.Invoke(this);
	}



	// Cycle

	protected override void OnEnable() {
		base.OnEnable();
		Update();
	}
}

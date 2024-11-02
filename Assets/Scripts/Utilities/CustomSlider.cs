using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UI;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Custom Slider
// ====================================================================================================

public class CustomSlider : Selectable, IPointerClickHandler, IDragHandler {

	[System.Serializable] public class SliderFresh : UnityEvent<CustomSlider> {}
	[System.Serializable] public class SliderEvent : UnityEvent<float> {}



	// Fields

	[SerializeField] RectTransform   bodyRect;
	[SerializeField] RectTransform   handleRect;
	[SerializeField] TextMeshProUGUI textTMP;
	[SerializeField] TextMeshProUGUI labelTMP;

	[SerializeField] float minValue = 0;
	[SerializeField] float maxValue = 1;
	[SerializeField] float value    = 1;
	[SerializeField] float step     = 0.10f;
	[SerializeField] float fineStep = 0.02f;

	[SerializeField] string format = "{0:P0}";

	public SliderFresh onFreshInvoked = new SliderFresh();
	public SliderEvent onValueChanged = new SliderEvent();



	// Properties

	RectTransform   BodyRect   { get => bodyRect;   set => bodyRect   = value; }
	RectTransform   HandleRect { get => handleRect; set => handleRect = value; }
	TextMeshProUGUI TextTMP    { get => textTMP;    set => textTMP    = value; }
	TextMeshProUGUI LabelTMP   { get => labelTMP;   set => labelTMP   = value; }

	float MinValue {
		get => minValue;
		set {
			if (minValue == value) return;
			minValue = Mathf.Min(value, MaxValue);
			Value = Value;
		}
	}

	float MaxValue {
		get => maxValue;
		set {
			if (maxValue == value) return;
			maxValue = Mathf.Max(value, MinValue);
			Value = Value;
		}
	}

	float Value {
		get => value;
		set {
			if (this.value == value) return;
			this.value = Mathf.Clamp(value, MinValue, MaxValue);
			onValueChanged?.Invoke(Value);
			onFreshInvoked?.Invoke(this );
		}
	}

	float Step {
		get => step;
		set => step = Mathf.Clamp(value, 0, MaxValue - MinValue);
	}

	float FineStep {
		get => fineStep;
		set => fineStep = Mathf.Clamp(value, 0, Step);
	}

	string Format {
		get => format;
		set {
			if (format == value) return;
			format = value;
			onFreshInvoked?.Invoke(this);
		}
	}

	string Formatted => string.Format(Format, Value, MinValue, MaxValue);



	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(CustomSlider)), CanEditMultipleObjects]
		public class CustomSliderEditor : SelectableEditor {
			CustomSlider I => target as CustomSlider;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				base.OnInspectorGUI();
				
				Space();
				LabelField("Slider", EditorStyles.boldLabel);
				I.BodyRect    = ObjectField("Body Rect",   I.BodyRect  );
				I.HandleRect  = ObjectField("Handle Rect", I.HandleRect);
				I.TextTMP     = ObjectField("Text TMP",    I.TextTMP   );
				I.LabelTMP    = ObjectField("Label TMP",   I.LabelTMP  );

				Space();
				LabelField("Values", EditorStyles.boldLabel);
				I.MinValue    = FloatField("Value Min",    I.MinValue  );
				I.MaxValue    = FloatField("Value Max",    I.MaxValue  );
				I.Value       = Slider    ("Value",        I.Value,    I.MinValue, I.MaxValue);
				I.Step        = Slider    ("Step",         I.Step,     I.MinValue, I.MaxValue);
				I.FineStep    = Slider    ("Fine Step",    I.FineStep, I.MinValue, I.MaxValue);

				Space();
				LabelField("Format", EditorStyles.boldLabel);
				TextField (I.Formatted, "{0} = Value, {1} = Min Value, {2} = Max Value");
				I.Format      = TextField ("Format",      I.Format      );

				Space();
				PropertyField(serializedObject.FindProperty("onFreshInvoked"));
				PropertyField(serializedObject.FindProperty("onValueChanged"));
				
				if (GUI.changed) EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}
		}
	#endif



	// Methods

	RectTransform Rect => transform as RectTransform;

	float Ratio => (Value - MinValue) / (MaxValue - MinValue);
	int   Width => Mathf.RoundToInt(Ratio * (Rect.rect.width - HandleRect.rect.width));
	bool  Ctrl  => Input.GetKey(KeyCode.LeftControl);

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable && !eventData.dragging) {
			Vector2 point = Rect.InverseTransformPoint(eventData.position);
			if (point.x < Width) Value -= Ctrl ? FineStep : Step;
			if (Width < point.x) Value += Ctrl ? FineStep : Step;
		}
	}
	
	public void OnDrag(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = Rect.InverseTransformPoint(eventData.position);
			float a = HandleRect.rect.width / 2;
			float b = Rect.rect.width - HandleRect.rect.width / 2;
			Value = Mathf.Lerp(MinValue, MaxValue, Mathf.InverseLerp(a, b, point.x));
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
				DoStateTransition(SelectionState.Pressed, false);
				Value -= Ctrl ? FineStep : Step;
				return;
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				Value += Ctrl ? FineStep : Step;
				return;
		}
		base.OnMove(eventData);
	}

	public override void OnSelect(BaseEventData eventData) {
		base.OnSelect(eventData);
		if (eventData is AxisEventData) {
			ScrollRect scrollRect = GetComponentInParent<ScrollRect>();
			if (scrollRect) {
				Vector2 anchoredPosition = scrollRect.content.anchoredPosition;
				anchoredPosition.y = -scrollRect.viewport.rect.height / 2;
				anchoredPosition.y -= Rect.anchoredPosition.y - Rect.rect.height / 2;
				scrollRect.content.anchoredPosition = anchoredPosition;
			}
		}
	}



	protected override void Start() {
		base.Start();
		onFreshInvoked?.Invoke(this);
	}

	public void Fresh() {
		if (BodyRect) {
			Vector2 sizeDelta = BodyRect.sizeDelta;
			sizeDelta.x = HandleRect.rect.width / 2 + Width;
			BodyRect.sizeDelta = sizeDelta;
		}
		if (HandleRect) {
			Vector2 anchoredPosition = HandleRect.anchoredPosition;
			anchoredPosition.x = Width;
			HandleRect.anchoredPosition = anchoredPosition;
		}
		if (TextTMP) TextTMP.text = Formatted;
	}
}

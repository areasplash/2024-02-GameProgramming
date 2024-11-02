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
// Custom Stepper
// ====================================================================================================

public class CustomStepper : Selectable, IPointerClickHandler {

	[System.Serializable] public class StepperRresh : UnityEvent<CustomStepper> {}
	[System.Serializable] public class StepperEvent : UnityEvent<int> {}



	// Fields

	[SerializeField] RectTransform   leftArrowRect;
	[SerializeField] RectTransform   rightArrowRect;
	[SerializeField] TextMeshProUGUI textTMP;
	[SerializeField] TextMeshProUGUI labelTMP;

	[SerializeField] int  length = 1;
	[SerializeField] int  value  = 0;
	[SerializeField] bool loop   = false;

	public StepperRresh onFreshInvoked = new StepperRresh();
	public StepperEvent onValueChanged = new StepperEvent();



	// Properties

	RectTransform   LeftArrowRect  { get => leftArrowRect;  set => leftArrowRect  = value; }
	RectTransform   RightArrowRect { get => rightArrowRect; set => rightArrowRect = value; }
	TextMeshProUGUI TextTMP        { get => textTMP;        set => textTMP        = value; }
	TextMeshProUGUI LabelTMP       { get => labelTMP;       set => labelTMP       = value; }

	public string Text {
		get => TextTMP ? TextTMP.text : "";
		set {
			if (TextTMP) TextTMP.text = value;
		}
	}

	public int Length {
		get => length;
		set {
			if (length == value) return;
			length = Mathf.Max(value, 1);
			Value = Value;
		}
	}

	public int Value {
		get => value;
		set {
			if (this.value == value) return;
			if (Loop) this.value = (int)Mathf.Repeat(value, Length);
			else      this.value =      Mathf.Clamp (value, 0, Length - 1);
			onValueChanged?.Invoke(Value);
			onFreshInvoked?.Invoke(this );
		}
	}

	public bool Loop {
		get => loop;
		set {
			if (loop == value) return;
			loop = value;
			onFreshInvoked?.Invoke(this);
		}
	}



	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(CustomStepper)), CanEditMultipleObjects]
		public class CustomStepperEditor : SelectableEditor {
			CustomStepper I => target as CustomStepper;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				base.OnInspectorGUI();
				
				Space();
				I.LeftArrowRect  = ObjectField("Left Arrow",  I.LeftArrowRect );
				I.RightArrowRect = ObjectField("Right Arrow", I.RightArrowRect);
				I.TextTMP        = ObjectField("Text",        I.TextTMP       );
				I.LabelTMP       = ObjectField("Label",       I.LabelTMP      );

				Space();
				I.Length		 = IntSlider  ("Length",      I.Length,       1, I.Length + 1);
				I.Value          = IntSlider  ("Value",       I.Value,        0, I.Length - 1);
				I.Loop           = Toggle     ("Loop",        I.Loop          );

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

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = Rect.InverseTransformPoint(eventData.position);
			Value += (0 <= point.x) && (point.x < Rect.rect.width / 3) ? -1 : 1;
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
				DoStateTransition(SelectionState.Pressed, false);
				Value = Value - 1;
				return;
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				Value = Value + 1;
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
		if (leftArrowRect ) leftArrowRect .gameObject.SetActive(Loop || 0          < Value);
		if (rightArrowRect) rightArrowRect.gameObject.SetActive(Loop || Value < Length - 1);
	}

	public void SetValueForce(int value) => this.value = value;
}

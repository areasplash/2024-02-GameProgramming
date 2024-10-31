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



public class CustomStepper : Selectable, IPointerClickHandler {

	[System.Serializable] public class StepperEvent : UnityEvent<CustomStepper> {}
	[System.Serializable] public class StepperValue : UnityEvent<int> {}



	// Fields

	[SerializeField] RectTransform   leftArrowRect;
	[SerializeField] RectTransform   rightArrowRect;
	[SerializeField] TextMeshProUGUI textTMP;
	[SerializeField] TextMeshProUGUI labelTMP;

	[SerializeField] int  length = 1;
	[SerializeField] int  value  = 0;
	[SerializeField] bool loop   = false;

	public StepperEvent onRefresh      = new StepperEvent();
	public StepperValue onValueChanged = new StepperValue();



	// Properties

	RectTransform   LeftArrowRect  { get => leftArrowRect;  set => leftArrowRect  = value; }
	RectTransform   RightArrowRect { get => rightArrowRect; set => rightArrowRect = value; }
	TextMeshProUGUI TextTMP        { get => textTMP;        set => textTMP        = value; }
	TextMeshProUGUI LabelTMP       { get => labelTMP;       set => labelTMP       = value; }

	public int Length {
		get => length;
		set {
			length = Mathf.Max(1, value);
			Refresh();
		}
	}

	public int Value {
		get => value;
		set {
			if (Loop) {
				if (value < 0) value = Length - 1;
				if (Length - 1 < value) value = 0;
			}
			this.value = Mathf.Clamp(value, 0, Length - 1);
			onValueChanged?.Invoke(Value);
			Refresh();
		}
	}

	public bool Loop {
		get => loop;
		set {
			loop = value;
			Refresh();
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
				I.Length         = IntField   ("Length",      I.Length        );
				I.Value          = IntSlider  ("Value",       I.Value,        0, I.Length - 1);
				I.Loop           = Toggle     ("Loop",        I.Loop          );
				
				if (GUI.changed) EditorUtility.SetDirty(target);
			}
		}
	#endif



	// Methods

	RectTransform Rect => transform as RectTransform;

	float BodyWidth => Rect.rect.width;

	public void Refresh() {
		if (leftArrowRect ) leftArrowRect .gameObject.SetActive(Loop || 0          < Value);
		if (rightArrowRect) rightArrowRect.gameObject.SetActive(Loop || Value < Length - 1);
		onRefresh?.Invoke(this);
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = Rect.InverseTransformPoint(eventData.position);
			Value += (0 <= point.x) && (point.x < BodyWidth / 3) ? -1 : 1;
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
}
